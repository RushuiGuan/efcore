# Audit

The audit pattern answers three questions about every data change: **what** changed, **when** it changed, and **who** changed it. Each modification or deletion is recorded as a row in a dedicated audit table, giving you a permanent, queryable history of how your data evolved over time.

The pattern covers modifications and deletions only. A newly created entity has no prior state, so there is nothing to record in the history. The audit trail begins from the moment an entity is first changed after creation.

The audit record is written in the same database transaction as the entity change itself. This guarantees that the audit log is always consistent with the data — if the entity save rolls back, the audit record rolls back with it.

## Audit Table Design

A single audit table can track changes across **multiple entity types**. The `Change<TEntityId, TActorId>` base class includes an `EntityType` column (the CLR type name) so records from different entity types can coexist in the same table and be queried independently.

The constraint for sharing an audit table is that all tracked entities must use the **same primary key type** (`TEntityId`). For example, a single `CrmChange` table can audit both `Company` and `Contact` if both use `Guid` as their primary key. Entities with different key types require separate audit tables.

The actor ID type (`TActorId`) should be **consistent across the entire system** — typically `string` for a username or `int` for a user ID. Using the same actor ID type everywhere means a single audit table design covers all entities, and querying "what did this user change?" spans the whole schema without joins or type gymnastics.

## Three Roles

The audit pattern requires three collaborating pieces:

1. **Auditable entity** — the entity being tracked. Implements `IAuditable<TEntityId>` to expose its primary key to the audit infrastructure.
2. **Change record** — a dedicated entity that stores one audit entry per change event. Subclasses `Change<TEntityId, TActorId>` and is persisted to its own table. One table can cover many entity types as long as they share the same key type.
3. **Actor identity** — a service that resolves who is making the current change. Implements `IGetCurrentActorId<TActorId>` and is supplied by the application (HTTP context, job scheduler, CLI invocation, etc.).

`ChangeAuditInterceptor` wires these three pieces together inside the EF `SaveChanges` pipeline.

## Required Types

### `IAuditable<TEntityId>`

Any entity you want to audit must implement this interface:

```csharp
public interface IAuditable<out TEntityId> {
    TEntityId Id { get; }
}
```

Apply it to the entity class:

```csharp
public class Company : IAuditable<Guid> {
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
```

### `Change<TEntityId, TActorId>`

Define a concrete audit-record entity by subclassing `Change<TEntityId, TActorId>`. This entity is stored in the database and must have its own `EntityMap`.

```csharp
public class CompanyChange : Change<Guid, string> {
    // Inherits: EntityId, ActorId, UtcTimeStamp, ChangeType, EntityType, Json
}

public class CompanyChangeEntityMap : EntityMap<CompanyChange> {
    public override void Map(EntityTypeBuilder<CompanyChange> builder) {
        builder.HasKey(x => new { x.EntityId, x.UtcTimeStamp });
        builder.Property(x => x.Json).HasMaxLength(-1); // varchar(max)
    }
}
```

The base class properties:

| Property | Type | Description |
|---|---|---|
| `EntityId` | `TEntityId` | The primary key of the audited entity |
| `ActorId` | `TActorId` | Identifies who made the change |
| `UtcTimeStamp` | `DateTime` | When the change occurred (UTC) |
| `ChangeType` | `ChangeType` | `Modified` or `Deleted` |
| `EntityType` | `string` | The CLR type name of the audited entity |
| `Json` | `string?` | JSON snapshot of the changed properties |

The JSON snapshot format for a `Modified` entry:

```json
{
  "Name": { "Original": "Acme Corp", "Current": "Acme Corporation" },
  "Description": { "Original": null, "Current": "Global supplier" }
}
```

For a `Deleted` entry only `Original` values are captured.

### `IGetCurrentActorId<T>`

The interceptor uses this interface to resolve who is making the change. Implement it for your actor ID type and register it in DI:

```csharp
public class HttpContextActorIdProvider : IGetCurrentActorId<string> {
    readonly IHttpContextAccessor httpContextAccessor;

    public HttpContextActorIdProvider(IHttpContextAccessor httpContextAccessor) {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string Get() =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
}
```

For background jobs or CLI tools where there is no HTTP context, provide a fixed value:

```csharp
public class SystemActorIdProvider : IGetCurrentActorId<string> {
    public string Get() => "system";
}
```

---

## Registration

Register the interceptor as a **scoped** service and add it to the `DbContext` options. The interceptor is scoped so it can resolve scoped services such as `IHttpContextAccessor`.

```csharp
public static IServiceCollection AddCrmAudit(this IServiceCollection services) {
    services.AddScoped<IGetCurrentActorId<string>, HttpContextActorIdProvider>();
    services.AddScoped<ChangeAuditInterceptor<CompanyChange, Guid, string>>();
    return services;
}
```

Wire the interceptor into the `DbContext` options builder:

```csharp
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeAuditInterceptor<CompanyChange, Guid, string>>()
    );
});
```

> **Note:** Use `AddDbContext` (not `AddDbContextPool`) when registering interceptors that depend on scoped services. Context pooling resets the `DbContext` between uses but does not re-resolve scoped interceptors.

---

## Full Example

**Entities:**

```csharp
// Crm/Models/Company.cs
public class Company : IAuditable<Guid> {
    public required Guid Id { get; init; }
    [MaxLength(256)] public required string Name { get; set; }
    public string? Description { get; set; }
}

public class CompanyEntityMap : EntityMap<Company> {
    public override void Map(EntityTypeBuilder<Company> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

// Crm/Models/CompanyChange.cs
public class CompanyChange : Change<Guid, string> { }

public class CompanyChangeEntityMap : EntityMap<CompanyChange> {
    public override void Map(EntityTypeBuilder<CompanyChange> builder) {
        builder.HasKey(x => new { x.EntityId, x.UtcTimeStamp });
        builder.Property(x => x.Json).HasMaxLength(-1);
    }
}
```

**Actor ID provider:**

```csharp
// Crm/Models/HttpContextActorIdProvider.cs
public class HttpContextActorIdProvider : IGetCurrentActorId<string> {
    readonly IHttpContextAccessor httpContextAccessor;

    public HttpContextActorIdProvider(IHttpContextAccessor httpContextAccessor) {
        this.httpContextAccessor = httpContextAccessor;
    }

    public string Get() =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
}
```

**DI registration:**

```csharp
// Crm/Models/Extensions.cs
public static IServiceCollection AddCrmDbSession(this IServiceCollection services) {
    services
        .AddHttpContextAccessor()
        .AddScoped<IGetCurrentActorId<string>, HttpContextActorIdProvider>()
        .AddScoped<ChangeAuditInterceptor<CompanyChange, Guid, string>>()
        .AddScoped<ICrmDbSession>(provider => provider.GetRequiredService<CrmDbSession>())
        .AddScoped<ICompanyRepository, CompanyRepository>();
    return services;
}

// In Program.cs / Startup.cs
services.AddCrmDbSession();
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeAuditInterceptor<CompanyChange, Guid, string>>()
    );
});
```

---

## Concurrency Tokens

Properties marked as concurrency tokens (e.g. `[Timestamp]` / `IsRowVersion()`) are automatically excluded from the JSON snapshot. They are noise in the audit log — the actual row version value is not meaningful as history.
