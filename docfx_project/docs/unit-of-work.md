# Unit of Work Pattern

The unit of work spans a single HTTP request. The controller action is the application boundary: it validates input, calls services for domain logic, and commits **exactly once** via `repository.SaveAndReturn(...)`. Services and repositories never commit.

---

## Architecture

```
HTTP Request
     ↓
 Controller action
   • validates & sanitizes request (Albatross.Input)
   • calls service(s) for domain logic
   • calls repository.SaveAndReturn(...)  ← single commit point
   • returns DTO
     ↓
   Service
   • performs domain logic
   • calls repository.Add / .Delete / query methods
   • returns entity — never a DTO
   • never calls SaveChanges
     ↓
  Repository
   • reads and writes via IDbSession
   • never calls SaveChanges
```

---

## 1. Service

Services own business logic. They depend on repository interfaces, return entities, and **never call `SaveChangesAsync`**.

```csharp
public interface ICompanyService {
    Task<Company> Create(Guid id, string name, CancellationToken cancellationToken);
    Task<Company> Update(Guid id, string name, CancellationToken cancellationToken);
    Task Delete(Guid id, CancellationToken cancellationToken);
}

public class CompanyService : ICompanyService {
    readonly ICompanyRepository companyRepository;

    public CompanyService(ICompanyRepository companyRepository) {
        this.companyRepository = companyRepository;
    }

    public async Task<Company> Create(Guid id, string name, CancellationToken cancellationToken) {
        var company = new Company { Id = id, Name = name };
        companyRepository.Add(company);
        return company;
        // No SaveChangesAsync — caller is responsible
    }

    public async Task<Company> Update(Guid id, string name, CancellationToken cancellationToken) {
        var company = await companyRepository.GetById(id, cancellationToken); // throws NotFoundException if missing
        company.Name = name;
        return company;
        // EF change tracking picks up the mutation; no explicit SaveChanges needed here
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken) {
        var company = await companyRepository.GetById(id, cancellationToken);
        companyRepository.Delete(company);
    }
}
```

**Rules:**
- Services **never depend on `IDbSession`** — only on repository interfaces.
- Services **return entities**, not DTOs — transformation happens in the controller.
- Services **never pre-check uniqueness** — let the database enforce it. `SaveResults.NameConflict` tells the caller if a unique constraint was violated.

---

## 2. DTO

Define a `CreateDto()` method on the entity. The DTO is a `record class` in the shared/Core project.

```csharp
// Crm.Models/Dtos/CompanyDto.cs
public record class CompanyDto {
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

// On the entity (Company.cs)
public CompanyDto CreateDto() => new CompanyDto {
    Id = Id,
    Name = Name,
    Description = Description,
};
```

The controller calls `entity.CreateDto()` — it never constructs the DTO directly.

---

## 3. Controller

The controller:
1. Validates and sanitizes the request (`Albatross.Input`)
2. Calls service methods with sanitized values
3. Converts the returned entity to a DTO via `CreateDto()`
4. Commits once via `repository.SaveAndReturn(...)`

```csharp
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase {
    readonly ICompanyService companyService;
    readonly ICompanyRepository companyRepository;

    public CompanyController(ICompanyService companyService, ICompanyRepository companyRepository) {
        this.companyService = companyService;
        this.companyRepository = companyRepository;
    }

    [HttpGet]
    public async Task<List<CompanyDto>> GetAll(CancellationToken cancellationToken) {
        var companies = await companyRepository.GetAll(cancellationToken);
        return companies.Select(x => x.CreateDto()).ToList();
    }

    [HttpGet("{id}")]
    public async Task<CompanyDto> GetById(Guid id, CancellationToken cancellationToken) {
        var company = await companyRepository.GetById(id, cancellationToken); // throws NotFoundException → 404
        return company.CreateDto();
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken) {
        if (request.Validate(out var sanitized).HasProblem(out var problem)) {
            return BadRequest(problem);
        }
        return await companyRepository.SaveAndReturn(async ct => {
            var company = await companyService.Create(Guid.NewGuid(), sanitized.Name, ct);
            return company.CreateDto();
        }, cancellationToken);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken) {
        if (request.Validate(out var sanitized).HasProblem(out var problem)) {
            return BadRequest(problem);
        }
        return await companyRepository.SaveAndReturn(async ct => {
            var company = await companyService.Update(id, sanitized.Name, ct);
            return company.CreateDto();
        }, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken) {
        return await companyRepository.SaveAndReturn(async ct => {
            await companyService.Delete(id, ct);
        }, cancellationToken);
    }
}
```

**Rules:**
- Inject **both** `ICompanyService` and `ICompanyRepository` — the service is for logic, the repository is the commit point.
- **Never return an entity** from an endpoint. Always call `CreateDto()`.
- **Always add `CancellationToken cancellationToken`** as the last parameter on every async action.
- Always validate POST/PUT bodies. Use only the `sanitized` copy after calling `Validate` — never the raw `request`.

---

## 4. `SaveAndReturn`

`SaveAndReturn` (from `Albatross.Hosting.EFCore`) wraps work + commit + HTTP error mapping into one call:

| Signature | Use when |
|---|---|
| `SaveAndReturn(Func<CancellationToken, Task<T>>, ct)` → `ActionResult<T>` | Async work that returns a value (create/update) |
| `SaveAndReturn(Func<CancellationToken, Task>, ct)` → `ActionResult` | Async work with no return value (delete) |
| `SaveAndReturn(Func<T>, ct)` → `ActionResult<T>` | Sync work that returns a value |
| `SaveAndReturn(Action, ct)` → `ActionResult` | Sync work with no return value |
| `SaveAndReturn(ct)` → `ActionResult` | Work done before the call; just save |

HTTP responses produced by `SaveAndReturn`:

| Condition | HTTP status |
|---|---|
| Success with data | 200 OK |
| Success with no data | 204 No Content |
| `NotFoundException` thrown | 404 Not Found |
| `NameConflict` (unique constraint) | 409 Conflict |
| `ForeignKeyConflict` | 422 Unprocessable Entity |
| Other exception | 500 Internal Server Error |

All error responses use RFC 7807 `ProblemDetails`.

### Manual approach

For cases where `SaveAndReturn` does not fit, call `SaveChangesAsync` and use `HandleSaveResult` directly:

```csharp
var company = await companyService.Create(Guid.NewGuid(), sanitized.Name, cancellationToken);
var results = await companyRepository.SaveChangesAsync(throwException: false, cancellationToken);
return results.HandleSaveResult(company.CreateDto());

// For void actions:
return results.HandleSaveResult();
```

`SaveResults` properties:

| Property | Meaning |
|---|---|
| `Success` | `true` if no exception occurred |
| `NameConflict` | Unique constraint was violated |
| `ForeignKeyConflict` | Foreign key constraint was violated |
| `Error` | The underlying exception (non-null when `!Success`) |

Pass `throwException: true` when you want exceptions to propagate directly — for example, in background jobs where HTTP mapping is not needed.

---

## 5. Required Packages

| Package | Purpose |
|---|---|
| `Albatross.EFCore` | `IRepository`, `SaveResults`, `NotFoundException`, `Repository<T>` |
| `Albatross.Hosting.EFCore` | `SaveAndReturn`, `HandleSaveResult` |
| `Albatross.Input` | `IRequest<T>`, `Validate()` |
| `Albatross.Hosting` | `HasProblem`, global exception handler |

---

## Checklist

When adding a write endpoint:

- [ ] Request class implements `IRequest<T>` with a validator in the same file
- [ ] Entity has a `CreateDto()` method; DTO is a `record class`
- [ ] Service method takes primitives, returns entity, never calls `SaveChangesAsync`
- [ ] Controller validates the request: `request.Validate(out var sanitized).HasProblem(out var problem)`
- [ ] Controller calls service with `sanitized` values, not raw `request`
- [ ] Controller wraps work in `repository.SaveAndReturn(...)`
- [ ] Controller returns `entity.CreateDto()` — never the entity itself
- [ ] Every async action has `CancellationToken cancellationToken` as the last parameter
