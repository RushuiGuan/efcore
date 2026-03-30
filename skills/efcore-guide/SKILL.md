---
name: efcore-guide
description: Guide for setting up and using the Albatross.EFCore NuGet packages (DbSession, EntityMap, CodeGen, SQL Server, PostgreSQL, migrations, audit, change reporting). Use when a user asks how to set up EFCore with these packages, add entities, configure migrations, or use any Albatross.EFCore feature.
---

You are helping a developer use the **Albatross.EFCore** family of NuGet packages. Use the patterns and examples below to answer questions, scaffold code, or walk through setup.

---

## Packages

| Package | Purpose |
|---|---|
| `Albatross.EFCore` | Base library: `DbSession`, `EntityMap<T>`, event system, repository pattern |
| `Albatross.EFCore.CodeGen` | Dev-only Roslyn source generator — auto-registers all `EntityMap<T>` classes |
| `Albatross.EFCore.SqlServer` | SQL Server DI registration and constraint helpers |
| `Albatross.EFCore.PostgreSQL` | PostgreSQL DI registration, lowercase naming convention, constraint helpers |
| `Albatross.EFCore.Admin` | CLI admin utilities: migrate, run scripts, generate SQL |
| `Albatross.EFCore.Audit` | Audit event handler that captures entity changes |
| `Albatross.EFCore.AutoCacheEviction` | Auto-evicts cache entries on entity saves |
| `Albatross.EFCore.ChangeReporting` | Produces a change report for entity types within a DbContext |

---

## Prerequisites

- Dotnet SDK 9+ (or SDK 8 with `Microsoft.Net.Compilers.Toolset` 4.12.0+)
- `Albatross.EFCore.CodeGen` requires Roslyn compiler 4.12.0 or higher

If using SDK 8 with non-Visual Studio tooling, add to the project file:
```xml
<PackageReference Include="Microsoft.Net.Compilers.Toolset" Version="4.12.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

---

## Project Layout (Recommended)

Separate concerns across projects:

- **`YourApp.Models`** — entities, `EntityMap<T>` classes, `DbSession` subclass
- **`YourApp.SqlServer`** or **`YourApp.Postgres`** — database-specific DI wiring
- **`YourApp.Admin`** — migration DbContext, CLI admin commands (never mixed into the main app)

---

## Step 1 — Define Entities and Map Them

```csharp
// Entity
public class Address {
    public int Id { get; set; }
    public string? City { get; set; }
}

// Mapping class — CodeGen will auto-discover this
public class AddressEntityMap : EntityMap<Address> {
    public override void Map(EntityTypeBuilder<Address> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.City).HasMaxLength(100);
    }
}
```

`EntityMap<T>` implements `IBuildEntityModel`. The `Albatross.EFCore.CodeGen` source generator scans the assembly at compile time and generates a `BuildEntityModels()` extension method that calls every map.

**Rule: `int` primary keys use the default identity column — do not set `ValueGeneratedNever()` or any other value generation option.** EFCore's default behavior maps `int` keys to a database identity/auto-increment column, which is the intended pattern here.

**Rule: All string properties must have the `[MaxLength(n)]` attribute applied directly on the property, unless unbounded length is intentional (e.g., a JSON text column or a free-text/notes column).** Without a max length, EFCore maps the column to `nvarchar(max)` / `text`, which has performance and indexing implications. EFCore picks up `[MaxLength]` automatically — no need to also call `HasMaxLength()` in the entity map.

**Rule: Guid primary keys must always use `ValueGeneratedNever()`.**
EFCore defaults to treating Guid keys as database-generated. With this library, Guid keys are always assigned by the application. Omitting `ValueGeneratedNever()` causes EFCore to silently ignore the value you set and generate its own.

**Rule: Immutable properties use `{ get; init; }`. If the value is application-supplied (not database-generated), also add the `required` keyword.**

A property is immutable when:
- It is a primary key that is not database-generated (e.g., a Guid key with `ValueGeneratedNever()`)
- It is part of an alternative key (`HasAlternateKey`)
- It is any other property that should not change after the entity is created

An `int` identity primary key is database-generated, so it uses `{ get; init; }` without `required` (the value is assigned by the database, not the caller).

**Rule: All non-nullable, non-generated scalar properties (excluding navigation properties) must have the `required` keyword.**

**Rule: Navigation properties follow specific conventions:**
- **Collection navigation** — use `List<T>`, `{ get; init; }` setter, always initialized to an empty list (`= []`)
- **Reference navigation** — no `required`, initialized to `null!` when non-nullable to suppress compiler warnings (EFCore populates it via lazy loading or explicit include)
- **Foreign key** — always define the foreign key as an explicit property on the entity alongside the navigation property. Apply the same `required` and mutability rules as any other scalar property.

```csharp
public class Company {
    public int Id { get; init; }                            // generated, no required
    public required string Name { get; set; }               // mutable scalar, required
    public required string Code { get; init; }              // immutable scalar, required (alternate key)
    public string? Description { get; set; }                // nullable, no required

    // foreign key + reference navigation
    public required int AddressId { get; set; }
    public Address Address { get; set; } = null!;

    // collection navigation — init + initialized
    public List<Contact> Contacts { get; init; } = [];
}

// Guid primary key — application-supplied
public class Order {
    public required Guid Id { get; init; }                  // app-supplied, required + init
    public required string Reference { get; set; }          // mutable scalar, required
    public string? Notes { get; set; }                      // nullable, no required

    // collection navigation
    public List<OrderLine> Lines { get; init; } = [];
}

public class CompanyEntityMap : EntityMap<Company> {
    public override void Map(EntityTypeBuilder<Company> builder) {
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => x.Code);
    }
}

public class OrderEntityMap : EntityMap<Order> {
    public override void Map(EntityTypeBuilder<Order> builder) {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
    }
}
```

---

## Step 2 — Create the DbContext

Extend `DbSession` (or `DbSessionWithEventHandlers` if you need the event system):

```csharp
public class SampleDbSession : DbSession {
    public SampleDbSession(DbContextOptions<SampleDbSession> options) : base(options) { }

    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasDefaultSchema("sample");
        modelBuilder.BuildEntityModels();   // generated by CodeGen
    }
}
```

Use `DbSessionWithEventHandlers` when you need pre/post-save hooks (e.g., audit):

```csharp
public class SampleDbSession : DbSessionWithEventHandlers {
    public SampleDbSession(
        DbContextOptions<SampleDbSession> options,
        IDbEventSessionProvider eventSessionProvider) : base(options, eventSessionProvider) { }
}
```

---

## Step 3 — Register with DI

### SQL Server

```csharp
// Program.cs / Startup.cs
services.AddSqlServer<SampleDbSession>(provider =>
    provider.GetRequiredService<IConfiguration>().GetConnectionString("Default")!);

// High-throughput: use context pooling
services.AddSqlServerWithContextPool<SampleDbSession>(
    provider => connectionString);
```

### PostgreSQL

```csharp
services.AddPostgres<SampleDbSession>(provider => connectionString);

// With context pooling
services.AddPostgresWithContextPool<SampleDbSession>(
    provider => connectionString);
```

PostgreSQL automatically applies a lowercase column-naming convention.

### Event system (required for Audit / AutoCacheEviction)

```csharp
services.AddDbSessionEvents();
```

---

## Step 4 — Admin Project for Migrations

The admin project is a separate console app (`OutputType=Exe`). It must never be merged into the main application.

### Project file

Reference `Albatross.EFCore.Admin` and `Microsoft.EntityFrameworkCore.Design` (as a private/build-only asset), plus the main app project:

```xml
<PackageReference Include="Albatross.EFCore.Admin" Version="..." />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="...">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<ProjectReference Include="..\YourApp\YourApp.csproj" />
```

### Migration DbContext classes

Create one migration DbContext per database type, deriving from the app's `DbSession`. Each needs a parameterless constructor (required by the `dotnet ef` tool) and a connection-string constructor. Use `BuildMigrationOption<T>` from the database-specific package to build the options:

```csharp
// PostgreSQL
public class AppPostgresMigration : AppDbSession {
    public AppPostgresMigration() : this("any") { }
    public AppPostgresMigration(string connectionString)
        : base(Albatross.EFCore.PostgreSQL.Extensions.BuildMigrationOption<AppDbSession>(Constants.Schema, connectionString)) { }
}

// SQL Server
public class AppSqlServerMigration : AppDbSession {
    public AppSqlServerMigration() : this("any") { }
    public AppSqlServerMigration(string connectionString)
        : base(Albatross.EFCore.SqlServer.Extensions.BuildMigrationOption<AppDbSession>(Constants.Schema, connectionString)) { }
}
```

### CLI verbs

Use `Albatross.CommandLine` to define the admin CLI. Declare parent verbs for each database type in a shared `ParentParams` class:

```csharp
// ParentParams.cs
[Verb("sqlserver", Alias = ["sql"], Description = "Execute sql server related commands")]
[Verb("postgres",  Alias = ["pg"],  Description = "Execute postgresql server related commands")]
public class ParentParams { }
```

Wire up the built-in admin commands in `Verbs.cs` using assembly-level attributes:

```csharp
// Verbs.cs
[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<AppPostgresMigration>>("postgres exec-script")]
[assembly: Verb<GenerateSqlScriptParams,        GenerateSqlScript<AppPostgresMigration>>       ("postgres create-script")]
[assembly: Verb<EFMigrationParams,              EFMigrate<AppPostgresMigration>>               ("postgres ef-migrate")]

[assembly: Verb<ExecuteDeploymentScriptsParams, ExecuteDeploymentScripts<AppSqlServerMigration>>("sqlserver exec-script")]
[assembly: Verb<GenerateSqlScriptParams,        GenerateSqlScript<AppSqlServerMigration>>       ("sqlserver create-script")]
[assembly: Verb<EFMigrationParams,              EFMigrate<AppSqlServerMigration>>               ("sqlserver ef-migrate")]
```

### Program.cs

Detect design-time (when the `dotnet ef` tool is running) and short-circuit early. At runtime, register the appropriate database provider and migration DbContext based on the command key:

```csharp
static async Task<int> Main(string[] args) {
    if (EF.IsDesignTime) {
        await new HostBuilder().Build().RunAsync();
        return 0;
    }
    await using var host = new CommandHost("My Admin")
        .RegisterServices(RegisterServices)
        .AddCommands()
        .Parse(args)
        .WithDefaults()
        .Build();
    return await host.InvokeAsync();
}

static void RegisterServices(ParseResult result, IServiceCollection services) {
    services.AddAppDbSession().AddApp();     // register your app's DI
    var key = result.CommandResult.Command.GetCommandKey();
    if (key.StartsWith("sqlserver")) {
        services.AddConfig<IAppConfig, SqlServer.AppConfig>();
        services.AddSingleton<IExecuteScriptFile, ExecuteSqlServerScriptFile>();
        services.AddScoped(p => new AppSqlServerMigration(p.GetRequiredService<IAppConfig>().ConnectionString));
        services.AddSqlServerWithContextPool<AppDbSession>(p => p.GetRequiredService<IAppConfig>().ConnectionString);
    } else if (key.StartsWith("postgres")) {
        services.AddConfig<IAppConfig, Postgres.AppConfig>();
        services.AddSingleton<IExecuteScriptFile, ExecuteScriptFile>();
        services.AddScoped(p => new AppPostgresMigration(p.GetRequiredService<IAppConfig>().ConnectionString));
        services.AddPostgresWithContextPool<AppDbSession>(p => p.GetRequiredService<IAppConfig>().ConnectionString);
    }
    services.RegisterCommands();
}
```

### Adding a migration

```shell
dotnet ef migrations add <MigrationName> --context AppSqlServerMigration --output-dir Migrations/SqlServer
dotnet ef migrations add <MigrationName> --context AppPostgresMigration  --output-dir Migrations/Postgres
```

### Running migrations and scripts

```shell
# Apply pending migrations
App.Admin.exe sql ef-migrate
App.Admin.exe pg  ef-migrate

# Generate SQL script for review
App.Admin.exe sql create-script
App.Admin.exe pg  create-script

# Run deployment scripts (only if pending migrations exist and version matches)
App.Admin.exe sql exec-script --directory ./scripts --pre-migration
App.Admin.exe pg  exec-script --directory ./scripts --pre-migration
```

---

## Special Property Helpers

### JSON columns

```csharp
// Immutable JSON object
builder.Property(x => x.Config).HasImmutableJsonProperty<ConfigType>();

// JSON collection
builder.Property(x => x.Items).HasJsonCollectionProperty<ItemType>();
```

JSON is serialized with camelCase, enum converter, and no default value emission.

### UTC DateTime

```csharp
builder.Property(x => x.CreatedAt).UtcDateTimeProperty();
```

### Constraint violation detection

```csharp
// SQL Server
if (err.IsUniqueConstraintViolation()) { ... }
if (err.IsForeignKeyConstraintViolation()) { ... }

// PostgreSQL — same extension methods, different underlying detection
```

---

## Repository Pattern

Rules:
- Create one `Repository<T>` per **aggregate root** — not per entity. Child entities are accessed and persisted through their root's repository.
- Repositories live in the **same namespace as the models**.
- A repository's main responsibility is to **query and provide entities** for services. Keep query logic in the repository, not in services.
- Always expose a **repository interface** (e.g., `ICompanyRepository`) and have services depend on the interface — never on the concrete class or the DbSession directly. This keeps services unit-testable.
- All database operations in the repository must be **async** and accept a `CancellationToken`.
- Services that make state changes **must not call `SaveChangesAsync`**. That is the responsibility of the caller (e.g., a controller or command handler). This keeps the unit of work boundary explicit and composable.

The base `IRepository` interface provides `Add<T>`, `Delete<T>`, and `SaveChangesAsync` — do not redeclare these in specific repository interfaces.

```csharp
// Interface — declares only queries specific to this aggregate
public interface ICompanyRepository : IRepository {
    Task<List<Company>> GetAll(CancellationToken cancellationToken);
    Task<Company?> GetById(Guid id, CancellationToken cancellationToken);
}

// Implementation — lives in the same namespace as Company
public class CompanyRepository : Repository<ICrmDbSession>, ICompanyRepository {
    public CompanyRepository(ICrmDbSession session) : base(session) { }

    public override bool IsUniqueConstraintViolation(Exception err) => ...;
    public override bool IsForeignKeyConstraintViolation(Exception err) => ...;

    public Task<List<Company>> GetAll(CancellationToken cancellationToken) =>
        session.Set<Company>().ToListAsync(cancellationToken);

    public Task<Company?> GetById(Guid id, CancellationToken cancellationToken) =>
        session.Set<Company>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}

// Service — depends on ICompanyRepository, never on the DbSession
public class CompanyService {
    readonly ICompanyRepository _repo;
    public CompanyService(ICompanyRepository repo) => _repo = repo;

    // Add<T> is inherited from IRepository — no need to declare it on ICompanyRepository
    public Company CreateNewCompany(CreateCompanyRequest request) {
        var company = new Company { Id = Guid.NewGuid(), Name = request.Name };
        _repo.Add(company);
        return company;
    }

    // State change: does NOT call SaveChangesAsync — the caller is responsible
    public async Task Rename(Guid id, string name, CancellationToken cancellationToken) {
        var company = await _repo.GetById(id, cancellationToken)
            ?? throw new InvalidOperationException($"Company {id} not found");
        company.Name = name;
    }
}

// Caller (e.g., controller or command handler) owns the save
SaveResults result = await repo.SaveChangesAsync(cancellationToken);
if (result.NameConflict) { /* duplicate key */ }
if (result.ForeignKeyConflict) { /* FK violation */ }
```

---

## Audit

```csharp
services.AddAuditEventHandlers();  // registers AuditChangeDbEventHandler
```

The handler fires on `IDbSessionEventHandler.OnAddedEntry`, `OnModifiedEntry`, and `OnDeletedEntry`.

---

## Debugging CodeGen Output

To view the generated code in Visual Studio or Rider, add to the project file:

```xml
<PropertyGroup>
    <EmitAlbatrossCodeGenDebugFile>true</EmitAlbatrossCodeGenDebugFile>
</PropertyGroup>
<ItemGroup>
    <CompilerVisibleProperty Include="EmitAlbatrossCodeGenDebugFile" />
</ItemGroup>
```

In Rider: **Dependencies → .NET x.0 → Source Generators**

---

## Quick Checklist

- [ ] `Albatross.EFCore` + `Albatross.EFCore.CodeGen` referenced in models project
- [ ] Each entity has a corresponding `EntityMap<T>` subclass
- [ ] `DbSession.OnModelCreating` calls `modelBuilder.BuildEntityModels()`
- [ ] DB-specific package (`SqlServer` or `PostgreSQL`) registered in DI
- [ ] Admin project has its own migration DbContext (never co-located with the app)
- [ ] Compiler version ≥ 4.12.0 if using SDK 8
