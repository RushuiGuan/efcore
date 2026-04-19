# Cache Eviction

`CacheEvictionInterceptor` automatically invalidates cache entries when entities are modified or deleted. It runs inside the EF `SaveChanges` pipeline and fires your eviction logic only after the database write succeeds, so stale cache entries are never evicted prematurely.

---

## How It Works

The interceptor fires at three points:

1. **Before save** (`SavingChangesAsync`) — scans the change tracker for entities in `Modified` or `Deleted` state. For each entry it calls `ShouldEvict` to decide whether that entity should trigger cache invalidation. Qualifying entries are collected in memory.
2. **After successful save** (`SavedChangesAsync`) — invokes the `Evict` callback with the collected items, then clears the internal list.
3. **On failure** (`SaveChangesFailedAsync`) — clears the internal list without invoking `Evict`.

Errors thrown by `Evict` are caught and logged so they never block or roll back the save.

> **Added entities are not evicted.** A newly created entity cannot have a stale cache entry. Only modifications and deletions can invalidate existing cached data.

---

## `CacheEvictionItem`

Each item passed to `Evict` describes one entity change:

| Property | Type | Description |
|---|---|---|
| `EntityType` | `Type` | The CLR type of the changed entity |
| `Entity` | `object` | The entity instance |
| `State` | `EntityState` | `Modified` or `Deleted` |

---

## Configuration

`CacheEvictionInterceptor` is configured through `required init` properties:

| Property | Type | Description |
|---|---|---|
| `ShouldEvict` | `Func<CacheEvictionItem, bool>` | Return `true` to include this entity in the eviction batch |
| `Evict` | `Func<IEnumerable<CacheEvictionItem>, ValueTask>` | Called with all qualifying items after a successful save |

---

## Registration

Register the interceptor as a **scoped** service (it holds per-request state). Configure `ShouldEvict` and `Evict` when building it:

```csharp
services.AddScoped(provider => new CacheEvictionInterceptor(
    provider.GetRequiredService<ILogger<CacheEvictionInterceptor>>()
) {
    ShouldEvict = item => item.EntityType == typeof(Company),
    Evict = async items => {
        var cache = provider.GetRequiredService<IMemoryCache>();
        foreach (var item in items) {
            if (item.Entity is Company company) {
                cache.Remove($"company:{company.Id}");
            }
        }
        await ValueTask.CompletedTask;
    }
});
```

Wire the interceptor into the `DbContext` options:

```csharp
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<CacheEvictionInterceptor>()
    );
});
```

> **Note:** Use `AddDbContext` (not `AddDbContextPool`) when registering interceptors that depend on scoped services.

---

## Full Example

Evict company entries from `IDistributedCache` when a company is modified or deleted:

**Service registration:**

```csharp
// Crm/Models/Extensions.cs
public static IServiceCollection AddCrmCacheEviction(this IServiceCollection services) {
    services.AddScoped(provider => new CacheEvictionInterceptor(
        provider.GetRequiredService<ILogger<CacheEvictionInterceptor>>()
    ) {
        ShouldEvict = item =>
            item.EntityType == typeof(Company) ||
            item.EntityType == typeof(Contact),

        Evict = async items => {
            var cache = provider.GetRequiredService<IDistributedCache>();
            foreach (var item in items) {
                string key = item.EntityType == typeof(Company)
                    ? $"company:{((Company)item.Entity).Id}"
                    : $"contact:{((Contact)item.Entity).Id}";
                await cache.RemoveAsync(key);
            }
        }
    });
    return services;
}
```

**DbContext setup:**

```csharp
// Program.cs / Startup.cs
services.AddDistributedMemoryCache();
services.AddCrmCacheEviction();

services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<CacheEvictionInterceptor>()
    );
});
```

**Reading from cache in the repository:**

```csharp
public async Task<Company?> GetById(Guid id, CancellationToken cancellationToken) {
    var cacheKey = $"company:{id}";
    var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
    if (cached != null) {
        return JsonSerializer.Deserialize<Company>(cached);
    }
    var company = await session.DbContext.Set<Company>()
        .Include(x => x.Contacts)
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    if (company != null) {
        await cache.SetStringAsync(cacheKey,
            JsonSerializer.Serialize(company),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) },
            cancellationToken);
    }
    return company;
}
```

When `CompanyService.Update` is called and the save succeeds, `CacheEvictionInterceptor` automatically removes `company:{id}` from the cache. The next call to `GetById` repopulates it from the database.

---

## Combining Multiple Interceptors

All three interceptors — `ChangeAuditInterceptor`, `ChangeReportInterceptor`, and `CacheEvictionInterceptor` — can be registered together on the same `DbContext`:

```csharp
services.AddDbContext<CrmDbSession>((provider, builder) => {
    builder.UseSqlServer(connectionString);
    builder.AddInterceptors(
        provider.GetRequiredService<ChangeAuditInterceptor<CompanyChange, Guid, string>>(),
        provider.GetRequiredService<ChangeReportInterceptor<Contact>>(),
        provider.GetRequiredService<CacheEvictionInterceptor>()
    );
});
```

Each interceptor is independent — they all observe the same `SaveChanges` call but act on different concerns.
