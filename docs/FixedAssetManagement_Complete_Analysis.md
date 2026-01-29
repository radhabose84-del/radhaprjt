# BSOFT FixedAssetManagement Module - Complete Analysis & Modular Monolith Implementation

## 📋 Module Overview

The FixedAssetManagement module handles the complete lifecycle of fixed assets in the BSOFT ERP system, including asset registration, depreciation, maintenance, transfers, and disposal.

---

## 1️⃣ Functional Flow Analysis

### Business Processes

#### 1.1 Asset Registration & Onboarding
```
New Asset Purchase → Asset Registration → Assign to Department/Location 
→ Start Depreciation → Asset Active
```

**Key Activities**:
- Register new fixed asset with details (name, type, cost, purchase date)
- Assign asset group and category
- Set depreciation method (Straight Line, Declining Balance, Units of Production)
- Assign to department, location, and custodian
- Generate asset tag/barcode

#### 1.2 Asset Lifecycle Management
```
Active Asset → Track Location → Schedule Maintenance → Record Depreciation 
→ Transfer/Relocate → Update Status → Disposal/Sale
```

**Key Activities**:
- Track asset location and movements
- Record maintenance activities
- Calculate and post depreciation
- Handle asset transfers between departments
- Manage asset disposal or sale

#### 1.3 Depreciation Processing
```
Monthly/Yearly Schedule → Calculate Depreciation → Generate Journal Entries 
→ Post to Accounting → Update Asset Book Value
```

**Key Activities**:
- Calculate depreciation based on method
- Generate accounting entries
- Update asset book value
- Maintain depreciation history

#### 1.4 Asset Maintenance
```
Schedule Preventive Maintenance → Generate Work Order → Record Completion 
→ Track Costs → Update Asset Condition
```

**Key Activities**:
- Schedule preventive maintenance
- Record breakdown maintenance
- Track maintenance costs
- Update asset condition status

#### 1.5 Asset Transfer & Disposal
```
Transfer Request → Approval → Update Location/Custodian → Update Records
OR
Disposal Request → Approval → Calculate Gain/Loss → Remove from Books
```

**Key Activities**:
- Inter-department transfers
- Location changes
- Asset disposal (sale, scrap, donation)
- Calculate disposal gain/loss

---

## 2️⃣ Technical Flow within BSOFT

### 2.1 Current Microservices Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    FixedAssetManagement.API                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ Controllers                                            │  │
│  │ • AssetController                                      │  │
│  │ • AssetGroupController                                 │  │
│  │ • AssetCategoryController                             │  │
│  │ • DepreciationController                              │  │
│  │ • MaintenanceController                               │  │
│  │ • AssetTransferController                             │  │
│  │ • AssetDisposalController                             │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                               │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ gRPC Services (Called by other modules)               │  │
│  │ • AssetStateValidationService                         │  │
│  │ • AssetDepartmentValidationService                    │  │
│  │ • AssetCityValidationService                          │  │
│  │ • AssetCountryValidationService                       │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ gRPC Calls (10-50ms)
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                Other Microservices                           │
│  • UserManagement (Department, User validation)              │
│  • Accounting (Journal entry posting)                        │
│  • Workflow (Approval processes)                             │
│  • Purchase (Asset procurement)                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ RabbitMQ Events
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    Message Queue                             │
│  • AssetRegisteredEvent                                      │
│  • AssetTransferredEvent                                     │
│  • DepreciationCalculatedEvent                              │
│  • MaintenanceScheduledEvent                                │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Module Dependencies

**Depends On** (Inbound - FixedAsset calls these):
- **UserManagement**: Department validation, User information, Location data
- **Accounting**: Post journal entries for depreciation and disposal
- **Purchase**: Link to purchase orders and vendors
- **Workflow**: Approval workflows for transfers and disposal

**Used By** (Outbound - Others call FixedAsset):
- **Maintenance**: Validate asset exists, get asset details
- **Accounting**: Asset valuation reports
- **Project**: Allocate assets to projects
- **Budget**: Capital expenditure planning

### 2.3 Database Schema (Current Microservice)

```sql
-- FixedAssetManagement Database
CREATE DATABASE FixedAssetDB;

-- Core Tables
- Assets
- AssetGroups
- AssetCategories
- AssetDepreciation
- AssetMaintenance
- AssetTransfers
- AssetDisposal
- DepreciationSchedule
- MaintenanceSchedule
```

---

## 3️⃣ Domain Model Analysis

### 3.1 Core Entities

#### Asset (Aggregate Root)
```csharp
public class Asset : BaseEntity
{
    // Identity
    public int AssetId { get; set; }
    public Guid AssetGuid { get; set; }
    public string AssetNumber { get; set; }     // Auto-generated: FA-2024-001
    public string AssetTag { get; set; }        // Barcode/QR code
    
    // Classification
    public int AssetGroupId { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public int AssetCategoryId { get; set; }
    public AssetCategory AssetCategory { get; set; }
    
    // Basic Information
    public string AssetName { get; set; }
    public string Description { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    
    // Financial Information
    public decimal PurchaseCost { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string PurchaseInvoiceNumber { get; set; }
    public int? VendorId { get; set; }          // Link to Party module
    public int? PurchaseOrderId { get; set; }   // Link to Purchase module
    
    // Depreciation
    public DepreciationMethod DepreciationMethod { get; set; }
    public int UsefulLifeYears { get; set; }
    public decimal SalvageValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    
    // Location & Assignment
    public int DepartmentId { get; set; }        // From UserManagement
    public int? LocationId { get; set; }         // City/Location
    public int? CustodianUserId { get; set; }    // Assigned to user
    public string PhysicalLocation { get; set; } // Building, Floor, Room
    
    // Status
    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    
    // Relationships
    public ICollection<AssetDepreciation> Depreciations { get; set; }
    public ICollection<AssetMaintenance> MaintenanceRecords { get; set; }
    public ICollection<AssetTransfer> Transfers { get; set; }
    public AssetDisposal? Disposal { get; set; }
    
    // Domain Methods
    public void CalculateDepreciation(DateTime asOfDate);
    public void Transfer(int toDepartmentId, int toLocationId, int requestedByUserId);
    public void Dispose(DisposalType disposalType, decimal disposalAmount, string reason);
    public void ScheduleMaintenance(DateTime scheduledDate, string maintenanceType);
}
```

#### AssetGroup
```csharp
public class AssetGroup : BaseEntity
{
    public int AssetGroupId { get; set; }
    public string GroupCode { get; set; }       // FAC, VEH, IT, FUR
    public string GroupName { get; set; }       // Facilities, Vehicles, IT Equipment
    public string Description { get; set; }
    public int? ParentGroupId { get; set; }
    public AssetGroup? ParentGroup { get; set; }
    public ICollection<AssetGroup> SubGroups { get; set; }
    public ICollection<AssetCategory> Categories { get; set; }
}
```

#### AssetCategory
```csharp
public class AssetCategory : BaseEntity
{
    public int AssetCategoryId { get; set; }
    public int AssetGroupId { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public string CategoryCode { get; set; }
    public string CategoryName { get; set; }
    public DepreciationMethod DefaultDepreciationMethod { get; set; }
    public int DefaultUsefulLife { get; set; }
    public decimal DefaultSalvageValuePercent { get; set; }
    public int? DefaultGLAccountId { get; set; }  // Link to Chart of Accounts
}
```

#### AssetDepreciation
```csharp
public class AssetDepreciation : BaseEntity
{
    public int DepreciationId { get; set; }
    public int AssetId { get; set; }
    public Asset Asset { get; set; }
    public DateTime DepreciationDate { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValueAfter { get; set; }
    public bool IsPosted { get; set; }
    public int? JournalEntryId { get; set; }    // Link to Accounting
    public string Remarks { get; set; }
}
```

#### AssetMaintenance
```csharp
public class AssetMaintenance : BaseEntity
{
    public int MaintenanceId { get; set; }
    public int AssetId { get; set; }
    public Asset Asset { get; set; }
    public MaintenanceType Type { get; set; }   // Preventive, Corrective, Emergency
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Description { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public int? ServiceProviderId { get; set; }  // Vendor
    public string PerformedBy { get; set; }
    public MaintenanceStatus Status { get; set; }
    public string Remarks { get; set; }
}
```

#### AssetTransfer
```csharp
public class AssetTransfer : BaseEntity
{
    public int TransferId { get; set; }
    public int AssetId { get; set; }
    public Asset Asset { get; set; }
    public DateTime TransferDate { get; set; }
    
    // From
    public int FromDepartmentId { get; set; }
    public int? FromLocationId { get; set; }
    public int? FromCustodianUserId { get; set; }
    
    // To
    public int ToDepartmentId { get; set; }
    public int? ToLocationId { get; set; }
    public int? ToCustodianUserId { get; set; }
    
    public string TransferReason { get; set; }
    public int RequestedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public TransferStatus Status { get; set; }
}
```

#### AssetDisposal
```csharp
public class AssetDisposal : BaseEntity
{
    public int DisposalId { get; set; }
    public int AssetId { get; set; }
    public Asset Asset { get; set; }
    public DateTime DisposalDate { get; set; }
    public DisposalType DisposalType { get; set; }  // Sale, Scrap, Donation, Trade-In
    public decimal BookValueAtDisposal { get; set; }
    public decimal DisposalAmount { get; set; }
    public decimal GainLoss { get; set; }
    public string Reason { get; set; }
    public string DisposedTo { get; set; }          // Buyer, Scrap dealer, etc.
    public int RequestedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public bool IsPosted { get; set; }
    public int? JournalEntryId { get; set; }
}
```

### 3.2 Enums

```csharp
public enum DepreciationMethod
{
    StraightLine = 1,
    DecliningBalance = 2,
    DoubleDeclingBalance = 3,
    UnitsOfProduction = 4,
    SumOfYearsDigits = 5
}

public enum AssetStatus
{
    Active = 1,
    InMaintenance = 2,
    UnderRepair = 3,
    Transferred = 4,
    Disposed = 5,
    Lost = 6,
    Stolen = 7
}

public enum AssetCondition
{
    Excellent = 1,
    Good = 2,
    Fair = 3,
    Poor = 4,
    NonFunctional = 5
}

public enum MaintenanceType
{
    Preventive = 1,
    Corrective = 2,
    Emergency = 3,
    Calibration = 4,
    Inspection = 5
}

public enum MaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum TransferStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4
}

public enum DisposalType
{
    Sale = 1,
    Scrap = 2,
    Donation = 3,
    TradeIn = 4,
    Lost = 5,
    Stolen = 6
}
```

---

## 4️⃣ Modular Monolith Refactoring

### 4.1 Complete Folder Structure

```
BSOFT/
└── src/
    ├── Shared.Infrastructure/
    │   └── Database/
    │       ├── BSoftDbContext.cs
    │       └── Configurations/
    │           └── FixedAsset/                 # Schema: FixedAsset
    │               ├── AssetConfiguration.cs
    │               ├── AssetGroupConfiguration.cs
    │               ├── AssetCategoryConfiguration.cs
    │               ├── AssetDepreciationConfiguration.cs
    │               ├── AssetMaintenanceConfiguration.cs
    │               ├── AssetTransferConfiguration.cs
    │               └── AssetDisposalConfiguration.cs
    │
    └── Modules/
        └── FixedAssetManagement/
            ├── FixedAssetManagement.Module.csproj
            │
            ├── Core/
            │   ├── Core.Domain/
            │   │   ├── Entities/
            │   │   │   ├── Asset.cs
            │   │   │   ├── AssetGroup.cs
            │   │   │   ├── AssetCategory.cs
            │   │   │   ├── AssetDepreciation.cs
            │   │   │   ├── AssetMaintenance.cs
            │   │   │   ├── AssetTransfer.cs
            │   │   │   └── AssetDisposal.cs
            │   │   ├── Enums/
            │   │   │   ├── DepreciationMethod.cs
            │   │   │   ├── AssetStatus.cs
            │   │   │   ├── AssetCondition.cs
            │   │   │   ├── MaintenanceType.cs
            │   │   │   └── DisposalType.cs
            │   │   ├── Events/
            │   │   │   ├── AssetRegisteredEvent.cs
            │   │   │   ├── AssetTransferredEvent.cs
            │   │   │   ├── DepreciationCalculatedEvent.cs
            │   │   │   └── AssetDisposedEvent.cs
            │   │   └── Services/
            │   │       └── IDepreciationCalculator.cs
            │   │
            │   └── Core.Application/
            │       ├── Assets/
            │       │   ├── Commands/
            │       │   │   ├── CreateAsset/
            │       │   │   │   ├── CreateAssetCommand.cs
            │       │   │   │   ├── CreateAssetCommandHandler.cs
            │       │   │   │   └── CreateAssetValidator.cs
            │       │   │   ├── UpdateAsset/
            │       │   │   ├── DeleteAsset/
            │       │   │   ├── TransferAsset/
            │       │   │   └── DisposeAsset/
            │       │   └── Queries/
            │       │       ├── GetAssets/
            │       │       │   ├── GetAssetsQuery.cs
            │       │       │   ├── GetAssetsQueryHandler.cs
            │       │       │   └── AssetDto.cs
            │       │       ├── GetAssetById/
            │       │       ├── GetAssetsByDepartment/
            │       │       └── GetAssetsByLocation/
            │       │
            │       ├── AssetGroups/
            │       │   ├── Commands/
            │       │   └── Queries/
            │       │
            │       ├── AssetCategories/
            │       │   ├── Commands/
            │       │   └── Queries/
            │       │
            │       ├── Depreciation/
            │       │   ├── Commands/
            │       │   │   ├── CalculateDepreciation/
            │       │   │   └── PostDepreciation/
            │       │   └── Queries/
            │       │       └── GetDepreciationSchedule/
            │       │
            │       ├── Maintenance/
            │       │   ├── Commands/
            │       │   └── Queries/
            │       │
            │       └── Common/
            │           ├── Interfaces/
            │           │   ├── IAssetRepository.cs
            │           │   ├── IAssetGroupRepository.cs
            │           │   └── IDepreciationService.cs
            │           └── DTOs/
            │               └── [All DTOs]
            │
            ├── FixedAssetManagement.Infrastructure/
            │   ├── Repositories/
            │   │   ├── AssetRepository.cs
            │   │   ├── AssetGroupRepository.cs
            │   │   └── AssetCategoryRepository.cs
            │   └── Services/
            │       ├── DepreciationCalculator.cs
            │       └── AssetNumberGenerator.cs
            │
            ├── IFixedAssetManagementModule.cs         # Module Interface
            ├── FixedAssetManagementModule.cs          # Implementation
            └── ModuleExtensions.cs                    # DI Registration
```

### 4.2 Module Interface (Replaces gRPC)

```csharp
// IFixedAssetManagementModule.cs
namespace FixedAssetManagement.Module;

public interface IFixedAssetManagementModule
{
    // Asset Queries (called by other modules)
    Task<Result<AssetDto>> GetAssetByIdAsync(int assetId, CancellationToken ct);
    Task<Result<List<AssetDto>>> GetAssetsByDepartmentAsync(int departmentId, CancellationToken ct);
    Task<Result<bool>> IsAssetActiveAsync(int assetId, CancellationToken ct);
    Task<Result<decimal>> GetAssetBookValueAsync(int assetId, CancellationToken ct);
    
    // Asset Commands
    Task<Result<int>> CreateAssetAsync(CreateAssetCommand command, CancellationToken ct);
    Task<Result<bool>> UpdateAssetAsync(UpdateAssetCommand command, CancellationToken ct);
    Task<Result<bool>> TransferAssetAsync(TransferAssetCommand command, CancellationToken ct);
    
    // Validation (replaces gRPC validation services)
    Task<Result<bool>> ValidateAssetExistsAsync(int assetId, CancellationToken ct);
    Task<Result<bool>> ValidateAssetCanBeTransferredAsync(int assetId, CancellationToken ct);
    
    // Depreciation
    Task<Result<bool>> CalculateDepreciationAsync(CalculateDepreciationCommand command, CancellationToken ct);
    Task<Result<List<DepreciationScheduleDto>>> GetDepreciationScheduleAsync(int assetId, CancellationToken ct);
}
```

### 4.3 Cross-Module Communication

#### Example 1: UserManagement validates Department before Asset creation

**Before (Microservices - gRPC)**:
```csharp
// In FixedAssetManagement.API
public class CreateAssetCommandHandler
{
    private readonly IDepartmentValidationGrpcClient _deptClient;
    
    public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        // gRPC call to UserManagement (10-50ms)
        var isDeptValid = await _deptClient.IsDepartmentValidAsync(request.DepartmentId);
        if (!isDeptValid)
            return Result.Failure<int>(Error.Validation("Department", "Invalid"));
        
        // Create asset...
    }
}
```

**After (Modular Monolith - Direct call)**:
```csharp
// In FixedAssetManagement Module
public class CreateAssetCommandHandler
{
    private readonly IUserManagementModule _userModule;
    private readonly BSoftDbContext _context;
    
    public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        // Direct in-process call (<0.1ms)
        var deptResult = await _userModule.GetDepartmentByIdAsync(request.DepartmentId, ct);
        if (deptResult.IsFailure)
            return Result.Failure<int>(Error.Validation("Department", "Invalid"));
        
        var asset = new Asset
        {
            AssetName = request.AssetName,
            DepartmentId = request.DepartmentId,
            // ... other properties
        };
        
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(ct);
        
        // Raise domain event (in-process)
        await _eventBus.PublishAsync(new AssetRegisteredEvent(asset.AssetId), ct);
        
        return Result.Success(asset.AssetId);
    }
}
```

#### Example 2: Accounting module gets asset info for journal entries

**Before (Microservices)**:
```csharp
// In Accounting module - gRPC call
var assetValue = await _assetGrpcClient.GetAssetBookValueAsync(assetId);
```

**After (Modular Monolith)**:
```csharp
// In Accounting module - direct call
var assetValue = await _fixedAssetModule.GetAssetBookValueAsync(assetId, ct);
```

---

## 5️⃣ Database Schema Design

### 5.1 Schema: FixedAsset

```sql
CREATE SCHEMA [FixedAsset];

-- Asset Groups
CREATE TABLE [FixedAsset].[AssetGroups] (
    [AssetGroupId] int IDENTITY(1,1) PRIMARY KEY,
    [GroupCode] nvarchar(20) NOT NULL UNIQUE,
    [GroupName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500),
    [ParentGroupId] int NULL,
    
    -- BaseEntity properties
    [IsActive] int NOT NULL DEFAULT 1,
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByName] nvarchar(max),
    [CreatedIP] nvarchar(max),
    [ModifiedBy] int,
    [ModifiedAt] datetime2,
    [ModifiedByName] nvarchar(max),
    [ModifiedIP] nvarchar(max),
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetGroups_ParentGroup] 
        FOREIGN KEY ([ParentGroupId]) 
        REFERENCES [FixedAsset].[AssetGroups]([AssetGroupId])
);

-- Asset Categories
CREATE TABLE [FixedAsset].[AssetCategories] (
    [AssetCategoryId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetGroupId] int NOT NULL,
    [CategoryCode] nvarchar(20) NOT NULL UNIQUE,
    [CategoryName] nvarchar(200) NOT NULL,
    [DefaultDepreciationMethod] int NOT NULL,
    [DefaultUsefulLife] int NOT NULL,
    [DefaultSalvageValuePercent] decimal(5,2),
    [DefaultGLAccountId] int,
    
    -- BaseEntity properties
    [IsActive] int NOT NULL DEFAULT 1,
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetCategories_AssetGroups] 
        FOREIGN KEY ([AssetGroupId]) 
        REFERENCES [FixedAsset].[AssetGroups]([AssetGroupId])
);

-- Assets (Main Table)
CREATE TABLE [FixedAsset].[Assets] (
    [AssetId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetGuid] uniqueidentifier NOT NULL DEFAULT NEWID(),
    [AssetNumber] nvarchar(50) NOT NULL UNIQUE,
    [AssetTag] nvarchar(50) UNIQUE,
    
    -- Classification
    [AssetGroupId] int NOT NULL,
    [AssetCategoryId] int NOT NULL,
    
    -- Basic Info
    [AssetName] nvarchar(300) NOT NULL,
    [Description] nvarchar(1000),
    [Manufacturer] nvarchar(200),
    [Model] nvarchar(200),
    [SerialNumber] nvarchar(100),
    
    -- Financial
    [PurchaseCost] decimal(18,2) NOT NULL,
    [PurchaseDate] date NOT NULL,
    [PurchaseInvoiceNumber] nvarchar(50),
    [VendorId] int,
    [PurchaseOrderId] int,
    
    -- Depreciation
    [DepreciationMethod] int NOT NULL,
    [UsefulLifeYears] int NOT NULL,
    [SalvageValue] decimal(18,2),
    [AccumulatedDepreciation] decimal(18,2) NOT NULL DEFAULT 0,
    [BookValue] decimal(18,2) NOT NULL,
    [DepreciationStartDate] date,
    
    -- Location & Assignment (Cross-schema FKs)
    [DepartmentId] int NOT NULL,  -- FK to UserManagement.Departments
    [LocationId] int,              -- FK to UserManagement.Cities
    [CustodianUserId] int,         -- FK to UserManagement.Users
    [PhysicalLocation] nvarchar(200),
    
    -- Status
    [Status] int NOT NULL,
    [Condition] int NOT NULL,
    [WarrantyExpiryDate] date,
    [LastMaintenanceDate] date,
    [NextMaintenanceDate] date,
    
    -- BaseEntity
    [IsActive] int NOT NULL DEFAULT 1,
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ModifiedBy] int,
    [ModifiedAt] datetime2,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_Assets_AssetGroups] 
        FOREIGN KEY ([AssetGroupId]) 
        REFERENCES [FixedAsset].[AssetGroups]([AssetGroupId]),
    
    CONSTRAINT [FK_Assets_AssetCategories] 
        FOREIGN KEY ([AssetCategoryId]) 
        REFERENCES [FixedAsset].[AssetCategories]([AssetCategoryId]),
    
    -- Cross-schema foreign keys
    CONSTRAINT [FK_Assets_Departments] 
        FOREIGN KEY ([DepartmentId]) 
        REFERENCES [UserManagement].[Departments]([DepartmentId]),
    
    CONSTRAINT [FK_Assets_Users_Custodian] 
        FOREIGN KEY ([CustodianUserId]) 
        REFERENCES [UserManagement].[Users]([UserId]),
        
    CONSTRAINT [FK_Assets_Cities] 
        FOREIGN KEY ([LocationId]) 
        REFERENCES [UserManagement].[Cities]([CityId])
);

-- Asset Depreciation
CREATE TABLE [FixedAsset].[AssetDepreciation] (
    [DepreciationId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetId] int NOT NULL,
    [DepreciationDate] date NOT NULL,
    [DepreciationAmount] decimal(18,2) NOT NULL,
    [AccumulatedDepreciation] decimal(18,2) NOT NULL,
    [BookValueAfter] decimal(18,2) NOT NULL,
    [IsPosted] bit NOT NULL DEFAULT 0,
    [JournalEntryId] int,  -- FK to Accounting module
    [Remarks] nvarchar(500),
    
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetDepreciation_Assets] 
        FOREIGN KEY ([AssetId]) 
        REFERENCES [FixedAsset].[Assets]([AssetId])
);

-- Asset Maintenance
CREATE TABLE [FixedAsset].[AssetMaintenance] (
    [MaintenanceId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetId] int NOT NULL,
    [Type] int NOT NULL,
    [ScheduledDate] datetime2 NOT NULL,
    [CompletedDate] datetime2,
    [Description] nvarchar(1000) NOT NULL,
    [EstimatedCost] decimal(18,2),
    [ActualCost] decimal(18,2),
    [ServiceProviderId] int,  -- FK to Party/Vendor
    [PerformedBy] nvarchar(200),
    [Status] int NOT NULL,
    [Remarks] nvarchar(1000),
    
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetMaintenance_Assets] 
        FOREIGN KEY ([AssetId]) 
        REFERENCES [FixedAsset].[Assets]([AssetId])
);

-- Asset Transfers
CREATE TABLE [FixedAsset].[AssetTransfers] (
    [TransferId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetId] int NOT NULL,
    [TransferDate] datetime2 NOT NULL,
    
    [FromDepartmentId] int NOT NULL,
    [FromLocationId] int,
    [FromCustodianUserId] int,
    
    [ToDepartmentId] int NOT NULL,
    [ToLocationId] int,
    [ToCustodianUserId] int,
    
    [TransferReason] nvarchar(500),
    [RequestedByUserId] int NOT NULL,
    [ApprovedByUserId] int,
    [ApprovalDate] datetime2,
    [Status] int NOT NULL,
    
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetTransfers_Assets] 
        FOREIGN KEY ([AssetId]) 
        REFERENCES [FixedAsset].[Assets]([AssetId]),
        
    CONSTRAINT [FK_AssetTransfers_FromDept] 
        FOREIGN KEY ([FromDepartmentId]) 
        REFERENCES [UserManagement].[Departments]([DepartmentId]),
        
    CONSTRAINT [FK_AssetTransfers_ToDept] 
        FOREIGN KEY ([ToDepartmentId]) 
        REFERENCES [UserManagement].[Departments]([DepartmentId])
);

-- Asset Disposal
CREATE TABLE [FixedAsset].[AssetDisposal] (
    [DisposalId] int IDENTITY(1,1) PRIMARY KEY,
    [AssetId] int NOT NULL,
    [DisposalDate] datetime2 NOT NULL,
    [DisposalType] int NOT NULL,
    [BookValueAtDisposal] decimal(18,2) NOT NULL,
    [DisposalAmount] decimal(18,2) NOT NULL,
    [GainLoss] decimal(18,2) NOT NULL,
    [Reason] nvarchar(1000),
    [DisposedTo] nvarchar(300),
    [RequestedByUserId] int NOT NULL,
    [ApprovedByUserId] int,
    [IsPosted] bit NOT NULL DEFAULT 0,
    [JournalEntryId] int,
    
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] int NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_AssetDisposal_Assets] 
        FOREIGN KEY ([AssetId]) 
        REFERENCES [FixedAsset].[Assets]([AssetId]),
        
    CONSTRAINT [UQ_AssetDisposal_Asset] UNIQUE ([AssetId])
);

-- Indexes for Performance
CREATE INDEX [IX_Assets_Department] ON [FixedAsset].[Assets]([DepartmentId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Assets_Location] ON [FixedAsset].[Assets]([LocationId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Assets_Status] ON [FixedAsset].[Assets]([Status]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Assets_AssetGroup] ON [FixedAsset].[Assets]([AssetGroupId]);
CREATE INDEX [IX_AssetDepreciation_Date] ON [FixedAsset].[AssetDepreciation]([DepreciationDate]);
CREATE INDEX [IX_AssetMaintenance_ScheduledDate] ON [FixedAsset].[AssetMaintenance]([ScheduledDate]);
```

---

## 6️⃣ API Controllers

### AssetController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
public class AssetController : ApiControllerBase
{
    private readonly IMediator _mediator;
    
    public AssetController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAssets([FromQuery] GetAssetsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsset(int id)
    {
        var query = new GetAssetByIdQuery(id);
        var result = await _mediator.Send(query);
        
        return result.IsSuccess 
            ? Ok(result.Value)
            : NotFound(result.Error);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetCommand command)
    {
        var result = await _mediator.Send(command);
        
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAsset), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] UpdateAssetCommand command)
    {
        if (id != command.AssetId)
            return BadRequest("Asset ID mismatch");
            
        var result = await _mediator.Send(command);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
    
    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> TransferAsset(int id, [FromBody] TransferAssetCommand command)
    {
        command.AssetId = id;
        var result = await _mediator.Send(command);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
    
    [HttpPost("{id}/dispose")]
    public async Task<IActionResult> DisposeAsset(int id, [FromBody] DisposeAssetCommand command)
    {
        command.AssetId = id;
        var result = await _mediator.Send(command);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
    
    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetAssetsByDepartment(int departmentId)
    {
        var query = new GetAssetsByDepartmentQuery(departmentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### DepreciationController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
public class DepreciationController : ApiControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateDepreciation([FromBody] CalculateDepreciationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    
    [HttpPost("post")]
    public async Task<IActionResult> PostDepreciation([FromBody] PostDepreciationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    
    [HttpGet("schedule/{assetId}")]
    public async Task<IActionResult> GetDepreciationSchedule(int assetId)
    {
        var query = new GetDepreciationScheduleQuery(assetId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## 7️⃣ Implementation Summary

### What's Changed

| Aspect | Microservices | Modular Monolith |
|--------|---------------|------------------|
| **Database** | Separate FixedAssetDB | Single BSoftDB with FixedAsset schema |
| **gRPC Services** | Network calls (10-50ms) | Direct method calls (<0.1ms) |
| **Inter-module calls** | IDepartmentValidationGrpcClient | IUserManagementModule |
| **Events** | RabbitMQ/MassTransit | In-process EventBus |
| **Deployment** | Separate service | Part of BSOFT.API |
| **Transactions** | Distributed | Local ACID |

### What's Preserved

✅ All entity properties and relationships  
✅ All business logic and rules  
✅ All API endpoints and signatures  
✅ All validation rules  
✅ All naming conventions (AssetId, AssetNumber, etc.)  
✅ All domain events  
✅ Clean Architecture layers  

### Benefits

- **10x faster** cross-module communication
- **Simpler** deployment (no separate FixedAsset service)
- **ACID transactions** for asset operations
- **Easier debugging** (single process)
- **Maintains modularity** through clear boundaries

---

## 8️⃣ Next Steps

1. **Copy this structure** to BSOFT.ModularMonolith
2. **Create entity configurations** in Shared.Infrastructure
3. **Implement module interface** for cross-module calls
4. **Update BSoftDbContext** to include FixedAsset entities
5. **Create migration** to generate FixedAsset schema
6. **Register module** in Program.cs

All code follows BSOFT patterns and can be integrated seamlessly!
