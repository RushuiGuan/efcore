# Change Reporting

`ChangeReportInterceptor<TEntity>` collects property-level change data for a specific entity type during a `SaveChanges` call and delivers a structured report after the save succeeds. Unlike the audit interceptor it does not write to the database — it invokes an async callback you supply, giving you full control over what to do with the report (send a notification, update a cache, publish an event, etc.).

---

## How It Works

The interceptor fires at three points:

1. **Before save** (`SavingChangesAsync`) — scans the change tracker for entities of type `TEntity` whose state matches the configured `ChangeType`. For each matching entity it builds a `ChangeReport<TEntity>` record per modified property and holds them in memory.
2. **After successful save** (`SavedChangesAsync`) — invokes the `OnReportGenerated` callback with the collected reports, then clears the internal list.
3. **On failure** (`SaveChangesFailedAsync`) — clears the internal list without invoking the callback.

Errors thrown by `OnReportGenerated` are caught and logged so they never block or roll back the save.

---

## `ChangeReport<T>`

Each item in the report represents one property change on one entity:

| Property | Type | Description |
|---|---|---|
| `Entity` | `T` | The entity instance (after save) |
| `Type` | `ChangeType` | `Added`, `Modified`, or `Deleted` |
| `Property` | `string` | The EF property name |
| `OldValue` | `object?` | Original value (`null` for `Added`) |
| `NewValue` | `object?` | Current value (`null` for `Deleted`) |

---

## `ChangeType`

`ChangeType` is a flags enum. Use it to filter which states to report:

```csharp
[Flags]
public enum ChangeType {
    Added    = 1,
    Deleted  = 2,
    Modified = 4,
    None     = 0,
    All      = Added | Deleted | Modified,
}
```

---

## Configuration

`ChangeReportInterceptor<TEntity>` is configured entirely through `required init` properties — there is no separate options class.

| Property | Type | Default | Description |
|---|---|---|---|
| `ChangeType` | `ChangeType` | `All` | Which entity states to report on |
| `ShouldSkip` | `Func<IProperty, bool>` | Skips concurrency tokens | Per-property filter; return `true` to exclude a property from the report |
| `OnReportGenerated` | `Func<List<ChangeReport<TEntity>>, ValueTask>` | *(required)* | Callback invoked after a successful save |

---

## Registration

Register the interceptor as a **scoped** service (it holds per-request state). Build and configure it when adding it to the `DbContext` options:

```csharp
services.AddScoped(provider => new ChangeReportInterceptor<Contact>(
    provider.GetRequiredService<ILogger<ChangeReportInterceptor<Contact>>>()
) {
    ChangeType = ChangeType.Modified | ChangeType.Deleted,
    OnReportGenerated = async reports => {
        foreach (var report in reports) {
            Console.WriteLine($"{report.Entity.Name}.{report.Property}: {report.OldValue} → {report.NewValue}");
        }
        await Task.CompletedTask;
    }
});
```

Wire it into the `DbContext` options:

```csharp
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeReportInterceptor<Contact>>()
    );
});
```

> **Note:** Use `AddDbContext` (not `AddDbContextPool`) when registering interceptors that depend on scoped services.

---

## Customizing `ShouldSkip`

Override `ShouldSkip` to exclude properties from the report beyond the default (concurrency tokens):

```csharp
services.AddScoped(provider => new ChangeReportInterceptor<Contact>(
    provider.GetRequiredService<ILogger<ChangeReportInterceptor<Contact>>>()
) {
    ShouldSkip = property =>
        property.IsConcurrencyToken ||
        property.Name == nameof(Contact.CompanyId),  // exclude FK columns
    OnReportGenerated = reports => { /* ... */ return ValueTask.CompletedTask; }
});
```

---

## Full Example

Publish contact changes to an event bus after each save:

**Setup:**

```csharp
// Crm/Models/Extensions.cs
public static IServiceCollection AddContactChangeReporting(
    this IServiceCollection services,
    IEventBus eventBus)
{
    services.AddScoped(provider => new ChangeReportInterceptor<Contact>(
        provider.GetRequiredService<ILogger<ChangeReportInterceptor<Contact>>>()
    ) {
        ChangeType = ChangeType.All,
        ShouldSkip = property => property.IsConcurrencyToken,
        OnReportGenerated = async reports => {
            var events = reports
                .GroupBy(r => r.Entity.Id)
                .Select(g => new ContactChangedEvent {
                    ContactId = g.Key,
                    Changes = g.Select(r => new PropertyChange {
                        Property = r.Property,
                        OldValue = r.OldValue?.ToString(),
                        NewValue = r.NewValue?.ToString(),
                        Type = r.Type,
                    }).ToList()
                });
            foreach (var evt in events) {
                await eventBus.PublishAsync(evt);
            }
        }
    });
    return services;
}
```

**Register with `DbContext`:**

```csharp
services.AddContactChangeReporting(eventBus);
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeReportInterceptor<Contact>>()
    );
});
```

**Multiple entity types:**

Register a separate interceptor instance per entity type and add all of them:

```csharp
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeReportInterceptor<Contact>>(),
        provider.GetRequiredService<ChangeReportInterceptor<Address>>()
    );
});
```
