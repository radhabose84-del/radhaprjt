# BSOFT ERP - Development Standards & Architecture Guide

**Project:** BSOFT Modular Monolith ERP System
**Framework:** ASP.NET Core 8.0
**Architecture:** CQRS + Repository Pattern + Modular Monolith
**Last Updated:** 2026-02-25

---

## 🎯 Quick Reference

- **Solution:** `d:\BSOFT\BSOFT.sln`
- **Entry Point:** `src/BSOFT.Api/Program.cs`
- **Branch:** `usha_ModulerMonolithic` (main development)
- **Target Framework:** .NET 8.0
- **Database:** SQL Server (with EF Core + Dapper)
- **Logging:** MongoDB (audit logs)

---

## 📦 Module Structure

### 10 Business Modules
1. **UserManagement** - Users, roles, companies, authentication
2. **FixedAssetManagement** (FAM) - Asset tracking, depreciation
3. **InventoryManagement** - Stock, items, warehouses, HSN/SAC
4. **MaintenanceManagement** - Work orders, preventive maintenance
5. **PartyManagement** - Customers, vendors, contacts
6. **ProjectManagement** - Projects, tasks, milestones
7. **PurchaseManagement** - Purchase orders, quotations, suppliers
8. **SalesManagement** - Sales orders, organisations, customers
9. **WarehouseManagement** - Warehouse operations, transfers
10. **BudgetManagement** - Budget planning, allocation, tracking

### Shared Layers
- `src/Shared/Contracts` - Shared DTOs, interfaces, exceptions
- `src/Shared/Shared.Infrastructure` - Middleware, caching, utilities
- `src/Shared/Shared.Validation` - Validation rules, providers

---

## 🏗️ Standard Module Structure

Every module follows this exact structure:

```
src/Modules/{ModuleName}/
├── Core/
│   ├── {ModuleName}.Domain/
│   │   ├── Common/
│   │   │   └── BaseEntity.cs
│   │   ├── Entities/
│   │   └── Events/
│   │       └── AuditLogDomainEvent.cs (shared)
│   └── {ModuleName}.Application/
│       ├── {EntityName}/
│       │   ├── Commands/
│       │   │   ├── Create{EntityName}/
│       │   │   ├── Update{EntityName}/
│       │   │   └── Delete{EntityName}/
│       │   ├── Queries/
│       │   │   ├── GetAll{EntityName}/
│       │   │   ├── Get{EntityName}ById/
│       │   │   └── Get{EntityName}AutoComplete/
│       │   └── Dto/
│       │       ├── {EntityName}Dto.cs
│       │       └── {EntityName}LookupDto.cs
│       ├── Common/
│       │   ├── Interfaces/
│       │   │   └── I{EntityName}/
│       │   │       ├── I{EntityName}CommandRepository.cs
│       │   │       └── I{EntityName}QueryRepository.cs
│       │   └── Mappings/
│       │       └── {EntityName}Profile.cs
│       └── EventHandlers/
├── {ModuleName}.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   │       └── {EntityName}Configuration.cs
│   ├── Repositories/
│   │   └── {EntityName}/
│   │       ├── {EntityName}CommandRepository.cs
│   │       └── {EntityName}QueryRepository.cs
│   └── DependencyInjection.cs
├── {ModuleName}.Presentation/
│   ├── Controllers/
│   │   └── {EntityName}Controller.cs
│   └── Validation/
│       └── {EntityName}/
│           ├── Create{EntityName}CommandValidator.cs
│           └── Update{EntityName}CommandValidator.cs
└── {ModuleName}.Module/
    └── ModuleExtensions.cs
```

---

## 🧬 Base Entity Pattern

ALL entities must extend `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }

    public enum Status { Inactive = 0, Active = 1 }
    public enum IsDelete { NotDeleted = 0, Deleted = 1 }

    public Status IsActive { get; set; } = Status.Active;
    public IsDelete IsDeleted { get; set; } = IsDelete.NotDeleted;

    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedIP { get; set; }

    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
    public string? ModifiedIP { get; set; }
}
```

**Audit fields are auto-populated** by `ApplicationDbContext.SaveChangesAsync()` — never set manually.

---

## 🎨 CQRS Pattern (MediatR)

### Command Pattern

**Create Command:**
```csharp
public class Create{EntityName}Command : IRequest<ApiResponseDTO<int>>
{
    public string {UniqueCode} { get; set; }
    public string {Name} { get; set; }
    public int {ForeignKey} { get; set; }
}
```

**Update Command:**
```csharp
public class Update{EntityName}Command : IRequest<ApiResponseDTO<int>>
{
    public int Id { get; set; }
    public string {Name} { get; set; }
    public int {ForeignKey} { get; set; }
    public int IsActive { get; set; }  // 1=Active, 0=Inactive
}
```
⚠️ **Note:** Unique code fields (e.g., `CustomerCode`) are **immutable** — never include in Update command.

**Delete Command:**
```csharp
public sealed record Delete{EntityName}Command(int Id) : IRequest<bool>;
```

### Command Handler Pattern (IMapper Required)

**Create Command Handler:**
```csharp
public class Create{EntityName}CommandHandler : IRequestHandler<Create{EntityName}Command, ApiResponseDTO<int>>
{
    private readonly I{EntityName}CommandRepository _commandRepository;
    private readonly I{EntityName}QueryRepository _queryRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public Create{EntityName}CommandHandler(
        I{EntityName}CommandRepository commandRepository,
        I{EntityName}QueryRepository queryRepository,
        IMediator mediator,
        IMapper mapper)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<ApiResponseDTO<int>> Handle(Create{EntityName}Command request, CancellationToken cancellationToken)
    {
        // ✅ ALWAYS use AutoMapper — IsActive/IsDeleted are set via ForMember in the Profile
        var entity = _mapper.Map<Domain.Entities.{EntityName}>(request);

        var newId = await _commandRepository.CreateAsync(entity);

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: "{ENTITY}_CREATE",
            actionName: request.{UniqueCode},
            details: $"{EntityName} '{request.{UniqueCode}}' created successfully with Id {newId}.",
            module: "{EntityName}"
        );
        await _mediator.Publish(auditEvent, cancellationToken);

        return new ApiResponseDTO<int>
        {
            IsSuccess = true,
            Message = "{EntityName} created successfully.",
            Data = newId
        };
    }
}
```

**Update Command Handler:**
```csharp
public class Update{EntityName}CommandHandler : IRequestHandler<Update{EntityName}Command, ApiResponseDTO<int>>
{
    private readonly I{EntityName}CommandRepository _commandRepository;
    private readonly I{EntityName}QueryRepository _queryRepository;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public Update{EntityName}CommandHandler(
        I{EntityName}CommandRepository commandRepository,
        I{EntityName}QueryRepository queryRepository,
        IMediator mediator,
        IMapper mapper)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<ApiResponseDTO<int>> Handle(Update{EntityName}Command request, CancellationToken cancellationToken)
    {
        // ✅ Use AutoMapper — IsActive is mapped via ForMember in the Profile
        var entity = _mapper.Map<Domain.Entities.{EntityName}>(request);

        var result = await _commandRepository.UpdateAsync(entity);

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: "{ENTITY}_UPDATE",
            actionName: request.Id.ToString(),
            details: $"{EntityName} with Id {request.Id} updated successfully.",
            module: "{EntityName}"
        );
        await _mediator.Publish(auditEvent, cancellationToken);

        return new ApiResponseDTO<int>
        {
            IsSuccess = true,
            Message = "{EntityName} updated successfully.",
            Data = result
        };
    }
}
```

⚠️ **Key Rules for Command Handlers:**
1. **ALWAYS inject `IMapper`** — never manually assign command properties to entity properties
2. **ALWAYS use `_mapper.Map<>(request)`** to map command → domain entity
3. **NEVER set `IsActive`/`IsDeleted` manually in the handler** — define them via `ForMember` in the AutoMapper Profile instead
4. **ALWAYS inject all 4 dependencies:** `ICommandRepository`, `IQueryRepository`, `IMediator`, `IMapper`
5. **AutoMapper Profile MUST exist** in `Common/Mappings/{EntityName}Profile.cs` with `ForMember` for `IsActive` and `IsDeleted` on Create, and `ForMember` for `IsActive` on Update

### Query Pattern

**GetAll Query:**
```csharp
public class GetAll{EntityName}Query : IRequest<ApiResponseDTO<List<{EntityName}Dto>>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string SearchTerm { get; set; }
}
```

**GetById Query:**
```csharp
public class Get{EntityName}ByIdQuery : IRequest<{EntityName}Dto>
{
    public int Id { get; set; }
}
```

**AutoComplete Query:**
```csharp
public sealed record Get{EntityName}AutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<{EntityName}LookupDto>>;
```

### Query Handler Pattern (IMapper + IMediator Required)

All query handlers MUST inject `IMapper` and `IMediator` alongside the query repository.

**GetAll Query Handler:**
```csharp
public class GetAll{EntityName}QueryHandler : IRequestHandler<GetAll{EntityName}Query, ApiResponseDTO<List<{EntityName}Dto>>>
{
    private readonly I{EntityName}QueryRepository _queryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetAll{EntityName}QueryHandler(I{EntityName}QueryRepository queryRepository, IMapper mapper, IMediator mediator)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<ApiResponseDTO<List<{EntityName}Dto>>> Handle(GetAll{EntityName}Query request, CancellationToken cancellationToken)
    {
        var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
        var dtos = _mapper.Map<List<{EntityName}Dto>>(data);

        // 📘 Log domain event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetAll{EntityName}Query",
            actionCode: "Get",
            actionName: data.Count.ToString(),
            details: "{EntityName} details were fetched.",
            module: "{EntityName}"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        return new ApiResponseDTO<List<{EntityName}Dto>>
        {
            IsSuccess = true,
            Message = "Success",
            Data = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
```

**GetById Query Handler:**
```csharp
public class Get{EntityName}ByIdQueryHandler : IRequestHandler<Get{EntityName}ByIdQuery, {EntityName}Dto>
{
    private readonly I{EntityName}QueryRepository _queryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public Get{EntityName}ByIdQueryHandler(I{EntityName}QueryRepository queryRepository, IMapper mapper, IMediator mediator)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<{EntityName}Dto> Handle(Get{EntityName}ByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryRepository.GetByIdAsync(request.Id);

        if (result == null)
            return null;

        var dto = _mapper.Map<{EntityName}Dto>(result);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "Get{EntityName}ByIdQuery",
            actionName: dto.Id.ToString(),
            details: $"{EntityName} details {dto.Id} was fetched.",
            module: "{EntityName}"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        return dto;
    }
}
```

**AutoComplete Query Handler:**
```csharp
public class Get{EntityName}AutoCompleteQueryHandler : IRequestHandler<Get{EntityName}AutoCompleteQuery, IReadOnlyList<{EntityName}LookupDto>>
{
    private readonly I{EntityName}QueryRepository _queryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public Get{EntityName}AutoCompleteQueryHandler(I{EntityName}QueryRepository queryRepository, IMapper mapper, IMediator mediator)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<{EntityName}LookupDto>> Handle(Get{EntityName}AutoCompleteQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
        var dtos = _mapper.Map<List<{EntityName}LookupDto>>(result);

        // Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "Get{EntityName}AutoCompleteQuery",
            actionName: dtos.Count.ToString(),
            details: "{EntityName} details was fetched.",
            module: "{EntityName}"
        );
        await _mediator.Publish(domainEvent, cancellationToken);

        return dtos;
    }
}
```

⚠️ **Key Rules for Query Handlers:**
1. **ALWAYS inject all 3 dependencies:** `IQueryRepository`, `IMapper`, `IMediator`
2. **ALWAYS use `_mapper.Map<>(data)`** — never return repo result directly
3. **ALWAYS publish `AuditLogsDomainEvent`** after fetching data
4. **Use `public class`** (not `sealed`) and block namespace syntax (not file-scoped)
5. **GetById null guard:** `if (result == null) return null;` before mapping

---

## 📊 Repository Pattern

### CQRS Split: Command (Write) vs Query (Read)

#### Command Repository (EF Core)
```csharp
public interface I{EntityName}CommandRepository
{
    Task<int> CreateAsync(Domain.Entities.{EntityName} entity);
    Task<int> UpdateAsync(Domain.Entities.{EntityName} entity);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
}
```

**Implementation:**
- Uses `ApplicationDbContext` (EF Core)
- Audit fields auto-populated on save
- Update: Fetch existing entity, modify properties, save
- SoftDelete: Set `IsDeleted = Deleted`

#### Query Repository (Dapper)
```csharp
public interface I{EntityName}QueryRepository
{
    Task<(List<{EntityName}Dto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm);
    Task<{EntityName}Dto> GetByIdAsync(int id);
    Task<IReadOnlyList<{EntityName}LookupDto>> AutocompleteAsync(string term, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string {uniqueCode}, int? id = null);
    Task<bool> NotFoundAsync(int id);
    // FK validation methods as needed
}
```

**Implementation:**
- Uses `IDbConnection` (Dapper for performance)
- All queries filter `WHERE IsDeleted = 0`
- Autocomplete filters `WHERE IsActive = 1 AND IsDeleted = 0`
- Returns DTOs directly (no entity tracking)

---

## 🌐 Cross-Module vs Same-Module FK References

### ✅ Same-Module FKs — Use SQL JOINs (No Lookups Needed)

When a FK references another entity **within the same module**, use a direct SQL JOIN in the Dapper query repository to fetch the related name. **Do NOT create a shared lookup interface** for same-module references.

**Example — AgentCommissionConfiguration (SalesManagement module):**
- `SalesSegmentId` → `Sales.SalesSegment` (same module) → **JOIN**
- `CommissionTypeId` → `Sales.MiscMaster` (same module) → **JOIN**

```sql
-- ✅ CORRECT — Same-module JOINs in Dapper query repository
SELECT
    acc.Id, acc.AgentId, acc.SalesSegmentId, acc.ItemId,
    acc.CommissionTypeId, acc.CommissionPercentage,
    ss.SegmentName,                          -- Same-module JOIN
    mm.Description AS CommissionTypeName     -- Same-module JOIN
FROM Sales.AgentCommissionConfig acc
LEFT JOIN Sales.SalesSegment ss ON acc.SalesSegmentId = ss.Id AND ss.IsDeleted = 0
LEFT JOIN Sales.MiscMaster mm ON acc.CommissionTypeId = mm.Id AND mm.IsDeleted = 0
WHERE acc.IsDeleted = 0
```

**Same-module FK rules:**
1. **Use `LEFT JOIN`** in Dapper SQL to fetch related names directly
2. **Always filter** `AND {JoinedTable}.IsDeleted = 0` on the JOIN condition
3. **Define navigation properties** in the domain entity (e.g., `public SalesSegment SalesSegment { get; set; } = null!;`)
4. **Create DB FK constraints** in EF Core configuration with `DeleteBehavior.Restrict`
5. **Validate FK existence** via direct SQL query in the query repository (e.g., `SalesSegmentExistsAsync`)
6. **No shared lookup interface needed** — the data is in the same schema and DB context

**When to use same-module JOINs vs cross-module lookups:**

| FK Type | Example | Approach | DB Constraint |
|---|---|---|---|
| Same-module FK | SalesSegmentId in AgentCommissionConfig | SQL JOIN in Dapper | Yes (`DeleteBehavior.Restrict`) |
| Same-module FK | CommissionTypeId (MiscMaster) | SQL JOIN in Dapper | Yes (`DeleteBehavior.Restrict`) |
| Cross-module FK | AgentId (PartyManagement) | Lookup interface (`IPartyLookup`) | No (no DB constraint) |
| Cross-module FK | ItemId (InventoryManagement) | Lookup interface (`IItemLookup`) | No (no DB constraint) |
| Cross-module FK | CurrencyId (UserManagement) | Lookup interface (`ICurrencyLookup`) | No (no DB constraint) |

---

### ❌ DO NOT Use Direct SQL JOINs for Cross-Module FKs

**Wrong:**
```sql
SELECT o.*, c.CompanyName
FROM Sales.SalesOrganisation o
INNER JOIN UserManagement.Company c ON o.CompanyId = c.CompanyId  -- ❌ Cross-module JOIN
```

### ✅ Use Lookup Services for Cross-Module FKs

**Correct:**
```csharp
// 1. Inject lookup interface from Contracts
private readonly ICompanyLookup _companyLookup;

// 2. Query your table only
const string sql = "SELECT * FROM Sales.SalesOrganisation WHERE Id = @Id AND IsDeleted = 0";
var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrganisationDto>(sql, new { Id = id });

// 3. Populate FK names via lookup
if (dto != null)
{
    var companies = await _companyLookup.GetAllCompanyAsync();
    var company = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId);
    dto.CompanyName = company?.CompanyName;
}
```

**Available Lookup Interfaces:**
- `ICompanyLookup` (UserManagement) → `Contracts.Interfaces.Lookups.Users`
- `IUnitLookup` (UserManagement) → `Contracts.Interfaces.Lookups.Users`
- `IDepartmentLookup` (UserManagement) → `Contracts.Interfaces.Lookups.Users`
- `IStateLookup`, `ICityLookup`, `ICountryLookup` (UserManagement)
- ... and more in `src/Shared/Contracts/Interfaces/Lookups/`

**Why?**
- **Decoupling:** Modules remain independent
- **Caching:** Lookups use `CachedLookupDecorator` (in-memory cache)
- **Testability:** Easy to mock interfaces
- **Maintainability:** Changes in one module don't break others

### 🆕 When a Required Lookup Does NOT Exist

If the entity you are implementing requires a cross-module FK whose lookup interface is **not yet in `src/Shared/Contracts/Interfaces/Lookups/`**, you MUST create it before generating the entity code.

**Steps to create a new lookup interface:**

#### Step 1 — Lookup DTO
**File:** `src/Shared/Contracts/Interfaces/Lookups/{SourceModule}/{LookupEntity}LookupDto.cs`

```csharp
namespace Contracts.Interfaces.Lookups.{SourceModule};

public sealed class {LookupEntity}LookupDto
{
    public int {LookupEntity}Id { get; set; }
    public string {LookupEntity}Code { get; set; }
    public string {LookupEntity}Name { get; set; }
}
```

#### Step 2 — Lookup Interface
**File:** `src/Shared/Contracts/Interfaces/Lookups/{SourceModule}/I{LookupEntity}Lookup.cs`

```csharp
namespace Contracts.Interfaces.Lookups.{SourceModule};

public interface I{LookupEntity}Lookup
{
    Task<IReadOnlyList<{LookupEntity}LookupDto>> GetAll{LookupEntity}Async();
}
```

#### Step 3 — Repository Implementation (in Source Module's Infrastructure)
**File:** `src/Modules/{SourceModule}/{SourceModule}.Infrastructure/Repositories/Lookups/{SourceModule}/{LookupEntity}LookupRepository.cs`

```csharp
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.{SourceModule};
using Contracts.Interfaces.Lookups.{SourceModule};
using Dapper;

namespace {SourceModule}.Infrastructure.Repositories.Lookups.{SourceModule};

internal sealed class {LookupEntity}LookupRepository : I{LookupEntity}Lookup
{
    private readonly IDbConnection _dbConnection;

    public {LookupEntity}LookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<List<{LookupEntity}LookupDto>> GetAll{LookupEntity}Async()
    {
        const string sql = @"
            SELECT Id, Code, Description
            FROM {Schema}.{LookupEntity}
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY Description ASC;
        ";

        var result = await _dbConnection.QueryAsync<{LookupEntity}LookupDto>(sql);
        return result.ToList();
    }
}
```

> ✅ **No per-lookup cached decorator needed.**
> The global `CachedLookupDecorator<TLookup>` (DispatchProxy in `Shared.Infrastructure`) automatically
> intercepts ALL interfaces whose name ends with `Lookup` and caches their results (30 min sliding / 24 h absolute).
> This is wired in `Program.cs` via `builder.Services.AddLookupCaching(...)`.

#### Step 4 — Register in Source Module's DI
**File:** `src/Modules/{SourceModule}/{SourceModule}.Infrastructure/DependencyInjection.cs`

Add the using directives:
```csharp
using Contracts.Interfaces.Lookups.{SourceModule};
using {SourceModule}.Infrastructure.Repositories.Lookups.{SourceModule};
```

Add the registration:
```csharp
// Lookup registration — caching is handled globally by AddLookupCaching() in Program.cs
services.AddScoped<I{LookupEntity}Lookup, {LookupEntity}LookupRepository>();
```

> ⚠️ **After creating a new lookup, build the Contracts project and the source module's Infrastructure project before proceeding with the consuming entity.**

---

## ✅ Validation Pattern (FluentValidation + Shared Validation Rules)

### Shared Validation Infrastructure

All validators use the **shared validation rule system** from `Shared.Validation.Common`:

- **`ValidationRuleLoader.LoadValidationRules()`** — loads rules from `validation-rules.json` (embedded resource)
- **`MaxLengthProvider`** — extracts max length from EF Core entity metadata (avoids hardcoded magic numbers)
- **`switch/case` pattern** — dynamically applies rules based on JSON rule names

**Required usings:**
```csharp
using SalesManagement.Presentation.Validation.Common;  // MaxLengthProvider
using Shared.Validation.Common;                         // ValidationRuleLoader, ValidationRule
```

**Available JSON rule types:**

| Rule Name | Purpose | Pattern | Error Template |
|---|---|---|---|
| `NotEmpty` | Required field checks | — | `"is required."` |
| `Alphanumeric` | Code field format — **letters and digits only, NO spaces** | `^[A-Za-z0-9]+$` | `"must be alphanumeric only."` |
| `MaxLength` | Max length (from EF metadata) | — | `"cannot be longer than"` |
| `AlreadyExists` | Uniqueness / composite key checks | — | `"already exists."` |
| `NotFound` | Entity existence (Update validators) | — | `"not found."` |
| `FKColumnDelete` | FK existence (same-module + cross-module) | — | `"{PropertyName} is inactive/deleted."` |
| `ByteValue` | IsActive validation (0 or 1) | — | `"must be either 0 or 1."` |
| `Pincode` | 6-digit pincode format | `^[0-9]{6}$` | `"must be a valid 6-digit pincode."` |
| `MobileNumber` | 10-digit mobile number format | `^[0-9]{10}$` | `"Mobile number must be exactly 10 digits."` |
| `Email` | Email address format (FluentValidation built-in) | — | `"Invalid email address format."` |
| `GstFormat` | 15-character GSTIN format | `^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$` | `"must be in the correct format (e.g., '22AAAAA1234A1Z5')."` |
| `Latitude` | Decimal latitude range (-90 to 90) | — | `"must be between -90 and 90."` |
| `Longitude` | Decimal longitude range (-180 to 180) | — | `"must be between -180 and 180."` |
| `GreaterThan` | Numeric field must be > 0 (detail-level: ItemId, Quantity, Rate, HSNId) | — | `"must be greater than zero."` |
| `GreaterThanOrEqualToZero` | Numeric field must be >= 0 (Discount, FreightCharges, OtherCharges) | — | `"must be zero or positive."` |
| `DateCompare` | Date field must be >= another date (e.g., ValidityDate >= QuotationDate) | — | `"must be greater than or equal to"` |

> ⚠️ **`Alphanumeric` pattern is `^[A-Za-z0-9]+$` — spaces are NOT alphanumeric.**
> The shared `validation-rules.json` previously had `^[A-Za-z0-9 ]+$` (with a stray space) which incorrectly allowed spaces in code fields. This was a bug — it has been fixed. Code fields (e.g. `SalesOrganisationCode`, `BusinessUnitCode`) must reject spaces.

### Create Validator Structure

```csharp
public class Create{EntityName}CommandValidator : AbstractValidator<Create{EntityName}Command>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly I{EntityName}QueryRepository _queryRepo;

    public Create{EntityName}CommandValidator(
        MaxLengthProvider maxLengthProvider,
        I{EntityName}QueryRepository queryRepo)
    {
        _queryRepo = queryRepo;

        // Extract max lengths from EF Core metadata — no hardcoded magic numbers
        var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.{EntityName}>("{UniqueCode}") ?? 20;
        var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.{EntityName}>("{Name}") ?? 100;

        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || !_validationRules.Any())
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        foreach (var rule in _validationRules)
        {
            switch (rule.Rule)
            {
                case "NotEmpty":
                    RuleFor(x => x.{UniqueCode})
                        .NotNull()
                        .WithMessage($"{nameof(Create{EntityName}Command.{UniqueCode})} {rule.Error}")
                        .NotEmpty()
                        .WithMessage($"{nameof(Create{EntityName}Command.{UniqueCode})} {rule.Error}");

                    RuleFor(x => x.{Name})
                        .NotNull()
                        .WithMessage($"{nameof(Create{EntityName}Command.{Name})} {rule.Error}")
                        .NotEmpty()
                        .WithMessage($"{nameof(Create{EntityName}Command.{Name})} {rule.Error}");
                    break;

                case "Alphanumeric":
                    RuleFor(x => x.{UniqueCode})
                        .Matches(rule.Pattern)
                        .WithMessage($"{nameof(Create{EntityName}Command.{UniqueCode})} {rule.Error}")
                        .When(x => !string.IsNullOrWhiteSpace(x.{UniqueCode}));
                    break;

                case "MaxLength":
                    RuleFor(x => x.{UniqueCode})
                        .MaximumLength(maxLengthCode)
                        .WithMessage($"{nameof(Create{EntityName}Command.{UniqueCode})} {rule.Error} {maxLengthCode} characters.");

                    RuleFor(x => x.{Name})
                        .MaximumLength(maxLengthName)
                        .WithMessage($"{nameof(Create{EntityName}Command.{Name})} {rule.Error} {maxLengthName} characters.");
                    break;

                case "FKColumnDelete":
                    RuleFor(x => x.{ForeignKey})
                        .MustAsync(async (id, ct) => await _queryRepo.{Referenced}ExistsAsync(id))
                        .WithMessage($"{nameof(Create{EntityName}Command.{ForeignKey})} {rule.Error}")
                        .When(x => x.{ForeignKey} > 0);
                    break;

                case "AlreadyExists":
                    RuleFor(x => x.{UniqueCode})
                        .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code))
                        .WithMessage($"{nameof(Create{EntityName}Command.{UniqueCode})} {rule.Error}")
                        .When(x => !string.IsNullOrWhiteSpace(x.{UniqueCode}));
                    break;

                default:
                    break;
            }
        }
    }
}
```

### Update Validator Structure

```csharp
public class Update{EntityName}CommandValidator : AbstractValidator<Update{EntityName}Command>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly I{EntityName}QueryRepository _queryRepo;

    public Update{EntityName}CommandValidator(
        MaxLengthProvider maxLengthProvider,
        I{EntityName}QueryRepository queryRepo)
    {
        _queryRepo = queryRepo;

        var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.{EntityName}>("{Name}") ?? 100;

        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || !_validationRules.Any())
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        foreach (var rule in _validationRules)
        {
            switch (rule.Rule)
            {
                case "NotEmpty":
                    RuleFor(x => x.{Name})
                        .NotNull()
                        .WithMessage($"{nameof(Update{EntityName}Command.{Name})} {rule.Error}")
                        .NotEmpty()
                        .WithMessage($"{nameof(Update{EntityName}Command.{Name})} {rule.Error}");
                    break;

                case "MaxLength":
                    RuleFor(x => x.{Name})
                        .MaximumLength(maxLengthName)
                        .WithMessage($"{nameof(Update{EntityName}Command.{Name})} {rule.Error} {maxLengthName} characters.");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .GreaterThan(0).WithMessage("Valid Id is required.")
                        .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                        .WithMessage($"{EntityName} {rule.Error}");
                    break;

                case "FKColumnDelete":
                    RuleFor(x => x.{ForeignKey})
                        .MustAsync(async (id, ct) => await _queryRepo.{Referenced}ExistsAsync(id))
                        .WithMessage($"{nameof(Update{EntityName}Command.{ForeignKey})} {rule.Error}")
                        .When(x => x.{ForeignKey} > 0);
                    break;

                case "ByteValue":
                    RuleFor(x => x.IsActive)
                        .InclusiveBetween(0, 1)
                        .WithMessage($"{nameof(Update{EntityName}Command.IsActive)} {rule.Error}");
                    break;

                default:
                    break;
            }
        }
    }
}
```

### Delete Validator Structure

```csharp
public class Delete{EntityName}CommandValidator : AbstractValidator<Delete{EntityName}Command>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly I{EntityName}QueryRepository _queryRepository;

    public Delete{EntityName}CommandValidator(I{EntityName}QueryRepository queryRepository)
    {
        _queryRepository = queryRepository;
        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || _validationRules.Count == 0)
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        foreach (var rule in _validationRules)
        {
            switch (rule.Rule)
            {
                case "NotEmpty":
                    RuleFor(x => x.Id)
                        .NotEmpty()
                        .WithMessage($"{nameof(Delete{EntityName}Command.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                        .WithMessage($"{EntityName} {rule.Error}");
                    break;

                default:
                    break;
            }
        }
    }
}
```

⚠️ **Delete Validator Notes:**
- **Only injects `I{EntityName}QueryRepository`** — no `MaxLengthProvider` needed (no string length checks)
- **`NotEmpty`** ensures `Id` is not zero/default
- **`NotFound`** ensures the entity exists before attempting delete
- **`SoftDelete`** case may be added if the entity has FK dependents that must be checked first (add `SoftDeleteValidationAsync` to the query repository interface when needed)

⚠️ **Validator Rules:**
- **ALL validation logic lives in validators** — never in controllers or handlers
- **ALWAYS use `ValidationRuleLoader` + `MaxLengthProvider`** — never hardcode validation rules or max lengths
- **ALWAYS inject `MaxLengthProvider`** as the first constructor parameter (Create/Update only)
- Error messages use `$"{nameof(Command.Property)} {rule.Error}"` pattern from shared JSON config
- Validators can inject **cross-module lookup interfaces** (e.g., `ICurrencyLookup`) for FK validation in `FKColumnDelete` case
- Handlers assume validation has passed — they only do: map → persist → audit → return

**Switch case mapping:**

| Validation Need | JSON Rule Case | FluentValidation Method | Validator |
|---|---|---|---|
| Required fields | `NotEmpty` | `.NotNull().NotEmpty()` | Create/Update |
| Id required (Delete) | `NotEmpty` | `.NotEmpty()` | Delete |
| Alphanumeric format | `Alphanumeric` | `.Matches(rule.Pattern)` with `.When()` guard | Create |
| Max length (from EF) | `MaxLength` | `.MaximumLength(maxLengthVar)` | Create/Update |
| FK existence | `FKColumnDelete` | `.MustAsync(... ExistsAsync ...)` | Create/Update |
| Cross-module FK | `FKColumnDelete` (same case) | `.MustAsync(... lookup.GetByIdsAsync ...)` | Create/Update |
| Uniqueness | `AlreadyExists` | `.MustAsync(... AlreadyExistsAsync ...)` | Create |
| Composite key unique | `AlreadyExists` (same case) | `.MustAsync(... CompositeKeyExistsAsync ...)` | Create |
| Entity exists (Update/Delete) | `NotFound` | `.MustAsync(... NotFoundAsync ...)` | Update/Delete |
| FK dependent check (Delete) | `SoftDelete` | `.MustAsync(... SoftDeleteValidationAsync ...)` | Delete (optional) |
| IsActive range (Update) | `ByteValue` | `.InclusiveBetween(0, 1)` | Update |
| 6-digit pincode | `Pincode` | `.Matches(rule.Pattern)` with `.When()` guard | Create/Update |
| 10-digit mobile number | `MobileNumber` | `.Matches(rule.Pattern)` with `.When()` guard | Create/Update |
| Email address format | `Email` | `.EmailAddress()` with `.When()` guard | Create/Update |
| GSTIN format | `GstFormat` | `.Matches(rule.Pattern)` with `.When()` guard | Create/Update |
| Latitude range (-90 to 90) | `Latitude` | `.InclusiveBetween(-90m, 90m)` with `.When()` guard | Create/Update |
| Longitude range (-180 to 180) | `Longitude` | `.InclusiveBetween(-180m, 180m)` with `.When()` guard | Create/Update |
| Numeric > 0 (detail-level) | `GreaterThan` | `.GreaterThan(0)` via `RuleForEach` + `ChildRules` | Create/Update |
| Numeric >= 0 (charges, discount) | `GreaterThanOrEqualToZero` | `.GreaterThanOrEqualTo(0)` | Create/Update |
| Date >= another date | `DateCompare` | `.GreaterThanOrEqualTo(x => x.OtherDate)` | Create/Update |

### Global Validation Pipeline

**Already configured in `Program.cs`:**
```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

- All MediatR requests auto-validated
- `ValidationException` → 400 Bad Request (via `GlobalExceptionMiddleware`)
- **DO NOT register `ValidationBehavior` in modules** — it's global

---

## 🎭 Controller Pattern

```csharp
// ❌ NO [Authorize] attribute — authentication is handled globally, not per-controller
[Route("api/[controller]")]
public class {EntityName}Controller : ApiControllerBase
{
    public {EntityName}Controller(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> GetAll{EntityName}Async(
        [FromQuery] int PageNumber,
        [FromQuery] int PageSize,
        [FromQuery] string SearchTerm = null)
    {
        var result = await Mediator.Send(new GetAll{EntityName}Query
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
            SearchTerm = SearchTerm
        });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = result.Data,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get{EntityName}ByIdAsync(int id)
    {
        var result = await Mediator.Send(new Get{EntityName}ByIdQuery { Id = id });
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = result
        });
    }

    [HttpGet("by-name")]
    public async Task<IActionResult> Get{EntityName}AutoCompleteAsync([FromQuery] string term = null) { /* ... */ }

    [HttpPost]
    public async Task<IActionResult> Create{EntityName}([FromBody] Create{EntityName}Command command)
    {
        // ❌ NO ModelState.IsValid check — FluentValidation handles ALL validation via ValidationBehavior pipeline
        var result = await Mediator.Send(command);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            isSuccess = result.IsSuccess,
            message = result.Message,
            data = result.Data
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update{EntityName}([FromBody] Update{EntityName}Command command) { /* ... */ }

    [HttpDelete]
    public async Task<IActionResult> Delete{EntityName}(int id) { /* ... */ }
}
```

⚠️ **Controller Rules:**
- **NO `[Authorize]`** — authentication is handled globally via middleware, not per-controller
- **Simple HTTP verbs only** — no route suffixes on action attributes
- **NO `ModelState.IsValid` checks** — FluentValidation via `ValidationBehavior` pipeline handles ALL validation before the handler executes
- **Controllers are thin pass-through layers** — receive request → `Mediator.Send()` → return response

**Standard Endpoints:**
- `GET api/{Entity}` — List all (paginated) → `[HttpGet]`
- `GET api/{Entity}/{id}` — Get by ID → `[HttpGet("{id}")]`
- `GET api/{Entity}/by-name` — Autocomplete dropdown → `[HttpGet("by-name")]`
- `POST api/{Entity}` — Create new → `[HttpPost]`
- `PUT api/{Entity}` — Update existing → `[HttpPut]`
- `DELETE api/{Entity}` — Soft delete → `[HttpDelete]`

---

## 🗄️ Entity Configuration (EF Core)

```csharp
public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // Value converters for enums
        var statusConverter = new ValueConverter<Status, bool>(
            v => v == Status.Active,
            v => v ? Status.Active : Status.Inactive
        );

        var isDeleteConverter = new ValueConverter<IsDelete, bool>(
            v => v == IsDelete.Deleted,
            v => v ? IsDelete.Deleted : IsDelete.NotDeleted
        );

        // Table mapping
        builder.ToTable("{EntityName}", "{Schema}");
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.{UniqueCode})
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(t => t.{Name})
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasColumnType("bit")
            .HasConversion(statusConverter)
            .IsRequired();

        builder.Property(b => b.IsDeleted)
            .HasColumnType("bit")
            .HasConversion(isDeleteConverter)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.{UniqueCode}).IsUnique();
        builder.HasIndex(t => t.{ForeignKey});

        // Foreign keys (same-module only)
        // For cross-module FKs, do NOT create FK constraint
    }
}
```

**Apply in `ApplicationDbContext.OnModelCreating`:**
```csharp
modelBuilder.ApplyConfiguration(new {EntityName}Configuration());
```

---

## 📣 Domain Events (Audit Logging)

### Event Structure (Shared)

```csharp
// Use existing AuditLogsDomainEvent - do NOT create custom events
var auditEvent = new AuditLogsDomainEvent(
    actionDetail: "Create",       // Create, Update, Delete, SoftDelete
    actionCode: "{ENTITY}_CREATE", // Unique action code
    actionName: request.{UniqueCode},
    details: $"{EntityName} '{request.{UniqueCode}}' created successfully with Id {newId}.",
    module: "{EntityName}"
);
await _mediator.Publish(auditEvent, cancellationToken);
```

**Event Handler:** Already implemented in each module's `DomainEventHandler.cs`
- Stores logs in MongoDB `AuditLogs` collection
- Captures user info, IP, timestamp, browser, OS

---

## 🚫 Soft Delete (Never Physical Delete)

### Implementation

**Command Repository:**
```csharp
public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
{
    var existing = await _dbContext.{EntityName}
        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

    if (existing == null)
        return false;

    existing.IsDeleted = IsDelete.Deleted;
    _dbContext.{EntityName}.Update(existing);
    await _dbContext.SaveChangesAsync(ct);
    return true;
}
```

**All Queries MUST Filter:**
```sql
WHERE IsDeleted = 0  -- ⚠️ CRITICAL: Always filter soft-deleted records
```

**Autocomplete MUST Filter:**
```sql
WHERE IsActive = 1 AND IsDeleted = 0  -- Only show active, non-deleted records
```

---

## 🔒 Immutability Rules

### Unique Code Fields

**Examples:** `CustomerCode`, `ProductCode`, `SalesOrganisationCode`

**Rules:**
1. ✅ Can be set on **Create**
2. ❌ Cannot be changed on **Update**
3. ✅ Must be **unique** (validated via `AlreadyExistsAsync`)
4. ✅ Must be **alphanumeric** only

**Implementation:**
```csharp
// Create command - includes code
public class CreateCustomerCommand : IRequest<ApiResponseDTO<int>>
{
    public string CustomerCode { get; set; }  // ✅ Included
    public string CustomerName { get; set; }
}

// Update command - excludes code
public class UpdateCustomerCommand : IRequest<ApiResponseDTO<int>>
{
    public int Id { get; set; }
    // public string CustomerCode { get; set; }  // ❌ NOT included - immutable
    public string CustomerName { get; set; }
    public int IsActive { get; set; }
}
```

---

## 📦 DTO Organization

### Centralized DTOs

**Location:** `{Module}.Application/{EntityName}/Dto/`

**Files:**
- `{EntityName}Dto.cs` - Full DTO with all fields (for GetAll, GetById)
- `{EntityName}LookupDto.cs` - Lightweight DTO (Id, Code, Name) for autocomplete

**Example:**
```csharp
// Full DTO
public class SalesOrganisationDto
{
    public int Id { get; set; }
    public string SalesOrganisationCode { get; set; }
    public string SalesOrganisationName { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }  // Populated via lookup
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    // ... audit fields
}

// Lookup DTO
public sealed class SalesOrganisationLookupDto
{
    public int Id { get; set; }
    public string SalesOrganisationCode { get; set; }
    public string SalesOrganisationName { get; set; }
}
```

**Update All Usings:**
```csharp
using {Module}.Application.{EntityName}.Dto;
```

### ⚠️ Do NOT Duplicate LookupDto When It Already Exists in Contracts

If `{EntityName}LookupDto` already exists in `Contracts.Dtos.Lookups.{Module}/`, do **NOT** create a second `{EntityName}LookupDto.cs` inside `{Module}.Application/{EntityName}/Dto/`.

**Correct Application Dto folder — only one file:**
- ✅ `{EntityName}Dto.cs` — full DTO (for GetAll, GetById)
- ❌ `{EntityName}LookupDto.cs` — do NOT create; already defined in Contracts

**Example (wrong vs correct):**
```csharp
// ❌ WRONG — duplicate lookup DTO in Application layer
namespace ProductionManagement.Application.CountMaster.Dto
{
    public sealed class CountMasterLookupDto { ... }
}

// ✅ CORRECT — canonical lookup DTO lives in Contracts only
namespace Contracts.Dtos.Lookups.Production
{
    public sealed class CountMasterLookupDto { ... }
}
```

**AutoComplete query handlers must use the Contracts namespace:**
```csharp
using Contracts.Dtos.Lookups.Production; // ✅ correct
// NOT: using ProductionManagement.Application.CountMaster.Dto; // ❌
```

### 📋 What to Create — Contracts vs Application Layer

Use this table every time you add a new entity:

**When creating in Contracts (cross-module lookup):**

| File | Namespace | Class Name |
|---|---|---|
| `Contracts.Dtos.Lookups.{Module}/{EntityName}LookupDto.cs` | `Contracts.Dtos.Lookups.{Module}` | `{EntityName}LookupDto` |
| `Contracts.Interfaces.Lookups.{Module}/I{EntityName}Lookup.cs` | `Contracts.Interfaces.Lookups.{Module}` | `I{EntityName}Lookup` |

**When creating in Application layer:**

| File | Namespace | Class Name |
|---|---|---|
| `{Module}.Application/{EntityName}/Dto/{EntityName}Dto.cs` | `{Module}.Application.{EntityName}.Dto` | `{EntityName}Dto` |
| ❌ `{EntityName}LookupDto.cs` | — | **DO NOT CREATE** — use Contracts |

**Summary rule:**
- `{EntityName}LookupDto` → **Contracts only** (one place, shared across modules)
- `{EntityName}Dto` → **Application layer only** (full DTO with all fields)

---

## 🧪 EF Core Migrations

### Generate Migration

```bash
cd src/Modules/{Module}/{Module}.Infrastructure

dotnet ef migrations add {EntityName}Master --startup-project ../../../BSOFT.Api
```

### Apply Migration

```bash
dotnet ef database update --startup-project ../../../BSOFT.Api
```

### Design-Time Factory

Each module has `DesignTimeDbContextFactory.cs` that:
- Loads configuration from `BSOFT.Api/appsettings.json`
- Injects dummy `IIPAddressService`, `ITimeZoneService` for migrations
- Uses hardcoded connection string for dev environment

---

## 🔧 Dependency Injection

### Repository Registration

**File:** `{Module}.Infrastructure/DependencyInjection.cs`

```csharp
// Command repositories
services.AddScoped<I{EntityName}CommandRepository, {EntityName}CommandRepository>();

// Query repositories
services.AddScoped<I{EntityName}QueryRepository, {EntityName}QueryRepository>();
```

### Shared Services (Already Registered)

- `ICompanyLookup` (UserManagement)
- `IIPAddressService` (captures user info)
- `ITimeZoneService` (system timezone)
- `IMediator` (MediatR pipeline)
- `IMapper` (AutoMapper)
- `IMemoryCache` (caching)
- `IDbConnection` (Dapper)
- `ApplicationDbContext` (EF Core)

---

## 📝 Naming Conventions

### Files
- Commands: `{Verb}{EntityName}Command.cs`
- Handlers: `{Verb}{EntityName}CommandHandler.cs`
- Queries: `Get{EntityName}{Criteria}Query.cs`
- DTOs: `{EntityName}Dto.cs`, `{EntityName}LookupDto.cs`
- Repositories: `{EntityName}{CommandOrQuery}Repository.cs`
- Validators: `{Verb}{EntityName}CommandValidator.cs`

### Classes
- Entities: `{EntityName}` (e.g., `SalesOrganisation`)
- Commands: `{Verb}{EntityName}Command` (e.g., `CreateSalesOrganisationCommand`)
- Handlers: `{Command}Handler` (e.g., `CreateSalesOrganisationCommandHandler`)
- Controllers: `{EntityName}Controller` (e.g., `SalesOrganisationController`)

### Database
- Schema: `{ModulePrefix}` (e.g., `Sales`, `Purchase`, `Inventory`)
- Table: `{EntityName}` (e.g., `SalesOrganisation`)
- Columns: PascalCase (e.g., `SalesOrganisationCode`, `CompanyId`)

---

## 🛠️ Build & Test

### Build Commands

```bash
cd d:/BSOFT

# Build specific module
dotnet build "src/Modules/{Module}/Core/{Module}.Application/{Module}.Application.csproj" --no-restore
dotnet build "src/Modules/{Module}/{Module}.Infrastructure/{Module}.Infrastructure.csproj" --no-restore
dotnet build "src/Modules/{Module}/{Module}.Presentation/{Module}.Presentation.csproj" --no-restore

# Build entire solution (app must be stopped)
dotnet build BSOFT.sln --no-restore
```

**Expected Result:** `0 Warning(s)`, `0 Error(s)`

### Test Commands

```bash
# Run unit tests for a module
dotnet test "src/tests/{Module}.UnitTests/{Module}.UnitTests.csproj" --no-build

# Run integration tests for a module
dotnet test "src/tests/{Module}.IntegrationTests/{Module}.IntegrationTests.csproj" --no-build

# Run all tests in solution
dotnet test BSOFT.sln

# Run with coverage report
dotnet test "src/tests/{Module}.UnitTests/{Module}.UnitTests.csproj" --collect:"XPlat Code Coverage"
```

### Common Issues

**Issue:** DLL files locked
**Fix:** Stop running application before building

**Issue:** `CachedLookupDecorator` error "base type cannot be sealed"
**Fix:** Remove `sealed` keyword from `CachedLookupDecorator<T>` class definition

**Issue:** Integration tests fail with connection error
**Fix:** Verify SQL Server at `192.168.1.126` is reachable and test DB credentials are correct

---

## 🧪 Unit Testing

### Overview

Unit tests isolate business logic (command/query handlers and validators) using mocked dependencies. They are fast, run in parallel, and require no database.

### Test Project Location & Naming

```
src/tests/
└── {ModuleName}.UnitTests/
    └── {ModuleName}.UnitTests.csproj
```

**Example:** `src/tests/SalesManagement.UnitTests/`

### Required NuGet Packages

```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="FluentValidation" Version="11.11.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
```

### Unit Test Directory Structure

```
{ModuleName}.UnitTests/
├── Application/
│   └── {EntityName}/
│       ├── Commands/
│       │   ├── Create{EntityName}CommandHandlerTests.cs
│       │   ├── Update{EntityName}CommandHandlerTests.cs
│       │   └── Delete{EntityName}CommandHandlerTests.cs
│       └── Queries/
│           ├── GetAll{EntityName}QueryHandlerTests.cs
│           ├── Get{EntityName}ByIdQueryHandlerTests.cs
│           └── Get{EntityName}AutoCompleteQueryHandlerTests.cs
├── Controllers/
│   └── {EntityName}ControllerTests.cs
├── Domain/
│   ├── {EntityName}EntityTests.cs       ← one file per entity
│   └── BaseEntityAuditFieldsTests.cs
├── Validators/
│   └── {EntityName}/
│       ├── Create{EntityName}CommandValidatorTests.cs
│       └── Update{EntityName}CommandValidatorTests.cs
├── TestData/
│   └── {EntityName}Builders.cs
└── Usings.cs
```

### Global Usings File

**File:** `Usings.cs`

```csharp
global using Xunit;
global using FluentAssertions;
global using Moq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
```

### Test Data Builder Pattern

**File:** `TestData/{EntityName}Builders.cs`

```csharp
public static class {EntityName}Builders
{
    public static Create{EntityName}Command ValidCreateCommand(
        string code = "CODE001",
        string name = "Test {EntityName}",
        int companyId = 1) =>
        new Create{EntityName}Command
        {
            {EntityName}Code = code,
            {EntityName}Name = name,
            CompanyId = companyId
        };

    public static Update{EntityName}Command ValidUpdateCommand(
        int id = 1,
        string name = "Updated {EntityName}",
        int companyId = 1,
        int isActive = 1) =>
        new Update{EntityName}Command
        {
            Id = id,
            {EntityName}Name = name,
            CompanyId = companyId,
            IsActive = isActive
        };

    public static {EntityName}Dto ValidDto(
        int id = 1,
        string code = "CODE001",
        string name = "Test {EntityName}",
        int companyId = 1,
        string companyName = "Test Company") =>
        new {EntityName}Dto
        {
            Id = id,
            {EntityName}Code = code,
            {EntityName}Name = name,
            CompanyId = companyId,
            CompanyName = companyName,
            IsActive = true,
            IsDeleted = false
        };

    public static IReadOnlyList<{EntityName}LookupDto> ValidLookupList() =>
        new List<{EntityName}LookupDto>
        {
            new {EntityName}LookupDto { Id = 1, {EntityName}Code = "CODE001", {EntityName}Name = "Test {EntityName}" }
        };

    public static {Module}.Domain.Entities.{EntityName} ValidEntity(int id = 1) =>
        new {Module}.Domain.Entities.{EntityName}
        {
            Id = id,
            {EntityName}Code = "CODE001",
            {EntityName}Name = "Test {EntityName}",
            CompanyId = 1,
            IsActive = BaseEntity.Status.Active,
            IsDeleted = BaseEntity.IsDelete.NotDeleted
        };
}
```

### Command Handler Unit Test Pattern

**File:** `Application/{EntityName}/Commands/Create{EntityName}CommandHandlerTests.cs`

```csharp
public sealed class Create{EntityName}CommandHandlerTests
{
    private readonly Mock<I{EntityName}CommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<I{EntityName}QueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

    private Create{EntityName}CommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

    private void SetupHappyPath(Create{EntityName}Command command, int newId = 1)
    {
        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<{Module}.Domain.Entities.{EntityName}>()))
            .ReturnsAsync(newId);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = {EntityName}Builders.ValidCreateCommand();
        SetupHappyPath(command, newId: 1);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewId()
    {
        var command = {EntityName}Builders.ValidCreateCommand();
        SetupHappyPath(command, newId: 42);
        var sut = CreateSut();

        var result = await sut.Handle(command, CancellationToken.None);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnce()
    {
        var command = {EntityName}Builders.ValidCreateCommand();
        SetupHappyPath(command);
        var sut = CreateSut();

        await sut.Handle(command, CancellationToken.None);

        _mockCommandRepo.Verify(
            r => r.CreateAsync(It.IsAny<{Module}.Domain.Entities.{EntityName}>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesAuditEvent()
    {
        var command = {EntityName}Builders.ValidCreateCommand();
        SetupHappyPath(command);
        var sut = CreateSut();

        await sut.Handle(command, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "{ENTITY}_CREATE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCorrectEntityState()
    {
        var command = {EntityName}Builders.ValidCreateCommand();
        {Module}.Domain.Entities.{EntityName} capturedEntity = null;

        _mockCommandRepo
            .Setup(r => r.CreateAsync(It.IsAny<{Module}.Domain.Entities.{EntityName}>()))
            .Callback<{Module}.Domain.Entities.{EntityName}>(e => capturedEntity = e)
            .ReturnsAsync(1);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(command, CancellationToken.None);

        capturedEntity.IsActive.Should().Be(BaseEntity.Status.Active);
        capturedEntity.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
    }
}
```

### Delete Command Handler Unit Test Pattern

```csharp
[Fact]
public async Task Handle_NonExistentId_ThrowsExceptionRules()
{
    _mockCommandRepo
        .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var sut = CreateSut();
    Func<Task> act = async () => await sut.Handle(
        new Delete{EntityName}Command(99), CancellationToken.None);

    await act.Should().ThrowAsync<ExceptionRules>()
        .WithMessage("*not found*");
}

[Fact]
public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
{
    _mockCommandRepo
        .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    var sut = CreateSut();
    try { await sut.Handle(new Delete{EntityName}Command(99), CancellationToken.None); }
    catch { /* expected */ }

    _mockMediator.Verify(
        m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
        Times.Never);
}
```

### Query Handler Unit Test Pattern

**File:** `Application/{EntityName}/Queries/GetAll{EntityName}QueryHandlerTests.cs`

```csharp
public sealed class GetAll{EntityName}QueryHandlerTests
{
    private readonly Mock<I{EntityName}QueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private GetAll{EntityName}QueryHandler CreateSut() =>
        new(_mockQueryRepo.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<{EntityName}Dto> { {EntityName}Builders.ValidDto() };
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((dtoList, 1));

        var result = await CreateSut().Handle(
            new GetAll{EntityName}Query { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        var dtoList = new List<{EntityName}Dto> { {EntityName}Builders.ValidDto() };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "search"))
            .ReturnsAsync((dtoList, 11));

        var result = await CreateSut().Handle(
            new GetAll{EntityName}Query { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(11);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
            .ReturnsAsync((new List<{EntityName}Dto>(), 0));

        var result = await CreateSut().Handle(
            new GetAll{EntityName}Query { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
```

### Validator Unit Test Pattern

**File:** `Validators/{EntityName}/Create{EntityName}CommandValidatorTests.cs`

```csharp
public sealed class Create{EntityName}CommandValidatorTests
{
    private readonly Mock<I{EntityName}QueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

    private Create{EntityName}CommandValidator CreateValidator() =>
        new(_mockQueryRepo.Object);

    // ⚠️ FluentValidation runs ALL rules — setup ALL async mocks
    private void SetupAllAsyncMocks(string code = "CODE001", int companyId = 1)
    {
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.CompanyExistsAsync(companyId)).ReturnsAsync(true);
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var command = {EntityName}Builders.ValidCreateCommand();
        SetupAllAsyncMocks(command.{EntityName}Code, command.CompanyId);

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_EmptyCode_FailsValidation(string code)
    {
        var command = {EntityName}Builders.ValidCreateCommand(code: code);
        // No async setup needed - sync rule fails first

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.{EntityName}Code)
              .WithErrorMessage("{EntityName} Code is required.");
    }

    [Fact]
    public async Task Validate_DuplicateCode_FailsValidation()
    {
        var command = {EntityName}Builders.ValidCreateCommand(code: "EXIST001");
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST001", null)).ReturnsAsync(true);
        _mockQueryRepo.Setup(r => r.CompanyExistsAsync(command.CompanyId)).ReturnsAsync(true);

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.{EntityName}Code)
              .WithErrorMessage("{EntityName} Code already exists.");
    }

    [Fact]
    public async Task Validate_InvalidCompanyId_FailsValidation()
    {
        var command = {EntityName}Builders.ValidCreateCommand(companyId: 999);
        _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(command.{EntityName}Code, null)).ReturnsAsync(false);
        _mockQueryRepo.Setup(r => r.CompanyExistsAsync(999)).ReturnsAsync(false);

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.CompanyId)
              .WithErrorMessage("Company does not exist in Company Master.");
    }

    [Theory]
    [InlineData("CODE-01")]   // hyphen       — not alphanumeric → fails
    [InlineData("CODE 01")]   // space        — not alphanumeric → fails (pattern is ^[A-Za-z0-9]+$, no spaces)
    [InlineData("CODE@01")]   // special char — not alphanumeric → fails
    public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
    {
        var command = {EntityName}Builders.ValidCreateCommand(code: code);
        SetupAllAsyncMocks(code, command.CompanyId);

        var result = await CreateValidator().TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.{EntityName}Code)
              .WithErrorMessage("{EntityName} Code must be alphanumeric.");
    }
}
```

---

## 🔗 Integration Testing

### Overview

Integration tests validate repository implementations (EF Core command repos and Dapper query repos) against a real SQL Server database. They are slower and run sequentially.

### Test Project Location & Naming

```
src/tests/
└── {ModuleName}.IntegrationTests/
    └── {ModuleName}.IntegrationTests.csproj
```

**Example:** `src/tests/SalesManagement.IntegrationTests/`

### Required NuGet Packages

```xml
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Dapper" Version="2.1.44" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

### Integration Test Directory Structure

```
{ModuleName}.IntegrationTests/
├── Common/
│   └── DbFixture.cs                    # Shared database setup/teardown
├── Repositories/
│   └── {EntityName}/
│       ├── {EntityName}CommandRepositoryTests.cs
│       └── {EntityName}QueryRepositoryTests.cs
├── appsettings.Integration.json        # Test DB connection string
└── Usings.cs
```

### DbFixture: Shared Database Setup

**File:** `Common/DbFixture.cs`

```csharp
[CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DbFixture> { }

public class DbFixture : IAsyncLifetime
{
    private const string MasterConnection =
        "Server=192.168.1.126;Database=master;User Id=developer;Password=Dev@#$456;" +
        "Encrypt=False;TrustServerCertificate=True;";
    private const string TestDbConnection =
        "Server=192.168.1.126;Database={Module}_TestDb;User Id=developer;Password=Dev@#$456;" +
        "Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;";

    public string ConnectionString => TestDbConnection;
    public ApplicationDbContext DbContext { get; private set; }

    public async Task InitializeAsync()
    {
        await RecreateDatabaseAsync();
        ConfigureMocks();
        await CreateDbContextAsync();
        await EnsureSchemaAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public ApplicationDbContext CreateFreshDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(TestDbConnection)
            .Options;
        return new ApplicationDbContext(options, _mockIpService.Object, _mockTimeZone.Object);
    }

    private Mock<IIPAddressService> _mockIpService;
    private Mock<ITimeZoneService> _mockTimeZoneService;

    private void ConfigureMocks()
    {
        _mockIpService = new Mock<IIPAddressService>();
        _mockIpService.Setup(s => s.GetIPAddress()).Returns("127.0.0.1");
        _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
        _mockIpService.Setup(s => s.GetUserId()).Returns(1);

        _mockTimeZoneService = new Mock<ITimeZoneService>();
        _mockTimeZoneService.Setup(s => s.GetCurrentTime()).Returns(DateTimeOffset.UtcNow);
    }

    private async Task RecreateDatabaseAsync()
    {
        await using var conn = new SqlConnection(MasterConnection);
        await conn.OpenAsync();
        const string sql = """
            IF EXISTS (SELECT * FROM sys.databases WHERE name = N'{Module}_TestDb')
                DROP DATABASE [{Module}_TestDb];
            CREATE DATABASE [{Module}_TestDb];
            """;
        await conn.ExecuteAsync(sql);
    }

    private async Task EnsureSchemaAsync()
    {
        await using var conn = new SqlConnection(TestDbConnection);
        await conn.OpenAsync();
        await conn.ExecuteAsync("IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{Schema}') EXEC('CREATE SCHEMA [{Schema}]')");
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null) await DbContext.DisposeAsync();
    }
}
```

### Command Repository Integration Test Pattern

**File:** `Repositories/{EntityName}/{EntityName}CommandRepositoryTests.cs`

```csharp
[Collection("DatabaseCollection")]
public sealed class {EntityName}CommandRepositoryTests
{
    private readonly DbFixture _fixture;

    public {EntityName}CommandRepositoryTests(DbFixture fixture)
    {
        _fixture = fixture;
    }

    private {EntityName}CommandRepository CreateRepository(ApplicationDbContext ctx) =>
        new(ctx);

    private static {Module}.Domain.Entities.{EntityName} BuildEntity(
        string code = "CODE001",
        string name = "Test {EntityName}",
        int companyId = 1) =>
        new {Module}.Domain.Entities.{EntityName}
        {
            {EntityName}Code = code,
            {EntityName}Name = name,
            CompanyId = companyId,
            IsActive = BaseEntity.Status.Active,
            IsDeleted = BaseEntity.IsDelete.NotDeleted
        };

    private async Task ClearTableAsync(ApplicationDbContext ctx) =>
        await ctx.Database.ExecuteSqlRawAsync("DELETE FROM {Schema}.{EntityName}");

    // --- CREATE ---

    [Fact]
    public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);

        var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

        newId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_Should_Persist_Fields_Correctly()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);

        var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001", "Test Org", 1));
        ctx.ChangeTracker.Clear();

        var saved = await ctx.{EntityName}.FirstOrDefaultAsync(x => x.Id == newId);

        saved.{EntityName}Code.Should().Be("CODE001");
        saved.{EntityName}Name.Should().Be("Test Org");
        saved.CompanyId.Should().Be(1);
        saved.IsActive.Should().Be(BaseEntity.Status.Active);
        saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
    }

    [Fact]
    public async Task CreateAsync_Should_Populate_Audit_Fields()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);

        var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
        ctx.ChangeTracker.Clear();

        var saved = await ctx.{EntityName}.FirstOrDefaultAsync(x => x.Id == newId);

        saved.CreatedBy.Should().Be(1);
        saved.CreatedByName.Should().Be("test-user");
        saved.CreatedIP.Should().Be("127.0.0.1");
        saved.CreatedDate.Should().NotBeNull();
    }

    // --- UPDATE ---

    [Fact]
    public async Task UpdateAsync_Should_Persist_Changes()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);
        var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
        ctx.ChangeTracker.Clear();

        var entity = await ctx.{EntityName}.FirstAsync(x => x.Id == id);
        entity.{EntityName}Name = "Updated Name";
        await CreateRepository(ctx).UpdateAsync(entity);
        ctx.ChangeTracker.Clear();

        var updated = await ctx.{EntityName}.FirstAsync(x => x.Id == id);
        updated.{EntityName}Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_Should_Not_Change_Code()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);
        var id = await CreateRepository(ctx).CreateAsync(BuildEntity("ORIG001"));
        ctx.ChangeTracker.Clear();

        var entity = await ctx.{EntityName}.FirstAsync(x => x.Id == id);
        // Code is not updated (immutable) — just update name
        entity.{EntityName}Name = "Different Name";
        await CreateRepository(ctx).UpdateAsync(entity);
        ctx.ChangeTracker.Clear();

        var updated = await ctx.{EntityName}.FirstAsync(x => x.Id == id);
        updated.{EntityName}Code.Should().Be("ORIG001");
    }

    // --- SOFT DELETE ---

    [Fact]
    public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);
        var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
        ctx.ChangeTracker.Clear();

        var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);
        var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
        ctx.ChangeTracker.Clear();

        await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
        ctx.ChangeTracker.Clear();

        var deleted = await ctx.{EntityName}
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
    }

    [Fact]
    public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        await ClearTableAsync(ctx);

        var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

        result.Should().BeFalse();
    }
}
```

### Query Repository Integration Test Pattern

**File:** `Repositories/{EntityName}/{EntityName}QueryRepositoryTests.cs`

```csharp
[Collection("DatabaseCollection")]
public sealed class {EntityName}QueryRepositoryTests
{
    private readonly DbFixture _fixture;

    public {EntityName}QueryRepositoryTests(DbFixture fixture)
    {
        _fixture = fixture;
    }

    private {EntityName}QueryRepository CreateQueryRepo(Mock<ICompanyLookup> companyLookup = null)
    {
        companyLookup ??= BuildDefaultCompanyLookup();
        var conn = new SqlConnection(_fixture.ConnectionString);
        return new {EntityName}QueryRepository(conn, companyLookup.Object);
    }

    private Mock<ICompanyLookup> BuildDefaultCompanyLookup(
        int companyId = 1,
        string companyName = "Test Company")
    {
        var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
        mock.Setup(c => c.GetAllCompanyAsync())
            .ReturnsAsync(new List<CompanyLookupDto>
            {
                new CompanyLookupDto { CompanyId = companyId, CompanyName = companyName }
            });
        return mock;
    }

    private async Task<int> SeedEntityAsync(string code = "CODE001", string name = "Test")
    {
        await using var ctx = _fixture.CreateFreshDbContext();
        var repo = new {EntityName}CommandRepository(ctx);
        return await repo.CreateAsync(new {Module}.Domain.Entities.{EntityName}
        {
            {EntityName}Code = code,
            {EntityName}Name = name,
            CompanyId = 1,
            IsActive = BaseEntity.Status.Active,
            IsDeleted = BaseEntity.IsDelete.NotDeleted
        });
    }

    private async Task ClearTableAsync()
    {
        await using var conn = new SqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM {Schema}.{EntityName}");
    }

    // --- GET ALL ---

    [Fact]
    public async Task GetAllAsync_Should_Return_Seeded_Record()
    {
        await ClearTableAsync();
        await SeedEntityAsync();

        var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_Should_Populate_CompanyName()
    {
        await ClearTableAsync();
        await SeedEntityAsync();

        var (items, _) = await CreateQueryRepo(BuildDefaultCompanyLookup(1, "Acme Corp"))
            .GetAllAsync(1, 10, null);

        items[0].CompanyName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetAllAsync_Should_Exclude_SoftDeleted()
    {
        await ClearTableAsync();
        var id = await SeedEntityAsync();
        await using var ctx = _fixture.CreateFreshDbContext();
        await new {EntityName}CommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

        var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_Should_Filter_By_SearchTerm()
    {
        await ClearTableAsync();
        await SeedEntityAsync("CODE001", "Alpha Entity");
        await SeedEntityAsync("CODE002", "Beta Entity");

        var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

        items.Should().HaveCount(1);
        items[0].{EntityName}Name.Should().Be("Alpha Entity");
    }

    // --- GET BY ID ---

    [Fact]
    public async Task GetByIdAsync_Should_Return_Correct_Dto()
    {
        await ClearTableAsync();
        var id = await SeedEntityAsync("CODE001", "Test Name");

        var dto = await CreateQueryRepo().GetByIdAsync(id);

        dto.Should().NotBeNull();
        dto.{EntityName}Code.Should().Be("CODE001");
        dto.{EntityName}Name.Should().Be("Test Name");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
    {
        await ClearTableAsync();
        var id = await SeedEntityAsync();
        await using var ctx = _fixture.CreateFreshDbContext();
        await new {EntityName}CommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

        var dto = await CreateQueryRepo().GetByIdAsync(id);

        dto.Should().BeNull();
    }

    // --- ALREADY EXISTS ---

    [Fact]
    public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
    {
        await ClearTableAsync();
        await SeedEntityAsync("CODE001");

        var exists = await CreateQueryRepo().AlreadyExistsAsync("CODE001");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
    {
        await ClearTableAsync();
        var id = await SeedEntityAsync("CODE001");
        await using var ctx = _fixture.CreateFreshDbContext();
        await new {EntityName}CommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

        var exists = await CreateQueryRepo().AlreadyExistsAsync("CODE001");

        exists.Should().BeFalse();
    }

    // --- AUTOCOMPLETE ---

    [Fact]
    public async Task AutocompleteAsync_Should_Exclude_Inactive()
    {
        await ClearTableAsync();
        var id = await SeedEntityAsync("CODE001", "Active Org");
        await using var ctx = _fixture.CreateFreshDbContext();
        var entity = await ctx.{EntityName}.FirstAsync(x => x.Id == id);
        entity.IsActive = BaseEntity.Status.Inactive;
        await ctx.SaveChangesAsync();

        var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

        results.Should().BeEmpty();
    }
}
```

### Cross-Module Dependencies in Integration Tests

**⚠️ NEVER use real cross-module services in integration tests.**
Always mock them:

```csharp
// Mock ICompanyLookup — never call real UserManagement DB
var mockCompanyLookup = new Mock<ICompanyLookup>(MockBehavior.Loose);
mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
    .ReturnsAsync(new List<CompanyLookupDto>
    {
        new CompanyLookupDto { CompanyId = 1, CompanyName = "Test Company" }
    });
```

---

## 🌐 QA Tests (Live-Server / API)

QA tests exercise the **deployed, running API** end-to-end over HTTP (real auth, real DB) — the layer **above** Integration in the test pyramid. They prove each entity's controller contract (CRUD + validation + auth) against a live server.

### Project & location
```
src/tests/{Module}.QATests/
```
**Canonical templates (study these):** `UserManagement.QATests`, `FixedAssetManagement.QATests`, `MaintenanceManagement.QATests`.

### Mandatory conventions
1. **Reference `Shared.QAInfrastructure`** — use the shared `QAServerFixture` (one `testsales` login, run-unique `EntityCode`), `QaHttpClientFactory`, `QAHelper`, `PriorityOrderer` / `TestPriorityAttribute`. Do NOT re-implement these per module.
2. **Serialize collections** — `[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]` in `AssemblyInfo.cs` (the single `testsales` session is single-session-per-user).
3. **One test class per entity** — `Tests/{Entity}/{Entity}QATests.cs`, `[Collection]` + `[TestCaseOrderer(PriorityOrderer)]`, steps ordered with `[TestPriority]`; cross-step ids in `_f.CreatedId` / `static` fields.
4. **Cover per entity:** Create (happy + capture id) · no-auth 401 · empty-body 400 · required/format/maxlength/duplicate negatives · FK validation · GetAll · GetById · AutoComplete · Update (Edit) · **Deactivate = Update with `IsActive=0`** (not a delete) · Delete (soft) · already-deleted.
5. **Smoke tag (see below)** — tag the GetAll happy-path `[Trait("Layer","Smoke")]`.
6. **One `Security/AuthBypassQATests.cs` per module** — `[Trait("Layer","Security")]` (no/empty/garbage/tampered JWT → 401).
7. **`appsettings.QA.json`** copied to output (`<None Update CopyToOutputDirectory>`); BaseUrl/creds overridable by env (`QAServer__BaseUrl`, …).
8. **Run-unique data** — names/codes/sortOrder derived from `EntityCode` (e.g. `QAHelper.RunUniqueInt`) so the suite is **re-runnable without a DB reset**.
9. **FK ids resolved at runtime** — never hard-code id `1`; resolve a real parent via `QAHelper.FirstIdAsync` (create one if none). The QA clone has no guaranteed seed ids.

### 💨 Smoke is a TRAIT, not a project
Smoke = the **GetAll happy-path per entity**, tagged `[Trait("Layer","Smoke")]` inside the QATest class. It proves login → auth → DB → read works. CI runs the slice with `--filter "Layer=Smoke"` as the fail-fast **deploy gate**. There is **no** separate `*.SmokeTests` project.

### Authoring flow (live reconciliation)
Author from the source contract → build **0/0** → run against the live QA clone → reconcile assertions to real behavior (status codes, no-match → 400/404, backend NRE/500s documented with `// BUG (live):` + tolerant assert). First authored draft is expected to need a reconciliation pass — this is normal.

---

## 🧭 Functional Tests (Workflow / Story)

Functional tests verify **multi-step business workflows / acceptance criteria** that cross entities — the **top** of the pyramid. They prove the *story*, not individual endpoints (which QA covers).

### Project & location
```
src/tests/{Module}.FunctionalTests/
```
**Canonical templates:** `UserManagement.FunctionalTests`, `FixedAssetManagement.FunctionalTests`, `MaintenanceManagement.FunctionalTests`.

### Mandatory conventions
1. **Story-Catalogue is the REVIEW GATE** — `Stories/Story-Catalogue.md` lists each story with a user-story statement + an AC table tagged ✅ [implementable] / ⚠️ [verify live] / 🚫 [blocked — needs seeded data]. **Write/approve the catalogue before trusting test code.**
2. **One class per story** — `Workflows/US_{MOD}{NN}_{Slug}_Tests.cs`, carrying `[Trait("Story","US-…")]`, `[Trait("Module","…")]`, `[Trait("Owner","…")]` so CI routes failures and you can run a slice: `dotnet test --filter "Story=US-…"`.
3. **Ordered steps** via `[TestPriority]` + `PriorityOrderer` over one `QAServerFixture`; cross-step ids in `static` fields; teardown leaf-first.
4. **Assert the business meaning, not just 200** — e.g. for a *Deactivate* story: after `IsActive=0`, the record is **gone from `/by-name` autocomplete** but **still in GetAll** (`IsDeleted=0`).
5. **Blocked steps → `[Fact(Skip="needs seeded data: …")]`** with a precise reason (never a silent gap); un-skip during live reconciliation.

### Example mapping (master entity story, e.g. "Create / Edit / Deactivate")
- Create → QA `Create` happy + Functional Step 1
- Edit → QA `Update` happy + Functional Edit step
- Deactivate → QA `Update IsActive=0` + Functional Deactivate step **+ verify excluded from active autocomplete**

---

## 📚 Documentation & References

### Key Documents

- **Technical Docs:** `d:\BSOFT\docs\SalesOrganisation_Technical_Documentation.md`
- **Prompt Template:** `d:\BSOFT\docs\Master_Data_Implementation_Prompt_Template.md`
- **This File:** `d:\BSOFT\CLAUDE.md`

### Reference Implementation

**Module:** SalesManagement
**Entity:** SalesOrganisation
**Path:** `src/Modules/SalesManagement/`

**Key Files to Study:**
- Entity: `Core/SalesManagement.Domain/Entities/SalesOrganisation.cs`
- Command: `Core/SalesManagement.Application/SalesOrganisation/Commands/CreateSalesOrganisation/`
- Query: `Core/SalesManagement.Application/SalesOrganisation/Queries/GetAllSalesOrganisation/`
- Repository: `SalesManagement.Infrastructure/Repositories/SalesOrganisation/`
- Controller: `SalesManagement.Presentation/Controllers/SalesOrganisationController.cs`
- Validator: `SalesManagement.Presentation/Validation/SalesOrganisation/`

---

## 🚀 Quick Start Checklist for New Master Data

> ⚠️ **MANDATORY WORKFLOW:** Document → Approval → Entity/FK Review → Manual Migration → Code → Tests
> AI must STOP and wait for explicit user confirmation at each 🔐 gate before proceeding.

---

### PHASE 1 — Specification Document
- [ ] Gather entity details (name, fields, FKs, validations, schema)
- [ ] Identify all cross-module FK fields and check `src/Shared/Contracts/Interfaces/Lookups/` for each
  - ✅ Lookup exists → note it in spec
  - ❌ Lookup missing → flag it in spec as **"New lookup required: `I{LookupEntity}Lookup`"**
- [ ] Generate specification document covering:
  - Entity fields with types and constraints
  - FK references (same-module vs cross-module)
  - Validation rules (required, max length, format, uniqueness)
  - API endpoints list
  - DB table schema preview
  - **New lookup interfaces that must be created (if any)**

> 🔐 **GATE 1 — STOP:** Share specification document with user.
> ✋ Wait for explicit confirmation: *"Document approved"* or *"Proceed"*
> ❌ DO NOT write any code until approved.

---

### PHASE 2 — Entity & Infrastructure (Code Only — No Migration)
*(Only after GATE 1 approval)*

**Pre-step — Create Any Missing Lookups (Rule #15)**
- [ ] For each cross-module FK whose lookup is missing:
  - [ ] Create `{LookupEntity}LookupDto.cs` in `Contracts/Dtos/Lookups/{SourceModule}/`
  - [ ] Create `I{LookupEntity}Lookup.cs` in `Contracts/Interfaces/Lookups/{SourceModule}/`
  - [ ] Create `{LookupEntity}LookupRepository.cs` in `{SourceModule}.Infrastructure/Repositories/Lookups/{SourceModule}/`
  - [ ] Register: `services.AddScoped<I{LookupEntity}Lookup, {LookupEntity}LookupRepository>()` in source module's `DependencyInjection.cs`
  - [ ] Build `Contracts` + source module Infrastructure projects (0 errors)
  - [ ] *(No cached decorator needed — `AddLookupCaching()` handles it globally)*

- [ ] Create Domain entity extending `BaseEntity`
- [ ] Create EF Core entity configuration (`{EntityName}Configuration.cs`)
- [ ] Add `DbSet<{EntityName}>` + `ApplyConfiguration` in `ApplicationDbContext`
- [ ] Create DTOs (`{EntityName}Dto.cs`, `{EntityName}LookupDto.cs`)
- [ ] Create AutoMapper profile

> 🔐 **GATE 2 — STOP:** Share entity class and FK references with user for review.
> ✋ Ask user to verify: entity fields, FK columns, table schema, indexes.
> ❌ DO NOT run migrations. DO NOT create migration files.
> ✋ User will run migration manually:
> ```bash
> dotnet ef migrations add {EntityName}Master --startup-project ../../../BSOFT.Api
> dotnet ef database update --startup-project ../../../BSOFT.Api
> ```
> ✋ Wait for user confirmation: *"Migration done"* or *"Table created"*

---

### PHASE 3 — Application Layer
*(Only after GATE 2 migration confirmed)*

- [ ] Create Commands (Create, Update, Delete) + Handlers
- [ ] Create Queries (GetAll, GetById, AutoComplete) + Handlers
- [ ] Create Repository interfaces (Command + Query) in `Application/Common/Interfaces/`

---

### PHASE 4 — Infrastructure & Presentation
- [ ] Create Repository implementations (Command EF Core + Query Dapper)
- [ ] Register repositories in `DependencyInjection.cs`
- [ ] Create Controller with standard endpoints (GetAll, GetById, AutoComplete, Create, Update, Delete)
- [ ] Create FluentValidation validators (Create + Update + **Delete**)

---

### PHASE 5 — Tests (full pyramid) & Final Verification
- [ ] **Unit tests** — handlers, validators, entity, controller (`{Module}.UnitTests`)
- [ ] **Integration tests** — command + query repos (`{Module}.IntegrationTests`)
- [ ] **QA tests** — `{Module}.QATests/Tests/{Entity}/{Entity}QATests.cs`: full CRUD + negatives, GetAll tagged `[Trait("Layer","Smoke")]`; module `AuthBypassQATests`
- [ ] **Functional story** — add the story to `{Module}.FunctionalTests/Stories/Story-Catalogue.md` (🔐 review gate), then `Workflows/US_{MOD}{NN}_{Slug}_Tests.cs` (blocked steps `[Fact(Skip)]`)
- [ ] Build solution (0 warnings, 0 errors); all suites build 0/0
- [ ] **Live reconciliation** — run QA + Functional against the QA clone; reconcile to green (document real backend defects with `// BUG (live):` + tolerant asserts)
- [ ] **Wire into pipeline** — add the module's QA + Functional to `deploy-and-test.yml` (non-blocking first; promote the smoke slice to the blocking gate once green)
- [ ] Verify audit logs in MongoDB · soft-delete behavior · validation rules

> **Recommended sequence for a new story (e.g. `US-GL02-01 GL Account Master — Create/Edit/Deactivate`):**
> Story-Catalogue entry (🔐 approve) → Unit + Integration → QA suite (CRUD + Smoke tag) → Functional story → build 0/0 → live-reconcile green → wire pipeline.
> See the **🌐 QA Tests** and **🧭 Functional Tests** sections for the full conventions; `UserManagement` / `FixedAssetManagement` / `MaintenanceManagement` are the reference suites.

---

### PHASE 6 — Technical Documentation & Word Export
*(Only after all code and tests are complete and verified)*

#### Step 1 — HLD Document (PDF output — mandatory)
- [ ] Create HLD content: `docs/{EntityName}_HLD.md`
- [ ] Convert to PDF using **one** of:
  - **pandoc** (if installed):
    ```bash
    pandoc docs/{EntityName}_HLD.md -o docs/{EntityName}_HLD.pdf
    ```
  - **weasyprint** (Python HTML → PDF):
    ```bash
    pip install weasyprint
    python -c "from weasyprint import HTML; HTML(filename='docs/{EntityName}_HLD.html').write_pdf('docs/{EntityName}_HLD.pdf')"
    ```
- [ ] Verify `docs/{EntityName}_HLD.pdf` exists and is readable
- [ ] ✅ PDF is the **only accepted format** for HLD and all non-LLD documentation

**HLD must cover:**
- Module overview and entity purpose
- Entity relationships (same-module and cross-module)
- API endpoints summary table (Method, Route, Purpose)
- Data flow diagram (Controller → Handler → Repository → DB)
- Cross-module dependency list (which lookups are used)
- Soft delete and audit strategy

#### Step 2 — LLD Word Document (.docx output — mandatory)
- [ ] Copy `docs/generate_lld_docx.py` and rename to `docs/generate_{EntityName}_lld_docx.py`
- [ ] Update all section builder functions with the new entity's specific data:
  - Metadata (entity name, module, schema, branch, owner, date)
  - Data model (entity class fields + relationships)
  - Database design (table columns, indexes, FK constraints, DDL)
  - CQRS section (commands, handler flow steps, queries, DTOs)
  - API contracts (endpoint table + endpoint cards)
  - Validation rules table (all Create and Update rules)
  - Business rules (callout boxes per rule)
  - Cross-module references table
  - Test coverage (test file list + counts + checklist)
- [ ] Run the script to generate the Word file:
  ```bash
  cd d:/BSOFT/docs
  python generate_{EntityName}_lld_docx.py
  ```
- [ ] Verify `docs/{EntityName}_LLD.docx` is created with correct content and layout
- [ ] ✅ Word document is the **final shareable deliverable** for team review and Confluence upload

> **Template script location:** `d:\BSOFT\docs\generate_lld_docx.py`
> **Template HTML location:** `d:\BSOFT\docs\SalesItemPriceMaster_LLD.html`
> **Reference Word output:** `d:\BSOFT\docs\SalesItemPriceMaster_LLD.docx`
>
> The Python script uses `python-docx` with XML-level column width control (`_set_tcW` + `_set_tbl_fixed`).
> No Microsoft Word installation is required — only `pip install python-docx`.

---

## 🆕 New Module Registration Checklist

> 📄 See dedicated guide: [`NewModuleRegistrationChecklist.md`](NewModuleRegistrationChecklist.md)

---

## 🛡️ Polly v8 Resilience — New Module Checklist

> **Foundation is already wired.** `AddBsoftResilience(configuration)` is called once in `BSOFT.Api/Program.cs` and `BSOFT.Worker/Program.cs`. Every new module gets the three built-in profiles (Standard / Critical / FastFail) automatically — no `Program.cs` changes needed.

### Step 1 — Decide which resilience scenarios apply

Run through this checklist for every new module:

| Scenario | Applies? | Action Required |
|---|---|---|
| Module registers an `AddHttpClient<T, TImpl>()` | YES if external/internal HTTP | Add `.AddBsoftHttpResilience(profileName)` |
| Module has a Hangfire background job class | YES | Add `[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]` |
| Module has a Dapper SQL write that must be retried | OPTIONAL — high-value writes only | Wrap with `_resilience.ExecuteSqlAsync(profileName, ...)` |
| Module writes to MongoDB (e.g. audit log) | OPTIONAL — fire-and-forget writes are low risk | Wrap with `_resilience.ExecuteMongoTaskAsync(profileName, ...)` |
| MediatR command/query handler | ❌ NEVER | Do NOT add Polly here — risk of duplicate inserts |
| EF Core `SaveChangesAsync` | ❌ NOT needed | `EnableRetryOnFailure(5)` already configured on DbContext |

---

### Step 2 — HTTP Client resilience

**Every `AddHttpClient` registration MUST chain `.AddBsoftHttpResilience`.**

```csharp
// {Module}.Infrastructure/DependencyInjection.cs
using Shared.Infrastructure.Resilience;

// Typed client (preferred)
services.AddHttpClient<IMyExternalService, MyExternalService>()
    .AddBsoftHttpResilience(ResilienceProfileNames.Standard);

// Named client
services.AddHttpClient("MyClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ... })
    .AddBsoftHttpResilience(ResilienceProfileNames.Critical);
```

**Profile selection guide:**

| Profile | When to Use | Retry | Timeout | Circuit Breaker |
|---|---|---|---|---|
| `Standard` | Internal services, reporting, file fetch, SMS/WhatsApp APIs | 3 retries | 30 s | 50% failure ratio |
| `Critical` | GST/e-invoice APIs, auth services, business-critical external APIs | 5 retries | 60 s | 40% failure ratio |
| `FastFail` | SignalR push, UI-facing background calls, where latency > correctness | 1 retry | 5 s | 70% failure ratio |

> ❌ **NEVER call `.AddPollyHandler(...)` (old v7 API)** — the project uses Polly v8 via `Microsoft.Extensions.Http.Resilience`. Only use `.AddBsoftHttpResilience(profileName)`.

> ❌ **NEVER call `services.AddHttpClient()` (bare, unnamed, no resilience)** and then use `CreateClient()` inside a service — always register a named or typed client with `.AddBsoftHttpResilience`.

---

### Step 3 — Hangfire job resilience

Every class decorated with `[Queue(...)]` that performs network or DB I/O MUST also carry `[AutomaticRetry]`:

```csharp
[Queue("my-module-queue")]
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
public class MyModuleJob
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // ...
    }
}
```

> ⚠️ Hangfire's default `[AutomaticRetry(10)]` is too aggressive for minute-recurring jobs — always reduce to `Attempts = 3`. A recurring job that fires every minute picks up missed work on the next run automatically.

---

### Step 4 — Dapper SQL resilience (opt-in, high-value writes only)

Use `ExecuteSqlAsync` for Dapper writes that are **not idempotent by default** and must succeed (e.g. marking outbox messages as published, updating job status). Do NOT use it for every query — it is opt-in for high-value operations.

```csharp
// Inject IResiliencePipelineProvider
private readonly IResiliencePipelineProvider _resilience;

// Wrap a Dapper write
await _resilience.ExecuteSqlAsync(ResilienceProfileNames.Standard,
    ct => _dbConnection.ExecuteAsync(sql, new { Id = id, Now = DateTimeOffset.UtcNow }));

// Wrap a Dapper read that returns a value
var result = await _resilience.ExecuteSqlAsync<MyDto>(ResilienceProfileNames.Standard,
    ct => _dbConnection.QueryFirstOrDefaultAsync<MyDto>(sql, new { Id = id }));
```

---

### Step 5 — MongoDB resilience (opt-in, for critical writes)

Audit log writes are fire-and-forget and do not need wrapping. Wrap MongoDB writes only when the operation is **business-critical** (e.g. storing an outbox message that must be reliably persisted):

```csharp
// ValueTask-returning operation
await _resilience.ExecuteMongoAsync(ResilienceProfileNames.Standard, async ct =>
{
    await _mongoCollection.InsertOneAsync(document, null, ct);
});

// Task-returning operation
await _resilience.ExecuteMongoTaskAsync(ResilienceProfileNames.Standard, async ct =>
{
    await _mongoCollection.ReplaceOneAsync(filter, document, options, ct);
});
```

---

### Step 6 — DI registration (IResiliencePipelineProvider)

`IResiliencePipelineProvider` is registered as a **singleton** by `AddBsoftResilience` — inject it directly into any service or repository that needs SQL or Mongo resilience:

```csharp
// In {Module}.Infrastructure/{EntityName}CommandRepository.cs
private readonly IResiliencePipelineProvider _resilience;

public MyCommandRepository(IDbConnection dbConnection, IResiliencePipelineProvider resilience)
{
    _dbConnection = dbConnection;
    _resilience = resilience;
}
```

> No additional DI registration needed — `IResiliencePipelineProvider` is already registered globally.

---

### Step 7 — NuGet packages (no new packages needed)

The resilience infrastructure lives in `Shared.Infrastructure`. A new module's Infrastructure project only needs a project reference to `Shared.Infrastructure` (which it already has). No new NuGet packages are required in the new module.

```xml
<!-- Already present in every {Module}.Infrastructure.csproj -->
<ProjectReference Include="..\..\..\Shared\Shared.Infrastructure\Shared.Infrastructure.csproj" />
```

---

### Step 8 — Unit tests for resilience

Every new module test project MUST cover:

| What | Test File | What to Assert |
|---|---|---|
| HTTP client has resilience | `Application/{EntityName}/Commands/Create{EntityName}CommandHandlerTests.cs` | No change — handler doesn't know about Polly |
| Hangfire job retries | `Application/Jobs/{JobName}Tests.cs` | `[AutomaticRetry]` attribute present via reflection |
| SQL resilience (if used) | `Repositories/{EntityName}CommandRepositoryTests.cs` | Mock `IResiliencePipelineProvider` with `MockBehavior.Loose` |

**Mock pattern for `IResiliencePipelineProvider` in unit tests:**

```csharp
// MockBehavior.Loose — resilience is infrastructure, not business logic under test
private readonly Mock<IResiliencePipelineProvider> _mockResilience = new(MockBehavior.Loose);

// For ExecuteSqlAsync — make it pass-through so the inner delegate actually runs
_mockResilience
    .Setup(r => r.ExecuteSqlAsync(It.IsAny<string>(), It.IsAny<Func<CancellationToken, Task>>()))
    .Returns<string, Func<CancellationToken, Task>>(
        (_, fn) => fn(CancellationToken.None));
```

> ❌ **NEVER use `MockBehavior.Strict` for `IResiliencePipelineProvider`** — it is an infrastructure concern, not business logic. Strict mocking forces every test to set up Polly calls even when the test is about handler logic.

---

### Quick-reference summary for new modules

```
New module has AddHttpClient?
  YES → chain .AddBsoftHttpResilience(profileName)  [MANDATORY]
  NO  → nothing to do

New module has Hangfire job?
  YES → add [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = Fail)]  [MANDATORY]
  NO  → nothing to do

New module has high-value Dapper writes (outbox, status update)?
  YES → inject IResiliencePipelineProvider, wrap with ExecuteSqlAsync  [RECOMMENDED]
  NO  → nothing to do

New module writes to MongoDB critically?
  YES → wrap with ExecuteMongoTaskAsync  [RECOMMENDED]
  NO  → nothing to do (fire-and-forget audit logs are fine unwrapped)

MediatR handler?
  → NO Polly, ever. Handlers are idempotency-free — Polly here causes duplicate writes.
```

---

## ⚠️ CRITICAL Rules

### 1. **NEVER Physical Delete**
Always use soft delete (`IsDeleted = Deleted`). Master data must be retained for historical records.

### 2. **ALWAYS Filter IsDeleted**
Every query MUST include `WHERE IsDeleted = 0`.

### 3. **NO Cross-Module JOINs**
Use lookup interfaces (`ICompanyLookup`, etc.) instead of SQL JOINs to other modules.

### 4. **Code Fields Are Immutable**
Unique code fields (e.g., `CustomerCode`) cannot be changed after creation — exclude from Update command.

### 5. **Audit Fields Auto-Populated**
NEVER manually set `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate` — `ApplicationDbContext` handles this.

### 6. **Use Existing Domain Events**
Use `AuditLogsDomainEvent` for all audit logging — do NOT create custom event classes.

### 7. **Global ValidationBehavior**
`ValidationBehavior` is registered globally in `Program.cs` — do NOT re-register in modules.

### 8. **Status vs IsActive**
- `IsActive = Active (1)` → Available in dropdowns
- `IsActive = Inactive (0)` → Hidden from dropdowns but retained

### 9. **Autocomplete Filtering**
Autocomplete MUST filter `WHERE IsActive = 1 AND IsDeleted = 0` — only show active, available records.

### 10. **Document First — Code Never Before Approval**
ALWAYS generate the specification document first and wait for explicit user confirmation before writing any code. Do NOT generate code speculatively.

### 11. **NEVER Auto-Migrate — ABSOLUTE RULE**

> ❌ **STRICTLY FORBIDDEN — AI must NEVER do any of the following automatically:**
> - `dotnet ef migrations add ...`
> - `dotnet ef database update ...`
> - Create, modify, or delete any migration file (`.cs` / `.Designer.cs` / snapshot)
> - Call `EnsureCreated()` or `EnsureDeleted()` on the production database
> - Suggest running migration as part of code generation

> ✅ **MIGRATIONS ARE ALWAYS MANUAL — USER RUNS THEM:**
> The user is responsible for reviewing the entity and FK design, then running:
> ```bash
> dotnet ef migrations add {EntityName}Master --startup-project ../../../BSOFT.Api
> dotnet ef database update --startup-project ../../../BSOFT.Api
> ```
> AI must wait for the user to confirm **"Migration done"** or **"Table created"** before writing any further code.

### 12. **Migration Gate — Hard Stop Before Any Code**
The following code must NEVER be written until the user has manually run the migration:
- Repository implementations (Command or Query)
- Controller
- FluentValidation validators
- Command/Query handlers
- Unit tests
- Integration tests

**The gate sequence is:**
1. Specification document reviewed and approved by user ✅
2. Entity class + EF Core configuration written and shared ✅
3. User verifies entity fields, FK columns, table schema, indexes ✅
4. **User runs migration manually** ✅
5. User confirms **"Migration done"** ✅
6. ← Only then does code generation proceed

### 13. **Documentation Format Rule — LLD as Word, Everything Else as PDF**

After ALL code and tests are complete and passing, the following documentation steps are **MANDATORY** in order:

**Step 1 — HLD → PDF (non-negotiable)**
- Create `docs/{EntityName}_HLD.md` (high-level: purpose, relationships, API summary, data flow, cross-module deps, audit/soft-delete strategy)
- Convert to PDF: output must be `docs/{EntityName}_HLD.pdf`
- ✅ PDF is the **only accepted format** for HLD and all non-LLD documents

**Step 2 — LLD → Word .docx (non-negotiable)**
- Copy `docs/generate_lld_docx.py` → `docs/generate_{EntityName}_lld_docx.py`
- Update ALL section builders with the new entity's data
- Run: `python docs/generate_{EntityName}_lld_docx.py`
- Output: `docs/{EntityName}_LLD.docx`
- ✅ Word (.docx) is the **only accepted format** for LLD documentation

> ❌ **Implementation is NOT complete without BOTH deliverables:**
> - `docs/{EntityName}_HLD.pdf` — high-level overview for team review
> - `docs/{EntityName}_LLD.docx` — detailed technical reference for Confluence upload and client handover
>
> ❌ DO NOT produce HLD as Word. ❌ DO NOT produce LLD as PDF. Formats are fixed by this rule.
> NEVER mark an entity as "done" if either file is missing.

### 14. **Create Missing Lookup Interfaces Before Entity Implementation**

When implementing an entity with a cross-module FK, ALWAYS check `src/Shared/Contracts/Interfaces/Lookups/` first.

> **If the required lookup interface does NOT exist — create it immediately before writing any entity code.**

**Mandatory lookup creation sequence:**
1. Create `{LookupEntity}LookupDto.cs` in `Contracts/Dtos/Lookups/{SourceModule}/`
2. Create `I{LookupEntity}Lookup.cs` in `Contracts/Interfaces/Lookups/{SourceModule}/`
3. Create `{LookupEntity}LookupRepository.cs` in `{SourceModule}.Infrastructure/Repositories/Lookups/{SourceModule}/`
4. Register in source module's `DependencyInjection.cs` with `services.AddScoped<I{LookupEntity}Lookup, {LookupEntity}LookupRepository>()`
5. Build Contracts + source module Infrastructure projects (0 errors)
6. Only then proceed with the consuming entity

> ✅ No cached decorator needed — `AddLookupCaching()` in `Program.cs` auto-wraps all `*Lookup` interfaces globally.

> ❌ DO NOT store a FK as a plain `int` without a lookup interface — cross-module FKs must always have a corresponding lookup for DTO population and FK validation.

### 15. **ALWAYS Use IMapper (AutoMapper) in Command Handlers — Never Manual Mapping**

> ❌ **NEVER manually assign command properties to entity properties in handlers:**
> ```csharp
> // ❌ WRONG — manual property mapping
> var entity = new Domain.Entities.BusinessUnit
> {
>     BusinessUnitCode = request.BusinessUnitCode,
>     BusinessUnitName = request.BusinessUnitName,
>     Description = request.Description
> };
> ```

> ✅ **ALWAYS use `IMapper` to map command → entity, with `IsActive`/`IsDeleted` in the Profile:**
> ```csharp
> // ✅ CORRECT — AutoMapper handles everything including IsActive/IsDeleted
> var entity = _mapper.Map<Domain.Entities.BusinessUnit>(request);
> // No manual IsActive/IsDeleted assignment — handled in BusinessUnitProfile via ForMember
> ```
>
> **In `BusinessUnitProfile.cs`:**
> ```csharp
> using static BusinessUnit.Domain.Common.BaseEntity;
>
> CreateMap<CreateBusinessUnitCommand, Domain.Entities.BusinessUnit>()
>     .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
>     .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
>
> CreateMap<UpdateBusinessUnitCommand, Domain.Entities.BusinessUnit>()
>     .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src =>
>         src.IsActive == 1 ? Status.Active : Status.Inactive));
> ```

**Mandatory handler dependencies (all 4 required):**
1. `I{EntityName}CommandRepository` — write operations
2. `I{EntityName}QueryRepository` — validation/existence checks
3. `IMediator` — audit event publishing
4. `IMapper` — command-to-entity mapping

**Why?**
- **Consistency:** All handlers follow the same pattern — reduces cognitive load during code review
- **Maintainability:** When entity fields change, only the AutoMapper Profile needs updating — not every handler
- **Scalability:** Adding new fields requires zero handler changes
- **Testability:** `IMapper` can be mocked in unit tests for isolated testing

> ⚠️ Every Create/Update command MUST have a corresponding `AutoMapper.Profile` in `Common/Mappings/{EntityName}Profile.cs` that:
> - Maps `CreateMap<Create{EntityName}Command, {EntityName}>()` with **`ForMember` for `IsActive = Status.Active` and `IsDeleted = IsDelete.NotDeleted`**
> - Maps `CreateMap<Update{EntityName}Command, {EntityName}>()` with **`ForMember` for `IsActive` from `src.IsActive == 1 ? Status.Active : Status.Inactive`**
> - Uses `using static {Module}.Domain.Common.BaseEntity;` for clean enum references

### 16. **NO [Authorize] on Controllers — Simple HTTP Verb Routes Only**

> ❌ **NEVER add `[Authorize]` attribute to controllers:**
> ```csharp
> // ❌ WRONG
> [Authorize]
> [Route("api/[controller]")]
> public class BusinessUnitController : ApiControllerBase
> ```

> ✅ **Controllers use `[Route("api/[controller]")]` only — no `[Authorize]`:**
> ```csharp
> // ✅ CORRECT
> [Route("api/[controller]")]
> public class BusinessUnitController : ApiControllerBase
> ```

Authentication is handled globally by `TokenValidationMiddleware` in the request pipeline — individual controllers do NOT need `[Authorize]`.

> ❌ **NEVER add route suffixes to HTTP verb attributes:**
> ```csharp
> // ❌ WRONG — route suffixes
> [HttpPost("create")]
> [HttpPut("update")]
> [HttpDelete("delete/{id}")]
> [HttpGet("autocomplete")]
> ```

> ✅ **Use simple HTTP verb attributes only:**
> ```csharp
> // ✅ CORRECT — clean verbs
> [HttpGet]                // GET api/{Entity}
> [HttpGet("{id}")]        // GET api/{Entity}/{id}
> [HttpGet("by-name")]     // GET api/{Entity}/by-name
> [HttpPost]               // POST api/{Entity}
> [HttpPut]                // PUT api/{Entity}
> [HttpDelete]             // DELETE api/{Entity}
> ```

### 17. **NO Validation in Controllers or Handlers — ALL Validation in FluentValidation Validators Only**

> ❌ **NEVER add `ModelState.IsValid` checks in controllers:**
> ```csharp
> // ❌ WRONG — controller-level validation
> [HttpPost]
> public async Task<IActionResult> CreateBusinessUnit([FromBody] CreateBusinessUnitCommand command)
> {
>     if (!ModelState.IsValid)
>         return BadRequest(ModelState);
>     var result = await Mediator.Send(command);
>     return Ok(result);
> }
> ```

> ✅ **Controllers are thin pass-through layers — no validation:**
> ```csharp
> // ✅ CORRECT — validation handled by FluentValidation pipeline
> [HttpPost]
> public async Task<IActionResult> CreateBusinessUnit([FromBody] CreateBusinessUnitCommand command)
> {
>     var result = await Mediator.Send(command);
>     return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
> }
> ```

> ❌ **NEVER add inline validation in command handlers:**
> ```csharp
> // ❌ WRONG — handler-level validation
> public async Task<ApiResponseDTO<int>> Handle(UpdateBusinessUnitCommand request, CancellationToken ct)
> {
>     var existing = await _queryRepository.GetByIdAsync(request.Id);
>     if (existing == null)
>         throw new ExceptionRules("Business Unit not found.");
>     // ...
> }
> ```

> ✅ **Handlers assume validation has passed — only do: map → persist → audit → return:**
> ```csharp
> // ✅ CORRECT — handler trusts FluentValidation
> public async Task<ApiResponseDTO<int>> Handle(UpdateBusinessUnitCommand request, CancellationToken ct)
> {
>     var entity = _mapper.Map<Domain.Entities.BusinessUnit>(request);
>     entity.IsActive = request.IsActive == 1
>         ? Domain.Common.BaseEntity.Status.Active
>         : Domain.Common.BaseEntity.Status.Inactive;
>     var result = await _commandRepository.UpdateAsync(entity);
>     // audit + return
> }
> ```

**Why?**
- `ValidationBehavior<TRequest, TResponse>` is registered globally in `Program.cs` — it runs ALL FluentValidation validators **before** the handler executes
- `GlobalExceptionMiddleware` catches `ValidationException` → returns 400 Bad Request automatically
- Entity existence (`NotFoundAsync`), FK existence, uniqueness, and all business rules belong in `Presentation/Validation/{EntityName}/` validators
- Validators can inject cross-module lookup interfaces (e.g., `ICurrencyLookup`, `ICompanyLookup`) for FK validation
- This keeps handlers clean (4 standard dependencies only: `ICommandRepository`, `IQueryRepository`, `IMediator`, `IMapper`)

**Validation ownership summary:**

| Check Type | Where It Belongs | Example |
|---|---|---|
| Required fields | Validator | `RuleFor(x => x.Name).NotEmpty()` |
| Format validation | Validator | `RuleFor(x => x.Code).Matches("^[A-Za-z0-9]+$")` |
| Max length | Validator | `RuleFor(x => x.Name).MaximumLength(100)` |
| Uniqueness | Validator | `MustAsync(... AlreadyExistsAsync ...)` |
| Entity existence | Validator | `MustAsync(... NotFoundAsync ...)` |
| FK existence (same-module) | Validator | `MustAsync(... CompanyExistsAsync ...)` |
| FK existence (cross-module) | Validator | Inject `ICurrencyLookup`, use `GetByIdsAsync` |
| IsActive range | Validator | `InclusiveBetween(0, 1)` |
| Composite key uniqueness | Validator | `MustAsync(... CompositeKeyExistsAsync ...)` |
| `ModelState.IsValid` | ❌ NOWHERE | Removed — FluentValidation handles it |
| `if (existing == null) throw` | ❌ NOWHERE in handlers | Moved to validator `NotFoundAsync` |

### 18. **ALWAYS Use Shared Validation Infrastructure — Never Hardcode Validation Rules**

> ❌ **NEVER hardcode validation rules, regex patterns, or max lengths directly in validators:**
> ```csharp
> // ❌ WRONG — hardcoded rules, magic numbers, and inline regex
> RuleFor(x => x.Code)
>     .NotEmpty().WithMessage("Code is required.")
>     .Matches("^[A-Za-z0-9]+$").WithMessage("Code must be alphanumeric.")
>     .MaximumLength(20).WithMessage("Code cannot exceed 20 characters.");
> ```
>
> ❌ **NEVER hardcode the regex pattern in the `Alphanumeric` case — even with a comment:**
> ```csharp
> // ❌ WRONG — hardcoded regex; also the pattern with a space was a bug in validation-rules.json (now fixed)
> case "Alphanumeric":
>     // SalesGroupName allows alphanumeric + spaces (per business rule)
>     RuleFor(x => x.SalesGroupName)
>         .Matches(@"^[A-Za-z0-9 ]+$")   // ❌ WRONG — spaces are NOT alphanumeric; correct pattern is ^[A-Za-z0-9]+$
>         .WithMessage($"{nameof(Command.SalesGroupName)} {rule.Error}")
>         .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName));
>     break;
> ```
>
> ✅ **ALWAYS use `rule.Pattern` from the shared validation JSON — no exceptions:**
> ```csharp
> // ✅ CORRECT — pattern comes from validation-rules.json via ValidationRuleLoader
> case "Alphanumeric":
>     RuleFor(x => x.SalesGroupName)
>         .Matches(rule.Pattern)          // ✅ sourced from JSON, never hardcoded
>         .WithMessage($"{nameof(Command.SalesGroupName)} {rule.Error}")
>         .When(x => !string.IsNullOrWhiteSpace(x.SalesGroupName));
>     break;
> ```

> ✅ **ALWAYS use `ValidationRuleLoader` + `MaxLengthProvider` + `switch/case` pattern:**
> ```csharp
> // ✅ CORRECT — shared validation rules from JSON + EF Core metadata
> var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.{EntityName}>("Code") ?? 20;
> _validationRules = ValidationRuleLoader.LoadValidationRules();
>
> foreach (var rule in _validationRules)
> {
>     switch (rule.Rule)
>     {
>         case "NotEmpty":
>             RuleFor(x => x.Code)
>                 .NotNull().WithMessage($"{nameof(Command.Code)} {rule.Error}")
>                 .NotEmpty().WithMessage($"{nameof(Command.Code)} {rule.Error}");
>             break;
>         case "Alphanumeric":
>             RuleFor(x => x.Code)
>                 .Matches(rule.Pattern)
>                 .WithMessage($"{nameof(Command.Code)} {rule.Error}")
>                 .When(x => !string.IsNullOrWhiteSpace(x.Code));
>             break;
>         case "MaxLength":
>             RuleFor(x => x.Code)
>                 .MaximumLength(maxLengthCode)
>                 .WithMessage($"{nameof(Command.Code)} {rule.Error} {maxLengthCode} characters.");
>             break;
>     }
> }
> ```

**Mandatory validator dependencies:**
1. **`MaxLengthProvider`** — always the first constructor parameter; extracts varchar(n) from EF Core entity metadata
2. **`I{EntityName}QueryRepository`** — for existence/uniqueness async checks
3. **Cross-module lookups** (if needed) — e.g., `ICurrencyLookup` for FK validation in `FKColumnDelete` case

**Mandatory validator structure:**
1. Extract max lengths via `maxLengthProvider.GetMaxLength<Entity>("PropertyName") ?? fallback`
2. Load rules via `ValidationRuleLoader.LoadValidationRules()`
3. Null/empty check on loaded rules → `_validationRules.Count == 0` (not `!Any()`) → throw `InvalidOperationException`
4. `foreach (var rule in _validationRules)` with `switch (rule.Rule)`
5. Error messages: `$"{nameof(Command.Property)} {rule.Error}"`
6. **`Alphanumeric` case MUST use `.Matches(rule.Pattern)` — NEVER hardcode the regex.** The correct pattern in `validation-rules.json` is `^[A-Za-z0-9]+$` (letters and digits, **no spaces**). A previous version of the JSON had `^[A-Za-z0-9 ]+$` (with a space) — this was a bug that caused space-containing codes to pass validation. It has been fixed.

**Key files:**
- `src/Shared/Shared.Validation/Common/ValidationRuleLoader.cs` — rule loader
- `src/Shared/Shared.Validation/Common/validation-rules.json` — shared JSON rules (embedded resource)
- `{Module}.Presentation/Validation/Common/MaxLengthProvider.cs` — per-module EF metadata provider

### 19. **NO Unused Namespaces, Injections, or Variables**

> All projects use `<ImplicitUsings>enable</ImplicitUsings>` (.NET 8), which auto-imports:
> `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, `System.Threading.Tasks`

> ❌ **NEVER include implicit usings explicitly:**
> ```csharp
> // ❌ WRONG — these are already implicit in .NET 8
> using System;
> using System.Collections.Generic;
> using System.Linq;
> using System.Threading.Tasks;
> ```

> ✅ **Only include non-implicit usings that are actually referenced:**
> ```csharp
> // ✅ CORRECT — only non-implicit, actually-used namespaces
> using FluentValidation;
> using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
> using SalesManagement.Presentation.Validation.Common;
> using Shared.Validation.Common;
> ```

**Rules:**
1. **No redundant `using` directives** — never add `using System;`, `using System.Collections.Generic;`, `using System.Linq;`, `using System.Threading;`, or `using System.Threading.Tasks;` since they are implicit
2. **No unused constructor parameters** — every injected dependency must be used in the class body
3. **No unused private fields** — every `private readonly` field must be referenced in at least one method
4. **No unused local variables** — every declared variable must be read
5. **No unused DI registrations** — every `services.AddScoped<>()` in `DependencyInjection.cs` must have a consumer

**Before creating any new file**, ensure only non-implicit, actually-referenced namespaces are included in the `using` block.

---

### 20. **Lookup Repository Implementations MUST Be `internal sealed class`**

> ❌ **NEVER use `public class` for lookup repository implementations:**
> ```csharp
> // ❌ WRONG — public exposes internal infrastructure outside the module
> public class CityLookupRepository : ICityLookup
> ```

> ✅ **ALWAYS use `internal sealed class` for lookup repositories:**
> ```csharp
> // ✅ CORRECT — internal keeps implementation hidden, sealed prevents inheritance
> internal sealed class CityLookupRepository : ICityLookup
> ```

**Why?**
- **Encapsulation:** Lookup repositories are infrastructure implementation details — they should not be visible outside the module assembly
- **DI resolves via interface:** Consumers only depend on `ICityLookup` (from `Contracts`), never on the concrete class
- **`sealed` prevents inheritance:** Repository implementations are leaf classes — there is no reason to extend them
- **Consistency:** All lookup repositories across all modules must follow this pattern

**Applies to:** All files in `{Module}.Infrastructure/Repositories/Lookups/` directories.

### 21. **ALL Controllers MUST Extend `ApiControllerBase` — Never `ControllerBase` Directly**

> ❌ **NEVER extend `ControllerBase` directly in any controller:**
> ```csharp
> // ❌ WRONG — bypasses shared base class
> [ApiController]
> [Route("api/[controller]")]
> public class BudgetGroupController : ControllerBase
> ```

> ✅ **ALWAYS extend `ApiControllerBase` and omit `[ApiController]`:**
> ```csharp
> // ✅ CORRECT — inherits [ApiController] + [Route] + Mediator from base
> [Route("api/[controller]")]
> public class BudgetGroupController : ApiControllerBase
> {
>     public BudgetGroupController(IMediator mediator) : base(mediator) { }
> }
> ```

**Rules:**
1. **Every module has an `ApiControllerBase`** in `{Module}.Presentation/Controllers/ApiControllerBase.cs` with `[ApiController]` and a `Mediator` property
2. **`[ApiController]` must ONLY appear on `ApiControllerBase`** — never on individual controllers (it's inherited)
3. **All controllers must call `base(mediator)`** in their constructor
4. **Controllers can keep a private `_mediator` field** if they use it internally, but must still call `base(mediator)`

**Why?**
- **Single source of truth:** `[ApiController]` behavior (model binding, validation, 400 responses) is configured once in the base class
- **Consistency:** All controllers follow the same inheritance chain
- **Maintainability:** Changes to base controller behavior only need to happen in one place

### 22. **Nullable Reference Types (`<Nullable>enable</Nullable>`) — Correct Patterns**

All module `.csproj` files use `<Nullable>enable</Nullable>`. The following patterns MUST be followed to keep builds at 0 warnings.

#### NEVER Use `= null!;` — Always Use `?` Nullable Type Declarations

> ❌ **NEVER initialize a property with `= null!;` in entities, DTOs, commands, or any class:**
> ```csharp
> // ❌ WRONG — null-forgiving initializer hides a nullable mismatch; violates nullable semantics
> public string SalesGroupName { get; set; } = null!;
> public string? CompanyName { get; set; } = null!;
> public string Description { get; set; } = null!;
> ```

> ✅ **ALWAYS declare the type as nullable with `?` and omit any initializer:**
> ```csharp
> // ✅ CORRECT — type declaration is honest; compiler tracks nullability properly
> public string? SalesGroupName { get; set; }
> public string? CompanyName { get; set; }
> public string? Description { get; set; }
> ```

**Why?**
- `= null!;` tells the compiler "this is null, but trust me it won't be" — this is a lie that suppresses useful warnings
- `string?` honestly declares that the value may be null, so any consumer must handle it safely
- Every caller that uses the property without a null-check will get a proper CS8602/CS8604 warning

**The only valid uses of `!` (null-forgiving operator):**
| Situation | Example |
|---|---|
| Validator `MustAsync` lambda guarded by `.When()` | `.MustAsync(async (mobile, ct) => !await _repo.ExistsAsync(mobile!))` |
| Accessing nullable after explicit null-check | `if (result == null) return; var id = result!.Id;` |
| Null-forgiving on `int?` `.Value` across LINQ boundary | `item.NullableId!.Value` |

> ⚠️ **After changing `= null!;` to `?`, any validator `MustAsync` lambda that passes the property to a non-nullable parameter will emit CS8604. Fix with `!` on the argument (safe because `.When()` already guards against null).**
> ```csharp
> // Before fix_null_bang.ps1: mobile was string, passed to ExistsAsync(string)
> // After: mobile is string?, requires ! in MustAsync lambda
> .MustAsync(async (mobile, ct) => !await _repo.ExistsAsync(mobile!))  // ✅ Safe — .When() guards
> ```

#### DTO Properties Populated from Cross-Module Lookups

> ❌ **NEVER use `= null!` for properties populated from lookups — they can legitimately be null if the FK record is not found:**
> ```csharp
> // ❌ WRONG — lookup may not find a match; null! lies to the compiler
> public string CompanyName { get; set; } = null!;
> public string CurrencyCode { get; set; } = null!;
> public string ItemName { get; set; } = null!;
> ```

> ✅ **ALWAYS use `string?` for DTO properties populated from cross-module lookups:**
> ```csharp
> // ✅ CORRECT — nullable, honestly reflects that the lookup may return null
> public string? CompanyName { get; set; }
> public string? CurrencyCode { get; set; }
> public string? ItemName { get; set; }
> ```

**Rule:** Any DTO property that is set by a line like `dto.X = lookup?.Property` or `dict.TryGetValue(..., out var v) ? v : null` MUST be `string?`, not `string`.

---

#### `GetByIdAsync` Must Return `Task<T?>` — Not `Task<T>`

Dapper's `QueryFirstOrDefaultAsync<T>` always returns `T?`. Interface and implementation must match:

> ❌ **WRONG — return type mismatch with Dapper:**
> ```csharp
> Task<SalesOrganisationDto> GetByIdAsync(int id);   // interface
> public async Task<SalesOrganisationDto> GetByIdAsync(int id) { ... }  // impl
> ```

> ✅ **CORRECT — nullable return type matches Dapper:**
> ```csharp
> Task<SalesOrganisationDto?> GetByIdAsync(int id);   // interface
> public async Task<SalesOrganisationDto?> GetByIdAsync(int id) { ... }  // impl
> ```

All `GetByIdAsync` query handlers that receive `T?` from the repository **MUST** null-check before use:
```csharp
var data = await _queryRepository.GetByIdAsync(request.Id);
if (data == null)
    throw new EntityNotFoundException(nameof(Entity), request.Id);
return new ApiResponseDTO<EntityDto> { IsSuccess = true, Data = data };
```

---

#### `GetAllAsync` — `searchTerm` Parameter Must Be `string?`

Query classes declare `public string? SearchTerm { get; set; }`, so repository signatures must accept `string?`:

> ✅ **CORRECT:**
> ```csharp
> Task<(List<EntityDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
> ```

---

#### Nullable `int?` `.Value` Access in LINQ Chains

C# flow analysis does NOT carry `.HasValue` guards across LINQ method chain boundaries. Use the null-forgiving operator:

> ❌ **WRONG — CS8629 even though `.Where(x => x.NullableId.HasValue)` precedes this:**
> ```csharp
> foreach (var item in list.Where(x => x.ProductCategoryId.HasValue))
> {
>     item.Name = dict.TryGetValue(item.ProductCategoryId.Value, ...) ? v : null;  // CS8629
> }
> ```

> ✅ **CORRECT — null-forgiving operator suppresses the false-positive:**
> ```csharp
> foreach (var item in list.Where(x => x.ProductCategoryId.HasValue))
> {
>     item.Name = dict.TryGetValue(item.ProductCategoryId!.Value, ...) ? v : null;
> }
> ```

---

#### `GetConnectionString()` Returns `string?` — Must Null-Coalesce Before Chaining

> ❌ **WRONG — CS8602:**
> ```csharp
> var cs = configuration.GetConnectionString("DefaultConnection").Replace("{SERVER}", "...");
> ```

> ✅ **CORRECT:**
> ```csharp
> var cs = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
>     .Replace("{SERVER}", "...");
> ```

---

#### `string?` Parameters Passed to APIs Expecting `string`

| Situation | Fix |
|---|---|
| `string? username` → `new Claim(string type, string value)` | `username ?? string.Empty` |
| `string? EventType` → `Type.GetType(string typeName)` | `message.EventType!` (safe inside a known-non-null block) |
| `string? EventData` → `JsonSerializer.Deserialize(string json, ...)` | `message.EventData!` |

---

### 23. **ALWAYS Include Controller Tests and Domain Entity Tests — Not Just Handlers and Validators**

> ❌ **NEVER create a test suite that only covers handlers and validators:**
> ```
> {ModuleName}.UnitTests/
> ├── Application/          ← handler tests only
> ├── Validators/            ← validator tests only
> └── TestData/              ← builders only
> ```

> ✅ **ALWAYS include Controller tests AND Domain Entity tests alongside handler/validator tests:**
> ```
> {ModuleName}.UnitTests/
> ├── Application/           ← handler tests
> ├── Controllers/           ← controller tests (one per controller)
> ├── Domain/                ← entity tests
> ├── Validators/            ← validator tests
> └── TestData/              ← builders
> ```

#### Controller Unit Tests — `Controllers/{EntityName}ControllerTests.cs`

Every controller MUST have a dedicated test file that verifies:
1. Each action method returns `OkObjectResult` (200 status)
2. Each action method calls `Mediator.Send()` exactly once with the correct command/query type
3. Delete action returns Ok for both success and failure cases

**Pattern:**
```csharp
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Presentation.Controllers;
using {Module}.Application.{EntityName}.Commands.Create{EntityName};
using {Module}.Application.{EntityName}.Commands.Update{EntityName};
using {Module}.Application.{EntityName}.Commands.Delete{EntityName};
using {Module}.Application.{EntityName}.Queries.GetAll{EntityName};
using {Module}.Application.{EntityName}.Queries.Get{EntityName}ById;
using {Module}.Application.{EntityName}.Queries.Get{EntityName}AutoComplete;
using {Module}.Application.{EntityName}.Dto;

namespace {Module}.UnitTests.Controllers
{
    public sealed class {EntityName}ControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private {EntityName}Controller CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAll{EntityName}Query>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<{EntityName}Dto>>
                {
                    IsSuccess = true,
                    Data = new List<{EntityName}Dto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAll{EntityName}Async(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            // Same setup as above...
            await CreateSut().GetAll{EntityName}Async(1, 10);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAll{EntityName}Query>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult() { /* Mock Send → return ApiResponseDTO */ }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult() { /* Mock Send → return IReadOnlyList */ }

        [Fact]
        public async Task Create_ReturnsOkResult() { /* Mock Send → return ApiResponseDTO<int> */ }

        [Fact]
        public async Task Update_ReturnsOkResult() { /* Mock Send → return ApiResponseDTO<int> */ }

        [Fact]
        public async Task Delete_ReturnsOkResult() { /* Mock Send → return bool */ }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once() { /* Verify Times.Once */ }
    }
}
```

**Standard test count per controller:** 7-8 tests (GetAll Ok + GetAll Mediator, GetById Ok, AutoComplete Ok, Create Ok, Update Ok, Delete Ok + Delete Mediator)

**Key rules:**
- Mock `IMediator` (which extends `ISender`) — controllers accept `IMediator` in constructor
- Controller tests verify the **pass-through layer** works correctly (routing → MediatR → response)
- Use `It.IsAny<TQuery>()` for matching — do not assert query property values (that's the handler test's job)
- Test project must have `<FrameworkReference Include="Microsoft.AspNetCore.App" />` in `.csproj` for MVC types

#### Domain Entity Tests — One File Per Entity (`Domain/{EntityName}EntityTests.cs`)

> ❌ **NEVER put all entity tests in a single `EntityTests.cs` file:**
> ```
> Domain/
> └── EntityTests.cs    ← ❌ WRONG — all entities crammed into one file
> ```

> ✅ **ALWAYS create one dedicated test file per entity:**
> ```
> Domain/
> ├── SalesOrganisationEntityTests.cs    ← ✅ one file per entity
> ├── SalesChannelEntityTests.cs
> ├── BusinessUnitEntityTests.cs
> ├── SalesSegmentEntityTests.cs
> ├── SalesGroupEntityTests.cs
> ├── SalesOfficeEntityTests.cs
> ├── SalesItemPriceMasterEntityTests.cs
> ├── MiscTypeMasterEntityTests.cs
> ├── MiscMasterEntityTests.cs
> └── BaseEntityAuditFieldsTests.cs     ← shared audit-field tests (uses any concrete entity)
> ```

**File naming:** `{EntityName}EntityTests.cs` — one class, one entity, one file.

For EACH entity file, test:

1. **Default `IsActive` = `Status.Active`** — new entity starts as Active
2. **Default `IsDeleted` = `IsDelete.NotDeleted`** — new entity starts as NotDeleted
3. **Inherits from `BaseEntity`** — `typeof(BaseEntity).IsAssignableFrom(typeof(Entity))` is true
4. **Properties are assignable** — all custom properties can be set and read back
5. **Nullable properties accept null** — `int?`, `DateTime?` properties accept null values
6. **Navigation properties are assignable** — collection and single-entity navigation props can be set

**Pattern (`{EntityName}EntityTests.cs`):**
```csharp
using {Module}.Domain.Common;
using {Module}.Domain.Entities;
using static {Module}.Domain.Common.BaseEntity;

namespace {Module}.UnitTests.Domain
{
    public class {EntityName}EntityTests
    {
        [Fact]
        public void {EntityName}_DefaultIsActive_ShouldBeActive()
        {
            var entity = new {EntityName}();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void {EntityName}_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new {EntityName}();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void {EntityName}_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof({EntityName})).Should().BeTrue();
        }

        [Fact]
        public void {EntityName}_Properties_ShouldBeAssignable()
        {
            var entity = new {EntityName}
            {
                Id = 1,
                {Property1} = "value",
                {Property2} = 10
            };
            entity.Id.Should().Be(1);
            entity.{Property1}.Should().Be("value");
            entity.{Property2}.Should().Be(10);
        }

        // Add nullable and navigation tests as applicable for this entity
    }
}
```

**Shared audit-field tests (`BaseEntityAuditFieldsTests.cs`):**
```csharp
using {Module}.Domain.Common;
using {Module}.Domain.Entities;

namespace {Module}.UnitTests.Domain
{
    public class BaseEntityAuditFieldsTests
    {
        [Fact]
        public void BaseEntity_AuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new {AnyConcreteEntity}
            {
                CreatedBy = 1, CreatedDate = now, CreatedByName = "admin", CreatedIP = "127.0.0.1",
                ModifiedBy = 2, ModifiedDate = now.AddHours(1), ModifiedByName = "editor", ModifiedIP = "127.0.0.2"
            };
            entity.CreatedBy.Should().Be(1);
            entity.ModifiedBy.Should().Be(2);
        }

        [Fact]
        public void BaseEntity_NullableAuditFields_ShouldAcceptNull()
        {
            var entity = new {AnyConcreteEntity}
            {
                CreatedDate = null, CreatedByName = null, CreatedIP = null,
                ModifiedBy = null, ModifiedDate = null, ModifiedByName = null, ModifiedIP = null
            };
            entity.CreatedDate.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
        }
    }
}
```

**Standard test count:** ~4-7 tests per entity file (defaults + inheritance + properties + nullable/navigation)

> ⚠️ **Entities that do NOT extend `BaseEntity` are excluded from entity tests:**
> - `AuditLogs` extends `AuditLogBase` (MongoDB) — not a `BaseEntity`
> - `JwtSettings` is a configuration class — not a `BaseEntity`
> - Only test entities that are EF Core-managed and extend `BaseEntity`

**Why one file per entity:**
- **Single Responsibility:** Each file owns exactly one entity — easy to find, easy to extend
- **Discoverability:** File name matches entity name — `SalesGroupEntityTests.cs` is immediately obvious
- **Scalability:** Adding a new entity adds one new file, never modifies an existing one
- **Focused failure messages:** A failing test run points directly to the affected entity file

**Why controller and entity tests are mandatory:**
- **Controllers** are the HTTP entry point — testing them verifies that request routing, MediatR dispatch, and response formatting all work correctly
- **Entity tests** verify that default enum values (`IsActive`, `IsDeleted`) are correct and that property getters/setters work — this catches regressions when entity classes are modified
- Without these tests, a broken controller or entity default value could pass the build but fail at runtime

---

### 24. **NEVER Use `#nullable disable` — All C# Files Must Be Nullable-Warning-Free**

All module `.csproj` files use `<Nullable>enable</Nullable>`. **No source file or test file may suppress this with `#nullable disable`.**

> ❌ **NEVER add `#nullable disable` to any `.cs` file — source or test:**
> ```csharp
> // ❌ WRONG — suppresses compiler null-safety checks
> #nullable disable
> namespace SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
> ```

> ✅ **ALWAYS write nullable-correct code from the start — no suppression directives:**
> ```csharp
> // ✅ CORRECT — no suppression; all nullable warnings resolved explicitly
> namespace SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
> ```

#### Source Files — Common Patterns

| Situation | Fix |
|---|---|
| `GetByIdAsync` returns `T` but Dapper returns `T?` | Change interface + handler to `Task<T?>` |
| DTO property set from cross-module lookup | Declare as `string?` (not `string`) |
| `data.Count` on possibly-null list | Use `(data?.Count ?? 0)` |
| `GetConnectionString()` chained call | Null-coalesce: `(config.GetConnectionString("...") ?? string.Empty).Replace(...)` |
| Nullable `int?` `.Value` across LINQ boundary | Use null-forgiving: `item.NullableId!.Value` |

#### Test Files — Common Patterns

| Situation | Fix |
|---|---|
| Callback capture variable: `Entity capturedEntity = null` | `Entity? capturedEntity = null` |
| Accessing captured variable after callback | `capturedEntity!.Property` |
| `ReturnsAsync((XDto)null)` for nullable return | `ReturnsAsync((XDto?)null)` |
| `Returns<object>(o => (o as XDto))` | `Returns<object>(o => (o as XDto)!)` |
| `result.Property` after `result.Should().NotBeNull()` | `result!.Property` |
| `[InlineData(null)]` on `string` parameter | Change parameter to `string?` |
| Passing `null` to `string` parameter in mock Setup/Verify | Use `null!` |

#### Exception: EF Core Migration Files

EF Core auto-generates migration files with lowercase class names that trigger CS8981. Migration `.cs` and `.Designer.cs` files MAY retain `#nullable disable` since they are tool-generated. All hand-written source and test files must be free of it.

**Build target:** `0 Warning(s), 0 Error(s)` — never suppress warnings with pragmas or `#nullable disable`.

---

### 25. **Master Entity Delete & Inactivate Validation When Dependents Exist**

#### When This Rule Applies

When implementing **Entity B** that has a FK referencing **Master A**, you MUST go back and update Master A's validation to check the new dependent table. The rule fires **at the time of implementing the dependent entity**, not when the master is first created.

> ✅ Same-module dependents → **direct Dapper SQL** (`EXISTS` query)
> ✅ Cross-module dependents → **validation interface** (consistent with lookup interface pattern — do NOT use direct SQL to another module's table)

---

#### Step-by-Step: What to Update on Master A

**Step 1 — Add methods to `I{MasterEntity}QueryRepository` interface**

```csharp
Task<bool> SoftDeleteValidationAsync(int id);       // for Delete validator
Task<bool> Is{MasterEntity}LinkedAsync(int id);     // for Inactivate (Update handler)
```

**Step 2a — Implement in `{MasterEntity}QueryRepository` (same-module dependent)**

```csharp
// Same-module: direct Dapper SQL — dependent table is in the same schema
public async Task<bool> SoftDeleteValidationAsync(int id)
{
    const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1 FROM {Schema}.{DependentTable}
            WHERE {MasterEntity}Id = @Id AND IsDeleted = 0
        ) THEN 1 ELSE 0 END";

    return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
}

public async Task<bool> Is{MasterEntity}LinkedAsync(int id)
{
    const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1 FROM {Schema}.{DependentTable}
            WHERE {MasterEntity}Id = @Id AND IsDeleted = 0 AND IsActive = 1
        ) THEN 1 ELSE 0 END";

    return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
}
```

**Step 2b — Cross-module dependent: create a Validation Interface**

When the dependent table belongs to a different module, follow the same pattern as lookup interfaces:

**File:** `src/Shared/Contracts/Interfaces/Validations/{DependentModule}/I{DependentEntity}Validation.cs`
```csharp
namespace Contracts.Interfaces.Validations.{DependentModule};

public interface I{DependentEntity}Validation
{
    Task<bool> HasLinked{MasterEntity}Async(int masterEntityId);
}
```

**File:** `src/Modules/{DependentModule}/{DependentModule}.Infrastructure/Repositories/Validations/{DependentEntity}ValidationRepository.cs`
```csharp
internal sealed class {DependentEntity}ValidationRepository : I{DependentEntity}Validation
{
    private readonly IDbConnection _dbConnection;

    public {DependentEntity}ValidationRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> HasLinked{MasterEntity}Async(int masterEntityId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM {DependentSchema}.{DependentTable}
                WHERE {MasterEntity}Id = @Id AND IsDeleted = 0
            ) THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = masterEntityId });
    }
}
```

Register in dependent module's `DependencyInjection.cs`:
```csharp
services.AddScoped<I{DependentEntity}Validation, {DependentEntity}ValidationRepository>();
```

Inject `I{DependentEntity}Validation` into Master A's `{MasterEntity}QueryRepository` and delegate the cross-module check to it.

**Step 3 — Delete Validator (`Delete{MasterEntity}CommandValidator`)**

Add `SoftDelete` case to the `switch`:

```csharp
case "SoftDelete":
    RuleFor(x => x.Id)
        .MustAsync(async (id, ct) =>
            !await _queryRepository.SoftDeleteValidationAsync(id))
        .WithMessage(
            "This master is linked with other records. You cannot delete this record.");
    break;
```

**Step 4 — Update Handler (`Update{MasterEntity}CommandHandler`)**

Add the inactivate guard BEFORE mapping and persisting:

```csharp
public async Task<ApiResponseDTO<int>> Handle(
    Update{MasterEntity}Command request, CancellationToken cancellationToken)
{
    if (request.IsActive == 0)
    {
        var isLinked = await _queryRepository.Is{MasterEntity}LinkedAsync(request.Id);
        if (isLinked)
            throw new ExceptionRules(
                "This master is linked with other records. You cannot inactivate this record.");
    }

    var entity = _mapper.Map<Domain.Entities.{MasterEntity}>(request);
    var result = await _commandRepository.UpdateAsync(entity);

    var auditEvent = new AuditLogsDomainEvent(
        actionDetail: "Update",
        actionCode: "{MASTER}_UPDATE",
        actionName: request.Id.ToString(),
        details: $"{MasterEntity} with Id {request.Id} updated successfully.",
        module: "{MasterEntity}"
    );
    await _mediator.Publish(auditEvent, cancellationToken);

    return new ApiResponseDTO<int>
    {
        IsSuccess = true,
        Message = "{MasterEntity} updated successfully.",
        Data = result
    };
}
```

---

#### SQL Rules

| Check | Filter Required |
|---|---|
| `SoftDeleteValidationAsync` | `WHERE IsDeleted = 0` on dependent table |
| `Is{MasterEntity}LinkedAsync` | `WHERE IsDeleted = 0 AND IsActive = 1` on dependent table |

> ❌ **NEVER check already-deleted or already-inactive records** — they are irrelevant to blocking.

---

#### Multiple Dependents — Use `OR EXISTS`

If Master A is referenced by more than one table, check all in a single query:

```csharp
public async Task<bool> SoftDeleteValidationAsync(int id)
{
    const string sql = @"
        SELECT CASE WHEN
            EXISTS (SELECT 1 FROM {Schema}.{TableB} WHERE {MasterEntity}Id = @Id AND IsDeleted = 0)
            OR
            EXISTS (SELECT 1 FROM {Schema}.{TableC} WHERE {MasterEntity}Id = @Id AND IsDeleted = 0)
        THEN 1 ELSE 0 END";

    return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
}
```

---

#### Error Message Format (fixed — do not customise)

| Operation | Error Message |
|---|---|
| Delete | `"This master is linked with other records. You cannot delete this record."` |
| Inactivate | `"This master is linked with other records. You cannot inactivate this record."` |

---

#### Response Format on Block (400)

```json
{
  "isSuccess": false,
  "message": "Validation failed",
  "errors": [
    "This master is linked with other records. You cannot delete this record."
  ],
  "statusCode": 400
}
```

---

#### Summary Table

| Dependent Type | Validation Approach | Where SQL Lives |
|---|---|---|
| Same-module | Direct Dapper `EXISTS` SQL | Master's own `QueryRepository` |
| Cross-module | Validation interface (`I{DependentEntity}Validation`) | Dependent module's `Infrastructure` |

> ⚠️ **This rule is triggered when implementing the DEPENDENT entity (Entity B), not when creating the master (Master A).** If Master A already exists without these checks, they must be added retroactively when Entity B is implemented.

---

### 26. **Mandatory Unit, Integration, QA, Functional & Smoke Tests; No Production/Live-DB Changes; New Modules Allowed**

This rule has **three parts** — all are mandatory.

> ⚖️ **All five test layers carry EQUAL weight.** Unit, Integration, QA (live-server), Functional (workflow/story), and the Smoke trait are **peers** — none is optional or secondary. A change is not done until **every applicable layer** exists and passes. QA/Functional are not a "nice-to-have pyramid top" — they are mandatory exactly like Unit and Integration.
>
> 🏗️ **If a module is missing its `.QATests` or `.FunctionalTests` project, CREATE it first** — scaffolded from the canonical reference suites (`UserManagement`, `FixedAssetManagement`, `MaintenanceManagement` for both QA and Functional) following the **🌐 QA Tests** and **🧭 Functional Tests** sections — **then proceed** with the entity/story work. Never skip a layer because its project does not yet exist.

---

#### Part A — Production Code & Live Database Are OFF-LIMITS

> ❌ **NEVER modify production code or the live/production database under any circumstances.**

| Forbidden Action | Why |
|---|---|
| Edit code in any production deployment | Production is frozen — only released through formal deployment process |
| Run `dotnet ef database update` against live/production DB | Schema changes go through DBA-approved migration windows only |
| Run any `INSERT` / `UPDATE` / `DELETE` / `DROP` SQL on production DB | Data integrity is protected by change-control |
| Connect tools/scripts to production connection strings | Dev environment uses `192.168.1.126` test servers only |
| Hot-fix on a production server file system | All fixes go through git → PR → CI/CD |

✅ **Allowed environments:**
- Local dev machine
- Test/dev SQL Server: `192.168.1.126`
- Test database: `{Module}_TestDb` (auto-recreated by `DbFixture`)
- Feature branches in git

---

#### Part B — Mandatory Unit, Integration, QA, Functional AND Smoke Tests for ALL Code Changes

> ❌ **NEVER make any code change or create new code (including new modules) without ALSO adding/updating ALL applicable test layers — Unit, Integration, QA (live-server), Functional (workflow/story), and the Smoke trait. These five are mandatory peers; none may be skipped or deferred.**

| Action | Tests Required (all applicable layers) |
|---|---|
| Create new module | ✅ Create **all four** projects: `{Module}.UnitTests` + `{Module}.IntegrationTests` + `{Module}.QATests` + `{Module}.FunctionalTests` |
| Create new entity in existing/new module | ✅ Unit + ✅ Integration + ✅ QA (full CRUD + **Smoke** trait on GetAll) + ✅ Functional story |
| Add column to existing entity | ✅ Update unit (handler + validator + entity) + ✅ Integration (command + query repos) + ✅ QA assertions + ✅ Functional story step |
| Modify existing handler/repo/validator/controller | ✅ Update unit + integration + QA (re-reconcile) + functional where the behavior is story-visible |
| Modify domain entity | ✅ Update entity unit tests + integration + QA as affected |
| Add/change AutoMapper profile | ✅ Update handler unit tests + QA where response shape changes |
| Bug fix | ✅ Regression test that fails before fix and passes after, at the layer that catches it (unit / integration / QA / functional) |

**The full test pyramid is mandatory** — Unit, Integration, QA, Functional, and Smoke all carry equal weight. A new entity/story is **not done** until all applicable layers exist and pass:

| Layer | Location | Required for |
|---|---|---|
| Unit tests | `src/tests/{Module}.UnitTests/` | every entity (handlers, validators, entity, controller) |
| Integration tests | `src/tests/{Module}.IntegrationTests/` | every entity (command + query repos) |
| **QA tests (live-server)** | `src/tests/{Module}.QATests/` | every entity — full CRUD + the **Smoke** trait on GetAll |
| **Functional tests (workflow/story)** | `src/tests/{Module}.FunctionalTests/` | every business story (e.g. `US-…`), gated by the Story-Catalogue |

> 🏗️ **If a required test project does not exist, CREATE it first — do not skip the layer.**
> Scaffold the missing project from the canonical reference suites and the matching section of this file:
> - `{Module}.UnitTests` / `{Module}.IntegrationTests` → **Unit Testing** / **Integration Testing** sections
> - `{Module}.QATests` → **🌐 QA Tests** section, modeled on `UserManagement.QATests` / `FixedAssetManagement.QATests` / `MaintenanceManagement.QATests`
> - `{Module}.FunctionalTests` → **🧭 Functional Tests** section, modeled on `UserManagement.FunctionalTests` / `FixedAssetManagement.FunctionalTests` / `MaintenanceManagement.FunctionalTests`
>
> Only after the project exists (referencing `Shared.QAInfrastructure`, serialized collections, `appsettings.QA.json`, etc.) do you author the tests and **proceed** with the work.

**Definition of "Done" (new entity / story):**
- [ ] Production code (dev/feature branch — never production) written/modified
- [ ] **Unit** tests pass: `dotnet test src/tests/{Module}.UnitTests`
- [ ] **Integration** tests pass: `dotnet test src/tests/{Module}.IntegrationTests`
- [ ] **QA** suite authored (CRUD + `Layer=Smoke` tag) and reconciled green against the live QA clone
- [ ] **Functional** story authored against an approved Story-Catalogue entry (blocked steps `[Fact(Skip)]`), reconciled green
- [ ] New QA/Functional projects **wired into the pipeline** (smoke gate + non-blocking full suites, per `deploy-and-test.yml`)
- [ ] Solution builds with `0 Warning(s), 0 Error(s)`

> ❌ DO NOT split production code and tests across separate commits — they belong in the same change set.
> ❌ DO NOT mark any task complete if **any applicable test layer** (unit / integration / QA / functional) is missing or failing.

---

#### Part C — New Modules Are Allowed (with full test coverage)

New modules **CAN** be created in BSOFT. When creating a new module:

1. Follow the **Standard Module Structure** at the top of this file
2. Register the module via the **New Module Registration Checklist** ([NewModuleRegistrationChecklist.md](NewModuleRegistrationChecklist.md))
3. **Create ALL FOUR test projects from day one** (scaffold each from the canonical reference suites):
   - `src/tests/{NewModule}.UnitTests/{NewModule}.UnitTests.csproj`
   - `src/tests/{NewModule}.IntegrationTests/{NewModule}.IntegrationTests.csproj`
   - `src/tests/{NewModule}.QATests/{NewModule}.QATests.csproj` (references `Shared.QAInfrastructure`)
   - `src/tests/{NewModule}.FunctionalTests/{NewModule}.FunctionalTests.csproj` (with `Stories/Story-Catalogue.md`)
4. Every entity in the new module must have **unit + integration + QA (CRUD + Smoke trait)** coverage, and every business story a **Functional** test, before merge

---

#### Approval Gate — Wait for User Confirmation Before Any Code Work

> ✋ Before making ANY code change, creating any new code, or creating any new module, the AI MUST:
> 1. Share the proposed plan / file list with the user
> 2. Wait for explicit confirmation (e.g., *"approved"*, *"proceed"*)
> 3. Only then write the dev-branch code AND its tests in the same change set
> 4. NEVER touch production code or live database — only dev environment + test DB

---

## 📄 Technical Documentation Templates

All generated documentation lives in `d:\BSOFT\docs\` using the naming:
- HLD: `docs/{EntityName}_HLD.md` → final output: `docs/{EntityName}_HLD.pdf` **(PDF — mandatory)**
- LLD: `docs/{EntityName}_LLD.docx` **(Word — mandatory, generated by Python script)**

---

### HLD Template — `{EntityName}_HLD.md`

```markdown
# {EntityName} — High Level Design (HLD)

**Module:** {ModuleName}
**Entity:** {EntityName}
**Schema:** {Schema}
**Author:** Development Team
**Date:** {Date}

---

## 1. Overview
[Brief description of what this entity represents and its business purpose]

## 2. Entity Relationships
| Relationship | Type | Referenced Entity | Module |
|---|---|---|---|
| {EntityName} → Company | Many-to-One | Company | UserManagement |

## 3. API Endpoints
| Method | Route | Description | Auth |
|---|---|---|---|
| GET | api/{EntityName} | Get all (paginated + search) | ✅ |
| GET | api/{EntityName}/{id} | Get by ID | ✅ |
| GET | api/{EntityName}/by-name | Autocomplete dropdown | ✅ |
| POST | api/{EntityName} | Create new | ✅ |
| PUT | api/{EntityName} | Update existing | ✅ |
| DELETE | api/{EntityName} | Soft delete | ✅ |

## 4. Data Flow
```
HTTP Request
  → {EntityName}Controller
    → MediatR.Send(Command/Query)
      → ValidationBehavior (FluentValidation)
        → {Verb}{EntityName}CommandHandler / QueryHandler
          → I{EntityName}CommandRepository (EF Core) / I{EntityName}QueryRepository (Dapper)
            → SQL Server [{Schema}].[{EntityName}]
              ← DTO response
          → IMediator.Publish(AuditLogsDomainEvent)
            → MongoDB AuditLogs
```

## 5. Cross-Module Dependencies
| Lookup Interface | Source Module | Used For |
|---|---|---|
| ICompanyLookup | UserManagement | Populate CompanyName in DTO |

## 6. Soft Delete Strategy
All records use `IsDeleted` flag. Physical deletion is never performed.
Queries always filter `WHERE IsDeleted = 0`.

## 7. Audit Strategy
Audit fields (`CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate`) are
auto-populated by `ApplicationDbContext.SaveChangesAsync()`. All create/update/delete
operations publish `AuditLogsDomainEvent` to MongoDB.
```

---

### LLD Template — `{EntityName}_LLD.md`

```markdown
# {EntityName} — Low Level Design (LLD)

**Module:** {ModuleName}
**Entity:** {EntityName}
**Author:** Development Team
**Date:** {Date}

---

## 1. Domain Entity

| Property | Type | Constraints | Notes |
|---|---|---|---|
| Id | int | PK, Identity | Auto-generated |
| {EntityName}Code | string | Required, Max 20, Alphanumeric, Unique | Immutable after create |
| {EntityName}Name | string | Required, Max 100 | |
| CompanyId | int | Required, FK (cross-module, no constraint) | |
| IsActive | Status enum | bit, Default: Active | |
| IsDeleted | IsDelete enum | bit, Default: NotDeleted | |
| CreatedBy | int | Auto-populated | |
| CreatedDate | DateTimeOffset? | Auto-populated | |
| ModifiedBy | int? | Auto-populated | |
| ModifiedDate | DateTimeOffset? | Auto-populated | |

## 2. Database Table Schema

**Table:** `[{Schema}].[{EntityName}]`

| Column | SQL Type | Nullable | Default | Index |
|---|---|---|---|---|
| Id | int IDENTITY(1,1) | NO | — | PK |
| {EntityName}Code | varchar(20) | NO | — | UQ |
| {EntityName}Name | varchar(100) | NO | — | — |
| CompanyId | int | NO | — | IX |
| IsActive | bit | NO | 1 | — |
| IsDeleted | bit | NO | 0 | — |
| CreatedBy | int | NO | 0 | — |
| CreatedDate | datetimeoffset | YES | — | — |
| CreatedByName | varchar(100) | YES | — | — |
| CreatedIP | varchar(50) | YES | — | — |
| ModifiedBy | int | YES | — | — |
| ModifiedDate | datetimeoffset | YES | — | — |
| ModifiedByName | varchar(100) | YES | — | — |
| ModifiedIP | varchar(50) | YES | — | — |

## 3. Repository Interfaces

### I{EntityName}CommandRepository
| Method | Returns | Description |
|---|---|---|
| CreateAsync(entity) | Task\<int\> | Insert new record, return new ID |
| UpdateAsync(entity) | Task\<int\> | Update existing record |
| SoftDeleteAsync(id, ct) | Task\<bool\> | Set IsDeleted = Deleted |

### I{EntityName}QueryRepository
| Method | Returns | Description |
|---|---|---|
| GetAllAsync(page, size, term) | Task\<(List\<Dto\>, int)\> | Paginated list with total count |
| GetByIdAsync(id) | Task\<Dto\> | Single record by ID |
| AutocompleteAsync(term, ct) | Task\<IReadOnlyList\<LookupDto\>\> | Active records matching term |
| AlreadyExistsAsync(code, id?) | Task\<bool\> | Uniqueness check (excludes self on update) |
| NotFoundAsync(id) | Task\<bool\> | Existence check |
| CompanyExistsAsync(id) | Task\<bool\> | FK validation via UserManagement |

## 4. Command Handler Logic

### Create{EntityName}CommandHandler
1. Map command → domain entity via `_mapper.Map<Domain.Entities.{EntityName}>(request)` (AutoMapper — never manual)
2. Set `IsActive = Active`, `IsDeleted = NotDeleted`
3. Call `_commandRepo.CreateAsync(entity)` → returns `newId`
4. Publish `AuditLogsDomainEvent` (ActionDetail: "Create", ActionCode: "{ENTITY}_CREATE")
5. Return `ApiResponseDTO<int>` with `newId`

### Update{EntityName}CommandHandler
1. Map command → entity via `_mapper.Map<Domain.Entities.{EntityName}>(request)` (AutoMapper — never manual)
2. Update mutable fields only (exclude Code — immutable)
3. Call `_commandRepo.UpdateAsync(entity)`
4. Publish `AuditLogsDomainEvent` (ActionDetail: "Update")
5. Return `ApiResponseDTO<int>`

### Delete{EntityName}CommandHandler
1. Call `_commandRepo.SoftDeleteAsync(command.Id, ct)` → returns bool
2. If false → throw `ExceptionRules("{EntityName} not found.")`
3. Publish `AuditLogsDomainEvent` (ActionDetail: "SoftDelete")
4. Return `true`

## 5. Validation Rules

### Create{EntityName}Command
| Field | Rule | Error Message |
|---|---|---|
| {EntityName}Code | NotEmpty | "{EntityName} Code is required." |
| {EntityName}Code | MaxLength(20) | "{EntityName} Code cannot exceed 20 characters." |
| {EntityName}Code | Matches `^[A-Za-z0-9]+$` | "{EntityName} Code must be alphanumeric." |
| {EntityName}Code | AlreadyExistsAsync == false | "{EntityName} Code already exists." |
| {EntityName}Name | NotEmpty | "{EntityName} Name is required." |
| {EntityName}Name | MaxLength(100) | "{EntityName} Name cannot exceed 100 characters." |
| CompanyId | GreaterThan(0) | "Valid Company is required." |
| CompanyId | CompanyExistsAsync == true | "Company does not exist in Company Master." |

### Update{EntityName}Command
| Field | Rule | Error Message |
|---|---|---|
| Id | GreaterThan(0) | "Valid ID is required." |
| {EntityName}Name | NotEmpty | "{EntityName} Name is required." |
| {EntityName}Name | MaxLength(100) | "{EntityName} Name cannot exceed 100 characters." |
| CompanyId | GreaterThan(0) | "Valid Company is required." |
| CompanyId | CompanyExistsAsync == true | "Company does not exist in Company Master." |
| IsActive | InclusiveBetween(0, 1) | "IsActive must be 0 or 1." |

## 6. DTO Structure

### {EntityName}Dto (Full)
| Field | Type | Source |
|---|---|---|
| Id | int | DB |
| {EntityName}Code | string | DB |
| {EntityName}Name | string | DB |
| CompanyId | int | DB |
| CompanyName | string | ICompanyLookup |
| IsActive | bool | DB |
| IsDeleted | bool | DB |
| CreatedBy | int | DB |
| CreatedDate | DateTimeOffset? | DB |

### {EntityName}LookupDto (Autocomplete)
| Field | Type |
|---|---|
| Id | int |
| {EntityName}Code | string |
| {EntityName}Name | string |

## 7. AutoMapper Profile
| Source | Destination | Notes |
|---|---|---|
| Create{EntityName}Command | {EntityName} entity | Code + Name + FK fields |
| Update{EntityName}Command | {EntityName} entity | Name + FK fields only (no Code) |

## 8. Dependency Injection
| Interface | Implementation | Lifetime |
|---|---|---|
| I{EntityName}CommandRepository | {EntityName}CommandRepository | Scoped |
| I{EntityName}QueryRepository | {EntityName}QueryRepository | Scoped |
```

---

### 27. **Polly v8 Resilience — Mandatory for Every New Module**

> 📄 Full checklist: [🛡️ Polly v8 Resilience — New Module Checklist](#️-polly-v8-resilience--new-module-checklist) (earlier in this file)

**Short rules (memorise these):**

> ❌ **NEVER add `.AddPollyHandler(...)` (Polly v7 API)**
> ✅ **ALWAYS use `.AddBsoftHttpResilience(profileName)` (Polly v8 wrapper)**

> ❌ **NEVER call `services.AddHttpClient()` bare without chaining `.AddBsoftHttpResilience`**
> ✅ Every typed or named `AddHttpClient` registration MUST chain `.AddBsoftHttpResilience`

> ❌ **NEVER add Polly inside a MediatR command/query handler**
> ✅ Handlers are idempotency-free — retry at handler level risks duplicate inserts

> ❌ **NEVER add Polly around EF Core `SaveChangesAsync`**
> ✅ `EnableRetryOnFailure(5)` is already configured on every `ApplicationDbContext`

**Mandatory for every new module:**

| Action | Mandatory / Optional | One-liner |
|---|---|---|
| HTTP client registered | **MANDATORY** | `.AddBsoftHttpResilience(ResilienceProfileNames.Standard)` |
| Hangfire job class | **MANDATORY** | `[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]` |
| High-value Dapper write | RECOMMENDED | `_resilience.ExecuteSqlAsync(ResilienceProfileNames.Standard, ct => ...)` |
| Critical MongoDB write | RECOMMENDED | `_resilience.ExecuteMongoTaskAsync(ResilienceProfileNames.Standard, ct => ...)` |
| Audit log MongoDB write | SKIP | Fire-and-forget — low risk, no wrap needed |

**Profile quick-pick:**

| Profile | Use When |
|---|---|
| `ResilienceProfileNames.Standard` | Internal services, SMS, WhatsApp, file fetch, reporting (default choice) |
| `ResilienceProfileNames.Critical` | GST/e-invoice, auth tokens, payment gateways — correctness over speed |
| `ResilienceProfileNames.FastFail` | SignalR push, UI polling — latency over correctness |

**No new NuGet packages, no new DI registrations, no `Program.cs` changes** — the foundation is global. A new module only needs:
```csharp
// {Module}.Infrastructure/DependencyInjection.cs
using Shared.Infrastructure.Resilience;  // ← add this using

services.AddHttpClient<IMyService, MyService>()
    .AddBsoftHttpResilience(ResilienceProfileNames.Standard);  // ← chain this
```

---

## 🎓 Learning Path

1. **Study Reference:** Read `SalesOrganisation` implementation thoroughly
2. **Understand BaseEntity:** Learn what's inherited vs what's custom
3. **Master CQRS:** Commands write, Queries read — strict separation
4. **Learn Lookup Pattern:** How cross-module references work
5. **Practice:** Implement a simple master entity (e.g., Department, Category)
6. **Review:** Compare your code with existing patterns
7. **Refactor:** Adjust to match conventions exactly

---

## 📞 Support & Troubleshooting

**Issue Tracker:** GitHub Issues
**Main Branch:** `usha_ModulerMonolithic`
**Current Branch:** `ClaudeTest` (for AI-assisted development)

**Common Problems:**
1. Build errors → Check DLL locks, stop app
2. 500 errors → Check database table exists, lookup services registered
3. Validation not firing → Verify `ValidationBehavior` registered
4. Audit fields null → Check `ApplicationDbContext.SaveChangesAsync()` override

---

**Version:** 1.4
**Last Updated:** 2026-05-08
**Maintainer:** Development Team
**AI Assistant:** Claude Sonnet 4.6

