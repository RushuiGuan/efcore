# Albatross.EFCore

A .NET library that wraps EF Core's `DbContext` in a clean session abstraction and provides opinionated patterns for entity mapping, repositories, change auditing, and cache eviction. Designed for layered enterprise applications where data access is strictly separated from business logic.

## Key Features
- **Session Abstraction** - `IDbSession` / `DbSession` wraps `DbContext` as a disposable unit-of-work; repositories depend on it, services never do
- **Entity Mapping Convention** - Co-locate entities with their `EntityMap<T>` class; the `Albatross.EFCore.CodeGen` source generator auto-registers all maps via `modelBuilder.BuildEntityModels()`
- **Structured Save Results** - `Repository<T>.SaveChangesAsync` catches unique-key and foreign-key violations and returns typed `SaveResults` flags instead of raw exceptions
- **Change Report Interceptor** - `ChangeReportInterceptor<TEntity>` captures per-property before/after snapshots and invokes a callback after each successful save
- **Audit Trail Interceptor** - `ChangeAuditInterceptor<TChangeEntity, TEntityId, TActorId>` writes typed audit records for every `IAuditable<T>` entity that is modified or deleted
- **Cache Eviction Interceptor** - `CacheEvictionInterceptor` triggers cache invalidation for modified or deleted entities post-save
- **JSON Column Helpers** - `HasImmutableJsonProperty<T>` and `HasJsonCollectionProperty<T>` map complex types to `varchar(max)` JSON columns with correct change-tracking semantics
- **UTC DateTime Fix** - `UtcDateTimeProperty()` ensures `DateTime` columns round-trip with `DateTimeKind.Utc`

## Quick Start

Define a session, an entity with its map, and a repository:

```csharp
// 1. Session — one per bounded context
public interface ICrmDbSession : IDbSession { }

public class CrmDbSession : DbSession, ICrmDbSession {
    public CrmDbSession(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasDefaultSchema("crm");
        modelBuilder.BuildEntityModels(); // auto-registers all EntityMap<T> in this assembly
    }
}

// 2. Entity + map in the same file
public class Company {
    public required Guid Id { get; init; }
    [MaxLength(256)] public required string Name { get; set; }
}

public class CompanyEntityMap : EntityMap<Company> {
    public override void Map(EntityTypeBuilder<Company> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

// 3. Repository — one per aggregate root
public class CompanyRepository : Repository<ICrmDbSession> {
    public CompanyRepository(ICrmDbSession session) : base(session) { }

    public override bool IsUniqueConstraintViolation(Exception err) =>
        SqlServerExt.IsUniqueConstraintViolation(err);

    public override bool IsForeignKeyConstraintViolation(Exception err) =>
        SqlServerExt.IsForeignKeyConstraintViolation(err);

    public async Task<Company> GetById(Guid id, CancellationToken ct) {
        var entity = await session.DbContext.Set<Company>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) throw new NotFoundException<Company>(id);
        return entity;
    }
}

// 4. Save at the application boundary (controller / command handler)
var result = await repository.SaveChangesAsync(throwException: false, cancellationToken);
if (!result.Success) {
    if (result.NameConflict) return Conflict("Name already exists");
    throw result.Error!;
}
```

## Dependencies
- **Microsoft.EntityFrameworkCore 10.0.5+**
- **Microsoft.EntityFrameworkCore.Relational 10.0.5+**
- **Microsoft.Extensions.DependencyInjection.Abstractions 10.0.5+**

## Prerequisites
- **.NET 10.0+**
- **C# Compiler 4.12.0+** — required by the `Albatross.EFCore.CodeGen` source generator. Satisfied by .NET 9 SDK or Visual Studio 2022 with .NET 8. When using .NET 8 SDK with VS Code, Rider, or a terminal, add `Microsoft.Net.Compilers.Toolset 4.12.0+` to your project.

## Documentation

**[Complete Documentation](https://rushuiguan.github.io/efcore)**
