# BSOFT Modular Monolith - Complete Registration, Wiring & Deployment Guide

## 📋 Table of Contents

1. [Module Registration Architecture](#1-module-registration-architecture)
2. [How Modules are Registered](#2-how-modules-are-registered)
3. [How Modules are Wired Together](#3-how-modules-are-wired-together)
4. [API Endpoints Exposure](#4-api-endpoints-exposure)
5. [Request Execution Flow](#5-request-execution-flow)
6. [Deployment as Single Unit](#6-deployment-as-single-unit)
7. [Complete Examples](#7-complete-examples)

---

## 1️⃣ Module Registration Architecture

### Overview

In BSOFT Modular Monolith, **all modules are registered in Program.cs** but maintain logical separation through:
- Module interfaces
- Dependency injection
- Schema-based database separation
- Clear boundaries

### Registration Hierarchy

```
┌─────────────────────────────────────────────────────┐
│               Program.cs (Entry Point)               │
│  • Creates WebApplicationBuilder                     │
│  • Configures ALL services                          │
│  • Registers ALL modules                            │
│  • Builds single application                        │
└─────────────────────────────────────────────────────┘
                        │
                        ├──────────────────────────────┐
                        │                              │
┌───────────────────────▼────┐        ┌───────────────▼────────┐
│ UserManagement Module      │        │ FixedAsset Module      │
│                            │        │                        │
│ ModuleExtensions.cs        │        │ ModuleExtensions.cs    │
│ • AddUserManagementModule()│        │ • AddFixedAssetModule()│
│                            │        │                        │
│ Registers:                 │        │ Registers:             │
│ • IUserManagementModule    │        │ • IFixedAssetModule    │
│ • Repositories             │        │ • Repositories         │
│ • Domain Services          │        │ • Domain Services      │
│ • Application Services     │        │ • Application Services │
└────────────────────────────┘        └────────────────────────┘
```

### Key Principle

**Single Entry Point, Multiple Modules**:
- One `Program.cs` configures everything
- One `Startup` sequence
- One application instance
- One deployment unit
- Multiple logical modules with clear boundaries

---

## 2️⃣ How Modules are Registered

### Step-by-Step Registration Process

#### Step 1: Program.cs Calls Module Extension

**Location**: `src/API/Program.cs`

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ... other configurations ...

// MODULE REGISTRATION - This is where it happens!
builder.Services.AddUserManagementModule();      // ← Registers UserManagement
builder.Services.AddFixedAssetManagementModule(); // ← Registers FixedAssetManagement
builder.Services.AddInventoryModule();            // ← Registers Inventory
// ... other modules
```

#### Step 2: Module Extension Method Registers Services

**Location**: `src/Modules/UserManagement/ModuleExtensions.cs`

```csharp
namespace UserManagement.Module;

public static class ModuleExtensions
{
    public static IServiceCollection AddUserManagementModule(this IServiceCollection services)
    {
        // 1. Register Module Interface (for cross-module calls)
        services.AddScoped<IUserManagementModule, UserManagementModule>();
        
        // 2. Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        
        // 3. Register Domain Services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        
        // 4. Register Application Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        
        return services;
    }
}
```

#### Step 3: MediatR Handlers Registration

**In Program.cs**:

```csharp
// Register MediatR handlers from ALL modules
builder.Services.AddMediatR(cfg =>
{
    // UserManagement module handlers
    cfg.RegisterServicesFromAssembly(
        typeof(CreateUserCommandHandler).Assembly);
    
    // FixedAssetManagement module handlers
    cfg.RegisterServicesFromAssembly(
        typeof(CreateAssetCommandHandler).Assembly);
    
    // Inventory module handlers
    cfg.RegisterServicesFromAssembly(
        typeof(CreateItemCommandHandler).Assembly);
});
```

#### Step 4: Database Context Registration

**Single DbContext for ALL modules**:

```csharp
// Program.cs
builder.Services.AddDbContext<BSoftDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("BSOFT.Infrastructure");
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
        });
});
```

**BSoftDbContext includes ALL module entities**:

```csharp
// Shared.Infrastructure/Database/BSoftDbContext.cs
public class BSoftDbContext : DbContext
{
    // UserManagement entities
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Company> Companies => Set<Company>();
    
    // FixedAssetManagement entities
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetGroup> AssetGroups => Set<AssetGroup>();
    
    // Inventory entities
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Stock> Stocks => Set<Stock>();
    
    // ALL modules share this one DbContext!
}
```

### Registration Sequence Diagram

```
Program.cs starts
    │
    ├─→ Configure Logging
    ├─→ Configure Database (Single BSoftDbContext)
    ├─→ Configure EventBus (InMemoryEventBus)
    ├─→ Configure AutoMapper (All module profiles)
    ├─→ Configure MediatR (All module handlers)
    │
    ├─→ AddUserManagementModule()
    │      ├─→ Register IUserManagementModule
    │      ├─→ Register UserManagement repositories
    │      ├─→ Register UserManagement services
    │      └─→ Return services
    │
    ├─→ AddFixedAssetManagementModule()
    │      ├─→ Register IFixedAssetManagementModule
    │      ├─→ Register FixedAsset repositories
    │      ├─→ Register FixedAsset services
    │      └─→ Return services
    │
    ├─→ AddInventoryModule()
    │      └─→ ... same pattern
    │
    ├─→ Configure Authentication
    ├─→ Configure Controllers (All modules)
    ├─→ Configure Swagger
    │
    └─→ Build Application (Single app instance)
```

---

## 3️⃣ How Modules are Wired Together

### Cross-Module Communication

Modules communicate through **module interfaces** (not direct dependencies).

#### Example: FixedAsset Module Calling UserManagement Module

**Scenario**: When creating an asset, validate that the department exists.

##### Step 1: FixedAsset depends on IUserManagementModule

```csharp
// FixedAssetManagement/Application/Commands/CreateAsset/CreateAssetCommandHandler.cs
public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Result<int>>
{
    private readonly BSoftDbContext _context;
    private readonly IUserManagementModule _userModule;  // ← Module interface
    private readonly IEventBus _eventBus;

    public CreateAssetCommandHandler(
        BSoftDbContext context,
        IUserManagementModule userModule,  // ← Injected by DI
        IEventBus eventBus)
    {
        _context = context;
        _userModule = userModule;
        _eventBus = eventBus;
    }

    public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        // CROSS-MODULE CALL - Validate department exists
        var deptResult = await _userModule.GetDepartmentByIdAsync(request.DepartmentId, ct);
        
        if (deptResult.IsFailure)
            return Result.Failure<int>(Error.Validation("Department", "Invalid department"));
        
        // Department is valid, continue with asset creation
        var asset = new Asset
        {
            AssetName = request.AssetName,
            DepartmentId = request.DepartmentId,
            PurchaseCost = request.PurchaseCost,
            // ... other properties
        };
        
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(ct);
        
        // Raise domain event
        await _eventBus.PublishAsync(new AssetRegisteredEvent(asset.AssetId), ct);
        
        return Result.Success(asset.AssetId);
    }
}
```

##### Step 2: UserManagement implements the interface

```csharp
// UserManagement/UserManagementModule.cs
public class UserManagementModule : IUserManagementModule
{
    private readonly IDepartmentRepository _departmentRepository;

    public async Task<Result<DepartmentDto>> GetDepartmentByIdAsync(
        int departmentId, 
        CancellationToken ct)
    {
        // Direct repository access (fast path)
        var department = await _departmentRepository.GetByIdAsync(departmentId, ct);
        
        if (department == null)
            return Result.Failure<DepartmentDto>(Error.NotFound("Department", departmentId));
        
        var dto = new DepartmentDto(
            department.DepartmentId,
            department.DepartmentName,
            department.DepartmentCode,
            department.IsActive);
        
        return Result.Success(dto);
    }
}
```

##### Step 3: Dependency Injection wires it together

```csharp
// Program.cs already registered both:
builder.Services.AddUserManagementModule();      // Registers IUserManagementModule
builder.Services.AddFixedAssetManagementModule(); // Uses IUserManagementModule

// When CreateAssetCommandHandler is instantiated:
// DI automatically injects: UserManagementModule as IUserManagementModule
```

### Wiring Diagram

```
CreateAssetCommandHandler
         │
         │ (depends on)
         │
         ▼
IUserManagementModule interface
         ▲
         │ (implemented by)
         │
UserManagementModule class
         │
         │ (uses)
         │
         ▼
DepartmentRepository
         │
         ▼
BSoftDbContext (Single database)
```

### Key Benefits

1. **No direct module dependencies**: FixedAsset doesn't reference UserManagement directly
2. **Interface-based**: Loose coupling through interfaces
3. **In-process calls**: <0.1ms (vs gRPC 10-50ms)
4. **Type-safe**: Compile-time checking
5. **Testable**: Easy to mock interfaces

---

## 4️⃣ API Endpoints Exposure

### How Controllers are Registered

All controllers from all modules are registered in **one place**.

#### Step 1: Controllers Registration

```csharp
// Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
    });
```

This **automatically discovers ALL controllers** in the assembly:
- `UserController` from UserManagement
- `DepartmentController` from UserManagement
- `AssetController` from FixedAssetManagement
- `DepreciationController` from FixedAssetManagement
- All other controllers

#### Step 2: Controllers are in API Project

**Location**: `src/API/Controllers/`

```
src/API/Controllers/
├── UserManagement/
│   ├── UserController.cs
│   ├── DepartmentController.cs
│   ├── CompanyController.cs
│   └── ...
│
├── FixedAsset/
│   ├── AssetController.cs
│   ├── AssetGroupController.cs
│   ├── DepreciationController.cs
│   └── ...
│
├── Inventory/
│   ├── ItemController.cs
│   ├── StockController.cs
│   └── ...
```

#### Step 3: Controller Example

```csharp
// API/Controllers/FixedAsset/AssetController.cs
namespace BSOFT.API.Controllers.FixedAsset;

[ApiController]
[Route("api/[controller]")]  // → /api/asset
public class AssetController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AssetController(IMediator mediator)
    {
        _mediator = mediator;  // ← DI injects MediatR
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsset(int id)
    {
        // Send query to FixedAsset module via MediatR
        var query = new GetAssetByIdQuery(id);
        var result = await _mediator.Send(query);  // ← Routed to handler
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : NotFound(result.Error);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetCommand command)
    {
        // Send command to FixedAsset module via MediatR
        var result = await _mediator.Send(command);
        
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAsset), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
```

### Endpoint Organization in Swagger

```csharp
// Program.cs - Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BSOFT ERP API",
        Version = "v1",
        Description = "All modules in one API"
    });

    // Group endpoints by module
    c.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        
        if (controllerName?.Contains("Asset"))
            return new[] { "FixedAssetManagement" };
        if (controllerName?.Contains("User") || controllerName?.Contains("Department"))
            return new[] { "UserManagement" };
        if (controllerName?.Contains("Item") || controllerName?.Contains("Stock"))
            return new[] { "Inventory" };
        
        return new[] { "General" };
    });
});
```

**Result in Swagger UI**:
```
BSOFT ERP API
├── UserManagement
│   ├── POST /api/user
│   ├── GET /api/user/{id}
│   ├── GET /api/department
│   └── ...
│
├── FixedAssetManagement
│   ├── POST /api/asset
│   ├── GET /api/asset/{id}
│   ├── POST /api/asset/{id}/transfer
│   └── ...
│
└── Inventory
    ├── POST /api/item
    ├── GET /api/item/{id}
    └── ...
```

---

## 5️⃣ Request Execution Flow

### Complete Request Lifecycle

Let's trace a request: **Create Asset API call**

#### Request Flow Diagram

```
HTTP POST /api/asset
    │
    ├─→ 1. ASP.NET Core Pipeline
    │      ├─→ Middleware (Logging, Auth, etc.)
    │      └─→ Routing
    │
    ├─→ 2. AssetController
    │      ├─→ Receive CreateAssetCommand
    │      └─→ _mediator.Send(command)
    │
    ├─→ 3. MediatR Pipeline
    │      ├─→ Find handler: CreateAssetCommandHandler
    │      ├─→ Execute validation pipeline
    │      └─→ Call handler
    │
    ├─→ 4. CreateAssetCommandHandler
    │      ├─→ Cross-module call: _userModule.GetDepartmentByIdAsync()
    │      │      │
    │      │      ├─→ 4a. IUserManagementModule
    │      │      │      └─→ UserManagementModule.GetDepartmentByIdAsync()
    │      │      │            └─→ DepartmentRepository.GetByIdAsync()
    │      │      │                  └─→ BSoftDbContext.Departments
    │      │      │                        └─→ SQL: SELECT * FROM [UserManagement].[Departments]
    │      │      │
    │      │      └─→ Returns DepartmentDto
    │      │
    │      ├─→ Create Asset entity
    │      ├─→ _context.Assets.Add(asset)
    │      ├─→ _context.SaveChangesAsync()
    │      │      └─→ SQL: INSERT INTO [FixedAsset].[Assets]
    │      │
    │      └─→ _eventBus.PublishAsync(AssetRegisteredEvent)
    │             └─→ In-process event handlers execute
    │
    ├─→ 5. Return Result<int>
    │
    └─→ 6. AssetController
           └─→ Return HTTP 201 Created
```

### Step-by-Step Execution

#### Step 1: HTTP Request Arrives

```http
POST https://localhost:5001/api/asset
Content-Type: application/json

{
  "assetName": "Dell Laptop",
  "assetGroupId": 5,
  "assetCategoryId": 12,
  "purchaseCost": 75000.00,
  "purchaseDate": "2024-01-15",
  "departmentId": 3,
  "depreciationMethod": 1,
  "usefulLifeYears": 5
}
```

#### Step 2: Middleware Pipeline

```csharp
// Program.cs middleware pipeline
app.UseSerilogRequestLogging();   // ← Log request
app.UseAuthentication();          // ← Validate JWT token
app.UseAuthorization();           // ← Check permissions
app.MapControllers();             // ← Route to controller
```

#### Step 3: Controller Receives Request

```csharp
// AssetController.cs
[HttpPost]
public async Task<IActionResult> CreateAsset([FromBody] CreateAssetCommand command)
{
    // Model binding creates command from JSON
    // command.AssetName = "Dell Laptop"
    // command.DepartmentId = 3
    // etc.
    
    // Send to MediatR
    var result = await _mediator.Send(command);
    
    if (result.IsSuccess)
        return CreatedAtAction(nameof(GetAsset), new { id = result.Value }, result.Value);
    else
        return BadRequest(result.Error);
}
```

#### Step 4: MediatR Routes to Handler

```csharp
// MediatR pipeline:
// 1. Find handler for CreateAssetCommand
// 2. Execute validation behaviors (if configured)
// 3. Call CreateAssetCommandHandler.Handle()
```

#### Step 5: Handler Executes Business Logic

```csharp
// CreateAssetCommandHandler.cs
public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
{
    // CROSS-MODULE CALL (In-process <0.1ms)
    var deptResult = await _userModule.GetDepartmentByIdAsync(request.DepartmentId, ct);
    
    if (deptResult.IsFailure)
        return Result.Failure<int>(Error.Validation("Department", "Invalid"));
    
    // Generate asset number
    var assetNumber = await _assetNumberGenerator.GenerateAsync(ct);
    
    // Create entity
    var asset = new Asset
    {
        AssetNumber = assetNumber,
        AssetName = request.AssetName,
        DepartmentId = request.DepartmentId,
        PurchaseCost = request.PurchaseCost,
        BookValue = request.PurchaseCost,
        Status = AssetStatus.Active
    };
    
    // Save to database (Single transaction)
    _context.Assets.Add(asset);
    await _context.SaveChangesAsync(ct);
    
    // Raise domain event
    await _eventBus.PublishAsync(new AssetRegisteredEvent(asset.AssetId), ct);
    
    return Result.Success(asset.AssetId);
}
```

#### Step 6: Cross-Module Call Execution

```csharp
// UserManagementModule.cs
public async Task<Result<DepartmentDto>> GetDepartmentByIdAsync(int deptId, CancellationToken ct)
{
    // Direct repository call (same process, same database)
    var department = await _departmentRepository.GetByIdAsync(deptId, ct);
    
    if (department == null)
        return Result.Failure<DepartmentDto>(Error.NotFound("Department", deptId));
    
    return Result.Success(new DepartmentDto(...));
}
```

#### Step 7: Database Operations

```sql
-- All in SAME database, SINGLE transaction

-- 1. Validate department (from cross-module call)
SELECT * FROM [UserManagement].[Departments] 
WHERE DepartmentId = 3 AND IsDeleted = 0;

-- 2. Insert asset
INSERT INTO [FixedAsset].[Assets] 
(AssetNumber, AssetName, DepartmentId, PurchaseCost, ...)
VALUES ('FA-2024-001', 'Dell Laptop', 3, 75000.00, ...);

-- 3. Commit transaction (ACID)
COMMIT;
```

#### Step 8: Event Processing

```csharp
// In-process event bus publishes to handlers
public class AssetRegisteredEventHandler : INotificationHandler<AssetRegisteredEvent>
{
    public async Task Handle(AssetRegisteredEvent notification, CancellationToken ct)
    {
        // Send notification email
        // Update dashboards
        // Trigger workflows
        // All happen in the same process!
    }
}
```

#### Step 9: Return Response

```http
HTTP/1.1 201 Created
Location: /api/asset/145
Content-Type: application/json

{
  "assetId": 145,
  "assetNumber": "FA-2024-001",
  "assetName": "Dell Laptop",
  "status": "Active"
}
```

### Performance Comparison

| Operation | Microservices | Modular Monolith |
|-----------|---------------|------------------|
| Validate Department | gRPC call: 15ms | Direct call: <0.1ms |
| Create Asset | Insert: 5ms | Insert: 5ms |
| Raise Event | RabbitMQ publish: 10ms | In-process: <0.1ms |
| **Total** | **~30ms** | **~5ms** |

**Result**: 6x faster!

---

## 6️⃣ Deployment as Single Unit

### Build Process

#### Step 1: Build Command

```bash
# Build entire solution
dotnet build BSOFT.sln --configuration Release

# Or build just the API (includes all modules)
dotnet build src/API/BSOFT.API.csproj --configuration Release
```

**What happens**:
- All module projects are compiled
- All dependencies resolved
- Single output assembly created

#### Step 2: Publish Command

```bash
# Publish for deployment
dotnet publish src/API/BSOFT.API.csproj \
  --configuration Release \
  --output ./publish \
  --runtime win-x64 \
  --self-contained false
```

**Output structure**:
```
publish/
├── BSOFT.API.dll                      ← Main assembly
├── BSOFT.Infrastructure.dll           ← Shared infrastructure
├── UserManagement.Module.dll          ← UserManagement module
├── FixedAssetManagement.Module.dll    ← FixedAsset module
├── Inventory.Module.dll               ← Inventory module
├── appsettings.json
├── appsettings.Production.json
└── wwwroot/
```

**Key point**: One main DLL (`BSOFT.API.dll`) references all module DLLs.

### Deployment Options

#### Option 1: Traditional Server (IIS, Kestrel)

```bash
# Copy published files to server
scp -r ./publish/* user@server:/var/www/bsoft/

# Run on server
cd /var/www/bsoft
dotnet BSOFT.API.dll
```

#### Option 2: Docker Container

**Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/API/BSOFT.API.csproj"
RUN dotnet build "src/API/BSOFT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/API/BSOFT.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BSOFT.API.dll"]
```

**Build and run**:
```bash
# Build image
docker build -t bsoft-erp:latest .

# Run container
docker run -d \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=BSOFT;..." \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --name bsoft-app \
  bsoft-erp:latest
```

#### Option 3: Azure App Service

```bash
# Deploy directly from CLI
az webapp deployment source config-zip \
  --resource-group BSOFT-RG \
  --name bsoft-app \
  --src ./publish.zip
```

#### Option 4: Kubernetes (if needed for scaling)

**deployment.yaml**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bsoft-app
spec:
  replicas: 3  # Scale horizontally
  selector:
    matchLabels:
      app: bsoft
  template:
    metadata:
      labels:
        app: bsoft
    spec:
      containers:
      - name: bsoft-api
        image: bsoft-erp:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: bsoft-secrets
              key: db-connection
```

### Deployment Benefits

| Aspect | Microservices (8 services) | Modular Monolith (1 service) |
|--------|----------------------------|------------------------------|
| **Build time** | 15 min (8 builds) | 5 min (1 build) |
| **Deployment** | 8 separate deployments | 1 deployment |
| **Configuration** | 8 appsettings files | 1 appsettings file |
| **Connection strings** | 8 databases | 1 database |
| **Monitoring** | 8 endpoints | 1 endpoint |
| **Scaling** | Complex orchestration | Simple horizontal scaling |
| **Rollback** | Coordinate 8 services | Single rollback |

### Configuration Management

**Single appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=BSOFT;"
  },
  "JwtSettings": {
    "Secret": "...",
    "Issuer": "BSOFT",
    "Audience": "BSOFT_Users"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedOrigins": [
    "https://app.bsoft.com",
    "https://admin.bsoft.com"
  ]
}
```

**Environment-specific**:
```
appsettings.json              ← Base configuration
appsettings.Development.json  ← Dev overrides
appsettings.Staging.json      ← Staging overrides
appsettings.Production.json   ← Production overrides
```

### Database Migration in Production

```bash
# Option 1: Run migration on deployment
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/API \
  --connection "Server=prod-sql;Database=BSOFT;..."

# Option 2: Generate SQL script and apply via DBA
dotnet ef migrations script \
  --project src/Infrastructure \
  --startup-project src/API \
  --idempotent \
  --output migration.sql
```

---

## 7️⃣ Complete Examples

### Example 1: Adding a New Module (Inventory)

#### Step 1: Create Module Structure
```bash
mkdir -p src/Modules/Inventory/Core/{Core.Domain,Core.Application}
mkdir -p src/Modules/Inventory/Infrastructure
```

#### Step 2: Create Module Extension

```csharp
// src/Modules/Inventory/ModuleExtensions.cs
namespace Inventory.Module;

public static class ModuleExtensions
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IInventoryModule, InventoryModule>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        // ... register all services
        
        return services;
    }
}
```

#### Step 3: Register in Program.cs

```csharp
// src/API/Program.cs
builder.Services.AddUserManagementModule();
builder.Services.AddFixedAssetManagementModule();
builder.Services.AddInventoryModule();  // ← Add this line

// Register MediatR handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateAssetCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateItemCommandHandler).Assembly);  // ← Add this
});
```

#### Step 4: Add Entities to DbContext

```csharp
// Shared.Infrastructure/Database/BSoftDbContext.cs
public class BSoftDbContext : DbContext
{
    // ... existing entities ...
    
    // Inventory entities (ADD THESE)
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Indent> Indents => Set<Indent>();
}
```

#### Step 5: Create Migration

```bash
dotnet ef migrations add AddInventoryModule \
  --project src/Shared.Infrastructure \
  --startup-project src/API
```

#### Step 6: Update Database

```bash
dotnet ef database update \
  --project src/Shared.Infrastructure \
  --startup-project src/API
```

#### Step 7: Run Application

```bash
dotnet run --project src/API
```

**Result**: Inventory module is now part of the application!

### Example 2: Cross-Module Integration

**Scenario**: Inventory module needs to validate department when creating stock location.

#### In Inventory Module

```csharp
// Inventory/Application/Commands/CreateStockLocation/CreateStockLocationCommandHandler.cs
public class CreateStockLocationCommandHandler : IRequestHandler<CreateStockLocationCommand, Result<int>>
{
    private readonly BSoftDbContext _context;
    private readonly IUserManagementModule _userModule;  // ← Inject interface

    public async Task<Result<int>> Handle(CreateStockLocationCommand request, CancellationToken ct)
    {
        // Cross-module validation
        var deptResult = await _userModule.ValidateDepartmentExistsAsync(request.DepartmentId, ct);
        
        if (deptResult.IsFailure || !deptResult.Value)
            return Result.Failure<int>(Error.Validation("Department", "Invalid department"));
        
        // Create stock location
        var location = new StockLocation
        {
            LocationName = request.LocationName,
            DepartmentId = request.DepartmentId
        };
        
        _context.StockLocations.Add(location);
        await _context.SaveChangesAsync(ct);
        
        return Result.Success(location.LocationId);
    }
}
```

**No additional wiring needed!** DI automatically provides `IUserManagementModule`.

### Example 3: Running Locally

```bash
# 1. Clone repository
git clone https://github.com/yourorg/bsoft.git
cd bsoft

# 2. Restore packages
dotnet restore

# 3. Update connection string
# Edit: src/API/appsettings.Development.json

# 4. Create database
dotnet ef database update \
  --project src/Shared.Infrastructure \
  --startup-project src/API

# 5. Run application
dotnet run --project src/API

# 6. Access application
# Swagger: https://localhost:5001/swagger
# Health: https://localhost:5001/health
```

### Example 4: Production Deployment

```bash
# 1. Build release
dotnet publish src/API/BSOFT.API.csproj \
  --configuration Release \
  --output ./publish

# 2. Copy to server
scp -r ./publish/* user@prod-server:/opt/bsoft/

# 3. On production server
ssh user@prod-server
cd /opt/bsoft

# 4. Run as systemd service
sudo systemctl start bsoft
sudo systemctl enable bsoft

# 5. Check logs
sudo journalctl -u bsoft -f
```

---

## 🎉 Summary

### Module Registration
✅ **Single entry point**: Program.cs registers everything  
✅ **Extension methods**: Each module has `AddModuleName()` method  
✅ **DI container**: All services registered in one container  
✅ **MediatR**: Handlers from all modules registered centrally  

### Module Wiring
✅ **Module interfaces**: Loose coupling between modules  
✅ **Direct calls**: In-process communication (<0.1ms)  
✅ **Shared DbContext**: Single database, multiple schemas  
✅ **Event bus**: In-process event handling  

### API Endpoints
✅ **Single API**: All controllers in one assembly  
✅ **Automatic discovery**: ASP.NET Core finds all controllers  
✅ **Swagger grouping**: Endpoints organized by module  
✅ **Unified security**: JWT authentication for all endpoints  

### Deployment
✅ **Single unit**: One build, one deployment  
✅ **Simple configuration**: One appsettings file  
✅ **Easy scaling**: Horizontal scaling of single service  
✅ **Fast builds**: 5 minutes vs 15 minutes for microservices  

**Result**: A production-ready, modular monolithic application that's simpler to deploy and faster than microservices!
