# BSOFT ERP - Development Standards & Architecture Guide

**Project:** BSOFT Modular Monolith ERP System
**Framework:** ASP.NET Core 8.0
**Architecture:** CQRS + Repository Pattern + Modular Monolith
**Last Updated:** 2026-02-20

---

## 🎯 Quick Reference

- **Solution:** `d:\BSOFT\BSOFT.sln`
- **Entry Point:** `src/BSOFT.Bootstrapper/Program.cs`
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
public class Get{EntityName}ByIdQuery : IRequest<ApiResponseDTO<{EntityName}Dto>>
{
    public int Id { get; set; }
}
```

**AutoComplete Query:**
```csharp
public sealed record Get{EntityName}AutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<{EntityName}LookupDto>>;
```

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

## 🌐 Cross-Module References

### ❌ DO NOT Use Direct SQL JOINs

**Wrong:**
```sql
SELECT o.*, c.CompanyName
FROM Sales.SalesOrganisation o
INNER JOIN UserManagement.Company c ON o.CompanyId = c.CompanyId  -- ❌ Cross-module JOIN
```

### ✅ Use Lookup Services

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

---

## ✅ Validation Pattern (FluentValidation)

### Validator Structure

```csharp
public class Create{EntityName}CommandValidator : AbstractValidator<Create{EntityName}Command>
{
    private readonly I{EntityName}QueryRepository _queryRepo;

    public Create{EntityName}CommandValidator(I{EntityName}QueryRepository queryRepo)
    {
        _queryRepo = queryRepo;

        // 1. Required fields
        RuleFor(x => x.{UniqueCode})
            .NotEmpty().WithMessage("{Field} is required.")
            .MaximumLength(20).WithMessage("{Field} cannot exceed 20 characters.");

        // 2. Format validation
        RuleFor(x => x.{UniqueCode})
            .Matches("^[A-Za-z0-9]+$").WithMessage("{Field} must be alphanumeric.");

        // 3. Async uniqueness check
        RuleFor(x => x.{UniqueCode})
            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code))
            .WithMessage("{Field} already exists.")
            .When(x => !string.IsNullOrWhiteSpace(x.{UniqueCode}));

        // 4. FK existence validation
        RuleFor(x => x.{ForeignKey})
            .GreaterThan(0).WithMessage("Valid {Field} is required.")
            .MustAsync(async (id, ct) => await _queryRepo.{Referenced}ExistsAsync(id))
            .WithMessage("{Field} does not exist in {Referenced} Master.");
    }
}
```

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
[Authorize]  // ⚠️ Add if authentication required
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
    public async Task<IActionResult> Get{EntityName}ByIdAsync(int id) { /* ... */ }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Get{EntityName}AutoCompleteAsync([FromQuery] string term = null) { /* ... */ }

    [HttpPost("create")]
    public async Task<IActionResult> Create{EntityName}([FromBody] Create{EntityName}Command command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await Mediator.Send(command);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            isSuccess = result.IsSuccess,
            message = result.Message,
            data = result.Data
        });
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update{EntityName}([FromBody] Update{EntityName}Command command) { /* ... */ }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete{EntityName}(int id) { /* ... */ }
}
```

**Standard Endpoints:**
- `GET api/{Entity}` - List all (paginated)
- `GET api/{Entity}/{id}` - Get by ID
- `GET api/{Entity}/autocomplete?term=` - Autocomplete dropdown
- `POST api/{Entity}/create` - Create new
- `PUT api/{Entity}/update` - Update existing
- `DELETE api/{Entity}/delete/{id}` - Soft delete

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

---

## 🧪 EF Core Migrations

### Generate Migration

```bash
cd src/Modules/{Module}/{Module}.Infrastructure

dotnet ef migrations add {EntityName}Master --startup-project ../../../BSOFT.Bootstrapper
```

### Apply Migration

```bash
dotnet ef database update --startup-project ../../../BSOFT.Bootstrapper
```

### Design-Time Factory

Each module has `DesignTimeDbContextFactory.cs` that:
- Loads configuration from `BSOFT.Bootstrapper/appsettings.json`
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

### Common Issues

**Issue:** DLL files locked
**Fix:** Stop running application before building

**Issue:** `CachedLookupDecorator` error "base type cannot be sealed"
**Fix:** Remove `sealed` keyword from `CachedLookupDecorator<T>` class definition

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

- [ ] Copy template from `Master_Data_Implementation_Prompt_Template.md`
- [ ] Fill in entity details (name, fields, FKs, validations)
- [ ] Generate specification document (review with team)
- [ ] Create Domain entity extending `BaseEntity`
- [ ] Create Commands (Create, Update, Delete) + Handlers
- [ ] Create Queries (GetAll, GetById, AutoComplete) + Handlers
- [ ] Create DTOs in centralized `Dto/` folder
- [ ] Create Repository interfaces + implementations (Command + Query)
- [ ] Create EF Core entity configuration
- [ ] Create Controller with standard endpoints
- [ ] Create FluentValidation validators
- [ ] Create AutoMapper profile
- [ ] Register repositories in `DependencyInjection.cs`
- [ ] Add `DbSet` + `ApplyConfiguration` in `ApplicationDbContext`
- [ ] Generate + apply EF Core migration
- [ ] Build solution (0 warnings, 0 errors)
- [ ] Test all CRUD endpoints
- [ ] Verify audit logs in MongoDB
- [ ] Verify soft delete behavior
- [ ] Verify validation rules (uniqueness, FK existence)

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

### 8. **#nullable disable**
All C# files use `#nullable disable` (legacy project setting) — maintain consistency.

### 9. **Status vs IsActive**
- `IsActive = Active (1)` → Available in dropdowns
- `IsActive = Inactive (0)` → Hidden from dropdowns but retained

### 10. **Autocomplete Filtering**
Autocomplete MUST filter `WHERE IsActive = 1 AND IsDeleted = 0` — only show active, available records.

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

**Version:** 1.0
**Last Updated:** 2026-02-20
**Maintainer:** Development Team
**AI Assistant:** Claude Sonnet 4.5
