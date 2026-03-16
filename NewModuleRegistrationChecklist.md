# 🆕 New Module Registration Checklist

**Project:** BSOFT Modular Monolith ERP System
**Last Updated:** 2026-03-16

Every time a **new module** is created and wired into `BSOFT.Api`, ALL steps below are mandatory. Missing any one of them causes the exact bugs fixed in March 2026 (CS0246, route collisions, wrong audit log responses).

---

## Step 0 — Create the 5 Projects and Scaffold Common Files

Every module requires exactly **5 projects** and a set of **common files** that are identical across all modules (only namespaces change).

### 5 Project Structure

```
src/Modules/{ModuleName}/
├── Core/
│   ├── {ModuleName}.Domain/              ← entities, events, base classes
│   └── {ModuleName}.Application/         ← CQRS handlers, interfaces, mappings
├── {ModuleName}.Infrastructure/           ← EF Core, Dapper, MongoDB, services
├── {ModuleName}.Presentation/             ← controllers, validators
└── {ModuleName}.Module/                   ← DI registration entry point
```

After creating the projects, add them to `BSOFT.sln`:
- Create a solution folder named `{ModuleName}`
- Add all 5 `.csproj` files under that folder
- Add build configurations (Debug|Any CPU / Release|Any CPU) for each project

### Common Files to Copy from an Existing Module

Copy these files from any existing module (e.g., `FinanceManagement`) and replace the namespace prefix only.

**Domain — `Core/{ModuleName}.Domain/`**

| File | Path |
|---|---|
| `BaseEntity.cs` | `Common/BaseEntity.cs` |
| `AuditLogBase.cs` | `Common/AuditLogBase.cs` |
| `CommonEntity.cs` | `Common/CommonEntity.cs` |
| `AuditLogs.cs` | `Entities/AuditLogs.cs` |
| `JwtSettings.cs` | `Entities/JwtSettings.cs` |
| `AuditLogsDomainEvent.cs` | `Events/AuditLogsDomainEvent.cs` |

**Application — `Core/{ModuleName}.Application/`**

| File | Path |
|---|---|
| `AuditLogPublisher.cs` | `Common/AuditLogPublisher.cs` |
| `IMongoDbContext.cs` | `Common/Interfaces/IMongoDbContext.cs` |
| `IEventPublisher.cs` | `Common/Interfaces/IEventPublisher.cs` |
| `ITimeZoneService.cs` | `Common/Interfaces/ITimeZoneService.cs` |
| `IJwtTokenHelper.cs` | `Common/Interfaces/IJwtTokenHelper.cs` |
| `ILogQueryService.cs` | `Common/Interfaces/ILogQueryService.cs` |
| `IFileUploadService.cs` | `Common/Interfaces/IFileUploadService.cs` |
| `IAuditLogRepository.cs` | `Common/Interfaces/AuditLog/IAuditLogRepository.cs` |
| `IMapFrom.cs` | `Common/Mappings/IMapFrom.cs` |
| `MappingProfile.cs` | `Common/Mappings/MappingProfile.cs` |
| `AuditLogDomainEventHandler.cs` | `EventHandlers/AuditLogDomainEventHandler.cs` |
| `AuditLogDto.cs` | `AuditLog/Queries/GetAuditLog/AuditLogDto.cs` |
| `GetAuditLogQuery.cs` | `AuditLog/Queries/GetAuditLog/GetAuditLogQuery.cs` |
| `GetAuditLogQueryHandler.cs` | `AuditLog/Queries/GetAuditLog/GetAuditLogQueryHandler.cs` |
| `GetAuditLogAutoCompleteQuery.cs` | `AuditLog/Queries/GetAuditLogAutoComplete/GetAuditLogAutoCompleteQuery.cs` |
| `GetAuditLogAutoCompleteQueryHandler.cs` | `AuditLog/Queries/GetAuditLogAutoComplete/GetAuditLogAutoCompleteQueryHandler.cs` |

**Infrastructure — `{ModuleName}.Infrastructure/`**

| File | Path |
|---|---|
| `ApplicationDbContext.cs` | `Data/ApplicationDbContext.cs` |
| `MongoDbContext.cs` | `Data/MongoDbContext.cs` |
| `OutboxMessage.cs` | `Persistence/OutboxMessage.cs` |
| `DesignTimeDbContextFactory.cs` | `DesignTimeDbContextFactory.cs` |
| `AuditLogMongoRepository.cs` | `Repositories/AuditLog/AuditLogMongoRepository.cs` |
| `TimeZoneService.cs` | `Services/TimeZoneService.cs` |
| `EventPublisher.cs` | `Services/EventPublisher.cs` |
| `FileUploadRepository.cs` | `Services/FileUploadRepository.cs` |
| `JwtTokenHelper.cs` | `Services/JwtTokenHelper.cs` |
| `LogQueryService.cs` | `Services/LogQueryService.cs` |
| `AuthTokenHandler.cs` | `Services/AuthTokenHandler.cs` |
| `DependencyInjection.cs` | `DependencyInjection.cs` |

**Presentation — `{ModuleName}.Presentation/`**

| File | Path |
|---|---|
| `ApiControllerBase.cs` | `Controllers/ApiControllerBase.cs` |
| `AuditLogController.cs` | `Controllers/AuditLogController.cs` |
| `IMaxLengthProvider.cs` | `Validation/Common/IMaxLengthProvider.cs` |
| `MaxLengthProvider.cs` | `Validation/Common/MaxLengthProvider.cs` |

**Module — `{ModuleName}.Module/`**

| File | Path |
|---|---|
| `ModuleExtensions.cs` | `ModuleExtensions.cs` |

> ⚠️ **After copying, do a solution-wide find-and-replace on the namespace prefix** — e.g., replace `FinanceManagement` → `ProductionManagement` in all copied files.

### Verify all 5 projects build before proceeding

```bash
dotnet build src/Modules/{ModuleName}/Core/{ModuleName}.Domain/{ModuleName}.Domain.csproj --no-restore
dotnet build src/Modules/{ModuleName}/Core/{ModuleName}.Application/{ModuleName}.Application.csproj --no-restore
dotnet build src/Modules/{ModuleName}/{ModuleName}.Infrastructure/{ModuleName}.Infrastructure.csproj --no-restore
dotnet build src/Modules/{ModuleName}/{ModuleName}.Presentation/{ModuleName}.Presentation.csproj --no-restore
dotnet build src/Modules/{ModuleName}/{ModuleName}.Module/{ModuleName}.Module.csproj --no-restore
```

**Expected:** `0 Warning(s), 0 Error(s)` on all 5 projects before continuing.

---

## Step 1 — Add project references to `BSOFT.Api.csproj`

Both the `.Module` **and** `.Infrastructure` projects must be explicitly referenced:

```xml
<!-- Add {ModuleName} module -->
<ProjectReference Include="..\Modules\{ModuleName}\{ModuleName}.Module\{ModuleName}.Module.csproj" />
<ProjectReference Include="..\Modules\{ModuleName}\{ModuleName}.Infrastructure\{ModuleName}.Infrastructure.csproj" />
```

> ❌ **Missing either reference causes `CS0246` build error** — even if the Infrastructure is pulled in transitively through the Module, the explicit reference must still be present for consistency.

---

## Step 2 — Register the module in `Program.cs`

```csharp
using {ModuleName}.Module;
// ...
builder.Services.Add{ModuleName}Module(builder.Configuration, builder.Environment);
```

---

## Step 3 — Register the module in `SwaggerSetup.cs`

```csharp
new SwaggerModuleInfo("{ModuleName}", "{Module Display Name} API", "v1", "{ModuleName}.Presentation.Controllers"),
```

> The `NamespaceFilter` must exactly match the controller namespace used in `{ModuleName}.Presentation/Controllers/`.

---

## Step 4 — AuditLog Controller: MUST use module-prefix route

> ❌ **NEVER use `[Route("api/[controller]")]`** on an AuditLogController — this is always `api/auditlog` and will collide with every other module's AuditLog controller.

> ✅ **ALWAYS use a module-specific prefix:**
> ```csharp
> [Route("api/{modulePrefix}/[controller]")]   // e.g. api/production, api/finance, api/project
> public class AuditLogController : ApiControllerBase
> ```

**Established module prefixes (never reuse):**

| Module | Route Prefix |
|---|---|
| UserManagement | `api/usermanagement` |
| FixedAssetManagement | `api/fam` |
| MaintenanceManagement | `api/maintenance` |
| PurchaseManagement | `api/purchase` |
| InventoryManagement | `api/inventory` |
| PartyManagement | `api/party` |
| SalesManagement | `api/sales` |
| WarehouseManagement | `api/warehouse` |
| ProjectManagement | `api/project` |
| BudgetManagement | `api/budget` |
| GateEntryManagement | `api/gateentry` |
| FinanceManagement | `api/finance` |
| ProductionManagement | `api/production` |

> ✅ **This rule applies to ALL controllers in a module**, not just AuditLog. Every module controller MUST use `api/{modulePrefix}/[controller]` to prevent route collisions.

---

## Step 5 — AuditLog endpoints: standard structure

Every module's `AuditLogController` MUST follow this exact structure:

```csharp
[Route("api/{modulePrefix}/[controller]")]
public class AuditLogController : ApiControllerBase
{
    public AuditLogController(IMediator mediator) : base(mediator) { }

    // GET api/{modulePrefix}/AuditLog — no parameters, returns all logs
    [HttpGet]
    public async Task<IActionResult> GetAllAuditLogsAsync()
    {
        var result = await Mediator.Send(new GetAuditLogQuery());
        return Ok(result);
    }

    // GET api/{modulePrefix}/AuditLog/by-name?searchPattern=xxx — search logs
    [HttpGet("by-name")]
    public async Task<IActionResult> GetAuditLogAutoCompleteAsync([FromQuery] string? searchPattern = null)
    {
        var result = await Mediator.Send(new GetAuditLogAutoCompleteQuery { SearchPattern = searchPattern });
        return Ok(result);
    }
}
```

**AuditLog query return types (both must return `ApiResponseDTO`):**

```csharp
// GetAll — no parameters
public class GetAuditLogQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>> { }

// Search — searchPattern parameter
public class GetAuditLogAutoCompleteQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
{
    public string? SearchPattern { get; set; }
}
```

**Handler response structure:**
```csharp
// Success
return new ApiResponseDTO<List<AuditLogDto>> { IsSuccess = true, Message = "Success", Data = dtos };

// No results
return new ApiResponseDTO<List<AuditLogDto>> { IsSuccess = false, Message = "No audit logs found." };
```

> ❌ **NEVER return `List<string>` from AuditLog handlers** — always `ApiResponseDTO<List<AuditLogDto>>`
> ❌ **NEVER add `searchTerm` to `GetAuditLogQuery`** — the main GET endpoint has no parameters
> ❌ **NEVER wrap in extra anonymous object** `Ok(new { StatusCode, data = result })` — return `Ok(result)` directly when result is already `ApiResponseDTO`
