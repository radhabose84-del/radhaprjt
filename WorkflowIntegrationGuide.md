# BSOFT ERP - Workflow Integration Guide (Developer)

**Audience:** Developers integrating workflow approval into a new module/page
**Purpose:** Step-by-step code guide for adding workflow to any BSOFT module
**Last Updated:** 2026-03-19

---

## 1. Overview

This guide covers the **code changes** required to integrate workflow approval into a new module. For **configuration** (approval levels, rules, conditions), see `WorkflowConfigurationGuide.md`.

### When Is This Guide Needed?

| Scenario | Guide Needed? |
|---|---|
| Add workflow to a **new module** (e.g., Sales) | YES - follow all steps |
| Add workflow to a **new page** in an already-integrated module | Partial - Steps 1, 6, 7, 9, 10 only |
| Add a **new condition field** to an existing workflow | Steps 6 + 11 only |
| Change approval levels/rules/approvers | NO - config only, see `WorkflowConfigurationGuide.md` |

### Architecture Flow

```
Module Handler
  ↓ Publishes CreateApprovalRequestCommand to SQL Outbox
SqlOutboxProcessorJob (BSOFT.Worker)
  ↓ Picks up outbox message, publishes to RabbitMQ
ApprovalRequestConsumer (BackgroundService)
  ↓ Calls sp_EvaluateApproval with Payload JSON
  ↓ Creates ApprovalRequest + ApprovalRequestLine rows
User approves/rejects via UI
  ↓ POST api/ApprovalRequest/approve
ApproveApprovalRequestCommandHandler
  ↓ Calls usp_Approval_UpdateLines
  ↓ Publishes ApprovedRejectedEvent
ApprovalResultDispatcherConsumer
  ↓ Routes to module-specific command queue
Module's ApprovedRejectedConsumer
  ↓ Updates entity status (Approved/Rejected)
```

### Key Components Per Module

| Component | Location | Purpose |
|---|---|---|
| Contracts Command | `Contracts/Commands/{Module}/` | MassTransit command for module-specific routing |
| Contracts Event | `Contracts/Events/{Module}/` | Completion event after status update |
| Domain Entity | `{Module}.Domain/Entities/` | Add `ApprovalStatus` field |
| Outbox Entity | `{Module}.Domain/Entities/Outbox/` | SQL OutboxMessage entity |
| Outbox Interfaces | `{Module}.Application/Common/Interfaces/IOutbox/` | IOutboxRepository + IOutboxEventPublisher |
| Outbox Implementations | `{Module}.Infrastructure/Repositories/Outbox/` + `Services/Outbox/` | SQL-based outbox |
| Consumer | `{Module}.Application/Consumers/` | Handles approval result → updates entity |
| Handler Change | `{Module}.Application/{Entity}/Commands/Create{Entity}/` | Publishes to outbox after create |
| Validator Change | `{Module}.Presentation/Validation/{Entity}/` | Checks workflow is configured |
| Dispatcher Routing | `BackgroundService.Application/Consumer/Workflow/` | Routes approval result to module |
| DI Registration | `{Module}.Infrastructure/DependencyInjection.cs` | Registers outbox services |
| Queue Registration | `BackgroundService.Infrastructure/DependencyInjection.cs` | Registers consumer + queue |

---

## 2. Prerequisites

Before starting, verify:

- [ ] Module and Menu are registered in `AppData.Modules` and `AppData.Menus`
- [ ] You know the `ModuleId` and `MenuId` for your page
- [ ] You have a `ModuleTypeName` string (e.g., `"SalesOrder"`, `"PurchaseIndent"`)
- [ ] The existing workflow infrastructure is working (PurchaseManagement as reference)

### Reference Implementation

**Module:** PurchaseManagement (fully integrated)
**Key Files:**
- Handler: `PurchaseManagement.Application/PurchaseIndent/Commands/CreatePurchaseIndent/CreatePurchaseIndentCommandHandler.cs`
- Consumer: `PurchaseManagement.Application/Consumer/ApprovedRejectedConsumer.cs`
- Outbox: `PurchaseManagement.Application/Common/Interfaces/IOutbox/`
- Contracts: `Contracts/Commands/Purchase/UpdateApprovedRejectedPurchaseCommand.cs`
- Dispatcher: `BackgroundService.Application/Consumer/Workflow/ApprovalResultDispatcherConsumer.cs`

---

## 3. Step-by-Step Implementation

### Step 1: Contracts — Command + Event (2 new files)

Create the MassTransit command that the dispatcher will publish to route approval results to your module.

**File:** `src/Shared/Contracts/Commands/{Module}/UpdateApprovedRejected{Module}Command.cs`

```csharp
using Contracts.Dtos.Common;
using MassTransit;

namespace Contracts.Commands.{Module}
{
    public class UpdateApprovedRejected{Module}Command : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; } = default!;
    }
}
```

**File:** `src/Shared/Contracts/Events/{Module}/ApprovedRejected{Module}CompletedEvent.cs`

```csharp
using MassTransit;

namespace Contracts.Events.{Module};

public record ApprovedRejected{Module}CompletedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int ModuleTransactionId { get; init; }
}
```

> **Note:** If your module needs `PartyContacts` or `DynamicFields` (like Purchase does), add those properties to the command. Only include what your consumer actually uses.

---

### Step 2: Domain Entity — Add ApprovalStatus Field

**MODIFY:** `{Module}.Domain/Entities/{Entity}.cs`

```csharp
public string? ApprovalStatus { get; set; }  // "Pending" | "Approved" | "Rejected"
```

> **Alternative:** Use `int? ApprovalStatusId` as FK to MiscMaster if you prefer the lookup pattern (consistent with `StatusId` in PurchaseIndent). The string approach is simpler.

> **DB MIGRATION REQUIRED** — user runs manually:
> ```bash
> cd src/Modules/{Module}/{Module}.Infrastructure
> dotnet ef migrations add {Entity}ApprovalStatus --startup-project ../../../BSOFT.Api
> dotnet ef database update --startup-project ../../../BSOFT.Api
> ```

---

### Step 3: Outbox Infrastructure (5 new files)

The outbox pattern ensures the approval request is published reliably even if RabbitMQ is temporarily down.

#### 3a. Domain Entity

**NEW:** `{Module}.Domain/Entities/Outbox/OutboxMessage.cs`

```csharp
namespace {Module}.Domain.Entities.Outbox
{
    public class OutboxMessage
    {
        public long Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string EventType { get; set; } = default!;
        public string EventData { get; set; } = default!;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
    }
}
```

#### 3b. Application Interfaces

**NEW:** `{Module}.Application/Common/Interfaces/IOutbox/IOutboxRepository.cs`

```csharp
using {Module}.Domain.Entities.Outbox;

namespace {Module}.Application.Common.Interfaces.IOutbox
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task AddWithoutSaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        Task MarkAsPublishedAsync(long messageId, CancellationToken cancellationToken = default);
        Task MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default);
    }
}
```

**NEW:** `{Module}.Application/Common/Interfaces/IOutbox/IOutboxEventPublisher.cs`

```csharp
namespace {Module}.Application.Common.Interfaces.IOutbox
{
    public interface IOutboxEventPublisher
    {
        Task ScheduleAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
        Task ScheduleWithoutSaveAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
    }
}
```

#### 3c. Infrastructure Implementations

**NEW:** `{Module}.Infrastructure/Repositories/Outbox/OutboxRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using {Module}.Application.Common.Interfaces.IOutbox;
using {Module}.Domain.Entities.Outbox;
using {Module}.Infrastructure.Data;

namespace {Module}.Infrastructure.Repositories.Outbox
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OutboxRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddWithoutSaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default)
        {
            return await _dbContext.OutboxMessages
                .Where(m => m.Status == "Pending" && (m.NextRetryAt == null || m.NextRetryAt <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsPublishedAsync(long messageId, CancellationToken cancellationToken = default)
        {
            var message = await _dbContext.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
            if (message == null) return;
            message.Status = "Published";
            message.ProcessedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default)
        {
            var message = await _dbContext.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
            if (message == null) return;
            message.RetryCount++;
            message.LastError = errorMessage;
            message.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, message.RetryCount));
            if (message.RetryCount >= 5) message.Status = "Failed";
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
```

**NEW:** `{Module}.Infrastructure/Services/Outbox/OutboxEventPublisher.cs`

```csharp
using System.Text.Json;
using {Module}.Application.Common.Interfaces.IOutbox;
using {Module}.Domain.Entities.Outbox;

namespace {Module}.Infrastructure.Services.Outbox
{
    public class OutboxEventPublisher : IOutboxEventPublisher
    {
        private readonly IOutboxRepository _outboxRepository;

        public OutboxEventPublisher(IOutboxRepository outboxRepository)
        {
            _outboxRepository = outboxRepository;
        }

        public async Task ScheduleAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            var message = new OutboxMessage
            {
                CorrelationId = correlationId,
                EventType = typeof(TEvent).AssemblyQualifiedName!,
                EventData = JsonSerializer.Serialize(@event),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _outboxRepository.AddAsync(message, cancellationToken);
        }

        public async Task ScheduleWithoutSaveAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            var message = new OutboxMessage
            {
                CorrelationId = correlationId,
                EventType = typeof(TEvent).AssemblyQualifiedName!,
                EventData = JsonSerializer.Serialize(@event),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _outboxRepository.AddWithoutSaveAsync(message, cancellationToken);
        }
    }
}
```

#### 3d. EF Core Configuration

**MODIFY:** `{Module}.Infrastructure/Data/ApplicationDbContext.cs`

Add DbSet:
```csharp
using {Module}.Domain.Entities.Outbox;

public DbSet<OutboxMessage> OutboxMessages { get; set; }
```

Add in `OnModelCreating`:
```csharp
builder.Entity<OutboxMessage>(entity =>
{
    entity.ToTable("OutboxMessages", "{Schema}");  // e.g., "Sales", "Purchase"
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).ValueGeneratedOnAdd();
    entity.Property(e => e.EventType).HasColumnType("varchar(500)").IsRequired();
    entity.Property(e => e.EventData).HasColumnType("nvarchar(max)").IsRequired();
    entity.Property(e => e.Status).HasColumnType("varchar(20)").HasDefaultValue("Pending");
    entity.Property(e => e.CorrelationId).IsRequired();
    entity.HasIndex(e => new { e.Status, e.NextRetryAt });
});
```

> **DB MIGRATION REQUIRED** for the OutboxMessages table — user runs manually.

---

### Step 4: Command Repository — Add UpdateApprovalStatusAsync

**MODIFY:** `{Module}.Application/Common/Interfaces/I{Entity}/I{Entity}CommandRepository.cs`

```csharp
Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct);
```

**MODIFY:** `{Module}.Infrastructure/Repositories/{Entity}/{Entity}CommandRepository.cs`

```csharp
public async Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct)
{
    var entity = await _dbContext.{Entity}
        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

    if (entity == null) return;

    entity.ApprovalStatus = status;
    _dbContext.{Entity}.Update(entity);
    await _dbContext.SaveChangesAsync(ct);
}
```

---

### Step 5: ApprovedRejectedConsumer — Handle Approval Result

**NEW:** `{Module}.Application/Consumers/ApprovedRejectedConsumer.cs`

```csharp
using Contracts.Commands.{Module};
using Contracts.Events.{Module};
using MassTransit;
using Microsoft.Extensions.Logging;
using {Module}.Application.Common.Interfaces.I{Entity};

namespace {Module}.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejected{Module}Command>
    {
        private readonly I{Entity}CommandRepository _{entity}CommandRepo;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;

        public ApprovedRejectedConsumer(
            I{Entity}CommandRepository {entity}CommandRepo,
            ILogger<ApprovedRejectedConsumer> logger)
        {
            _{entity}CommandRepo = {entity}CommandRepo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejected{Module}Command> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "ApprovedRejected{Module}Consumer: ModuleTypeName={Module}, Status={Status}, TransactionId={Id}",
                msg.ModuleTypeName, msg.Status, msg.ModuleTransactionId);

            try
            {
                // Route by ModuleTypeName if module has multiple pages with workflow
                switch (msg.ModuleTypeName)
                {
                    case "{EntityTypeName}":  // e.g., "SalesOrder"
                        await _{entity}CommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status, context.CancellationToken);
                        break;

                    // Add more cases as more pages in this module get workflow:
                    // case "SalesQuotation":
                    //     await _salesQuotationCommandRepo.UpdateApprovalStatusAsync(...);
                    //     break;

                    default:
                        _logger.LogWarning("Unknown {Module} ModuleTypeName: {Type}", msg.ModuleTypeName);
                        return;
                }

                // Publish completion event
                await context.Publish(new ApprovedRejected{Module}CompletedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    ModuleTransactionId = msg.ModuleTransactionId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ApprovedRejected{Module}Consumer failed for TransactionId={Id}",
                    msg.ModuleTransactionId);
                throw;  // MassTransit retry policy handles retries
            }
        }
    }
}
```

---

### Step 6: Handler — Publish to Outbox After Create

**MODIFY:** `{Module}.Application/{Entity}/Commands/Create{Entity}/Create{Entity}CommandHandler.cs`

Add dependencies:
```csharp
using Contracts.Commands.Workflow;
using {Module}.Application.Common.Interfaces.IOutbox;
using System.Text.Json;

// Add field:
private readonly IOutboxEventPublisher _outboxEventPublisher;

// Add to constructor parameter list:
IOutboxEventPublisher outboxEventPublisher

// Add to constructor body:
_outboxEventPublisher = outboxEventPublisher;
```

Add after entity creation (after `newId` is obtained):
```csharp
// Set initial approval status
entity.ApprovalStatus = "Pending";

// Publish approval request to outbox
var correlationId = Guid.NewGuid();
await _outboxEventPublisher.ScheduleAsync(
    new CreateApprovalRequestCommand
    {
        CorrelationId = correlationId,
        ModuleTypeName = "{EntityTypeName}",  // e.g., "SalesOrder"
        ModuleTransactionId = newId,
        Payload = JsonSerializer.Serialize(new
        {
            Header = new
            {
                // Include ALL fields that approval conditions might reference
                entity.TotalAmount,
                entity.UnitId,
                entity.PartyId,
                // ... add more as needed for ApprovalDataField JsonPath evaluation
            },
            Lines = entity.Details?.Select(d => new
            {
                d.ItemId,
                d.Rate,
                d.Quantity,
                // ... add more line-level fields
            })
        })
    },
    correlationId,
    cancellationToken);
```

> **CRITICAL: The `Payload` JSON structure must match `ApprovalDataField.JsonPath` entries.**
> If a condition references `$.Header.TotalAmount`, the Payload must have `Header.TotalAmount`.
> If a condition references `$.Lines[*].Rate`, the Payload must have `Lines[].Rate`.

### Adding New Condition Fields Later

When an admin needs a new condition field (e.g., "Discount%"):

1. **Developer:** Add the field to the Payload JSON in the handler:
   ```csharp
   Header = new { ..., entity.DiscountPercentage }
   ```

2. **Admin:** Create `ApprovalDataField` row:
   ```sql
   INSERT INTO AppData.ApprovalDataField (FieldKey, JsonPath, ValueTypeId, ScopeId)
   VALUES ('DiscountPercentage', '$.Header.DiscountPercentage', 501, 601);
   ```

3. **Admin:** Add `ApprovalRuleCondition` referencing the new field.

---

### Step 7: Validator — Check Workflow Is Configured

**MODIFY:** `{Module}.Presentation/Validation/{Entity}/Create{Entity}CommandValidator.cs`

Add dependencies:
```csharp
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Interfaces;

// Add fields:
private readonly IWorkflowLookup _workflowLookup;
private readonly IIPAddressService _ipAddressService;

// Add to constructor parameters:
IWorkflowLookup workflowLookup,
IIPAddressService ipAddressService

// Add to constructor body:
_workflowLookup = workflowLookup;
_ipAddressService = ipAddressService;
```

Add **after** the `foreach (var rule in _validationRules)` loop:
```csharp
// Workflow configuration check
RuleFor(x => x)
    .MustAsync(async (command, ct) =>
        await _workflowLookup.IsApproveWorkflowConfigureAsync(
            "{EntityTypeName}",                    // Must match AppData.Menus.MenuName
            _ipAddressService.GetUnitId() ?? 0,
            0))                                    // DepartmentId (0 if not applicable)
    .WithMessage("Workflow approval is not configured for {EntityDisplayName}. Please configure it in Workflow Settings.");
```

> **`IsApproveWorkflowConfigureAsync`** checks:
> 1. A `WorkflowType` exists for this MenuName
> 2. An `ApprovalStepDetail` exists for the user's UnitId
> 3. Returns `true` if workflow is configured, `false` otherwise

---

### Step 8: DI Registration — Outbox Services

**MODIFY:** `{Module}.Infrastructure/DependencyInjection.cs`

Add usings:
```csharp
using {Module}.Application.Common.Interfaces.IOutbox;
using {Module}.Infrastructure.Repositories.Outbox;
using {Module}.Infrastructure.Services.Outbox;
```

Add registrations:
```csharp
// Outbox — SQL-based for transaction atomicity
services.AddScoped<IOutboxRepository, OutboxRepository>();
services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
```

---

### Step 9: Dispatcher Routing — Route Approval Results to Module

**MODIFY:** `BackgroundService.Application/Consumer/Workflow/ApprovalResultDispatcherConsumer.cs`

Add using:
```csharp
using Contracts.Commands.{Module};
```

Add HashSet (after existing type sets):
```csharp
private static readonly HashSet<string> {Module}Types = new(StringComparer.OrdinalIgnoreCase)
{
    "{EntityTypeName}"   // e.g., "SalesOrder"
    // Add more as module gets more pages with workflow:
    // "SalesQuotation"
};
```

Add routing block inside `Consume()` method (before `_inbox.MarkAsProcessedAsync`):
```csharp
if ({Module}Types.Contains(msg.ModuleTypeName))
{
    await context.Publish(new UpdateApprovedRejected{Module}Command
    {
        CorrelationId = msg.CorrelationId,
        ModuleTransactionId = msg.ModuleTransactionId,
        ModuleTypeName = msg.ModuleTypeName,
        Status = msg.Status,
        LineStatus = msg.LineStatus
    });
}
```

---

### Step 10: Queue Registration — Consumer + Endpoint

**MODIFY:** `BackgroundService.Infrastructure/DependencyInjection.cs`

In MassTransit configuration — add consumer:
```csharp
x.AddConsumer<{Module}.Application.Consumers.ApprovedRejectedConsumer>();
```

Add receive endpoint (after existing queue definitions):
```csharp
cfg.ReceiveEndpoint("approved-rejected-{module-lowercase}-task-queue", e =>
{
    e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
    e.ConfigureConsumer<{Module}.Application.Consumers.ApprovedRejectedConsumer>(context);
});
```

> **Queue naming convention:** `approved-rejected-{module-lowercase}-task-queue`
> Examples: `approved-rejected-purchase-task-queue`, `approved-rejected-sales-task-queue`

---

### Step 11: SqlOutboxProcessorJob — Add Schema Polling

**MODIFY:** `BackgroundService.Infrastructure/Jobs/SqlOutboxProcessorJob.cs`

Add the new module's schema to the outbox polling list. The job polls `{Schema}.OutboxMessages` tables.

> Currently polls: `purchase.OutboxMessages`, `maintenance.OutboxMessages`
> Add: `{schema}.OutboxMessages` (e.g., `sales.OutboxMessages`)

---

## 4. Checklist

### Before Development
- [ ] ModuleId and MenuId confirmed in `AppData.Modules` / `AppData.Menus`
- [ ] ModuleTypeName decided (e.g., "SalesOrder")
- [ ] Reference implementation reviewed (PurchaseManagement)

### Code Changes (Developer)
- [ ] Step 1: Contracts — Command + Event created
- [ ] Step 2: Domain entity — `ApprovalStatus` field added
- [ ] Step 3: Outbox infrastructure — 5 files created
- [ ] **DB Migration** — OutboxMessages table + ApprovalStatus column
- [ ] Step 4: Command repository — `UpdateApprovalStatusAsync` added
- [ ] Step 5: ApprovedRejectedConsumer created
- [ ] Step 6: Handler — outbox publish added with Payload JSON
- [ ] Step 7: Validator — workflow config check added
- [ ] Step 8: DI — outbox services registered
- [ ] Step 9: Dispatcher — routing added for module
- [ ] Step 10: Queue — consumer + endpoint registered
- [ ] Step 11: SqlOutboxProcessorJob — schema added
- [ ] Build solution — 0 errors, 0 warnings

### Configuration (Admin — after deployment)
- [ ] Create WorkflowType (ModuleId, MenuId, HasLine)
- [ ] Create ApprovalStepDetail (levels, approvers)
- [ ] Map steps to units (ApprovalStepUnitMapping)
- [ ] Create ApprovalRules with conditions
- [ ] Define ApprovalDataFields (JsonPath matching Payload)
- [ ] Test end-to-end: Create entity -> approval request created -> approve -> status updated

### Testing
- [ ] Create entity -> verify `ApprovalStatus = "Pending"`
- [ ] Check `{Schema}.OutboxMessages` table -> message created with `Status = "Pending"`
- [ ] SqlOutboxProcessorJob processes message -> status changes to `Published`
- [ ] `ApprovalRequest` row created in `AppData.ApprovalRequest`
- [ ] Approve via API -> `ApprovedRejectedEvent` published
- [ ] Dispatcher routes to module consumer
- [ ] Entity `ApprovalStatus` updated to "Approved"
- [ ] Reject scenario: Entity `ApprovalStatus` updated to "Rejected"
- [ ] Validation: Create entity without workflow config -> error message returned

---

## 5. Adding a New Page to an Already-Integrated Module

If your module already has workflow (e.g., PurchaseManagement adding ServicePO), you only need:

1. **Step 1 (partial):** No new Contracts files needed — reuse existing `UpdateApprovedRejected{Module}Command`
2. **Step 2:** Add `ApprovalStatus` to the new entity
3. **Step 4:** Add `UpdateApprovalStatusAsync` to the new entity's repository
4. **Step 5 (partial):** Add a `case` to the existing consumer's `switch (msg.ModuleTypeName)`
5. **Step 6:** Add outbox publish to the new entity's handler
6. **Step 7:** Add workflow check to the new entity's validator
7. **Step 9:** Add the new `ModuleTypeName` to the existing HashSet in `ApprovalResultDispatcherConsumer`

No new outbox infrastructure, no new queue, no new consumer class needed.

---

## 6. Payload JSON Design Guidelines

The Payload JSON is what `sp_EvaluateApproval` evaluates conditions against. Follow these guidelines:

### Structure

```json
{
  "Header": {
    "TotalAmount": 750000.00,
    "UnitId": 101,
    "PartyId": 501,
    "PaymentType": "Credit",
    "OrderDate": "2026-03-19"
  },
  "Lines": [
    {
      "ItemId": 1,
      "Rate": 15000.00,
      "Quantity": 50,
      "Amount": 750000.00
    }
  ]
}
```

### Rules

| Rule | Description |
|---|---|
| Always include `Header` object | Even if only header-level conditions exist |
| Include `Lines` array for HasLine=1 | Each line item as a separate object |
| Use flat property names | `TotalAmount` not `total.amount` |
| Match JsonPath in ApprovalDataField | `$.Header.TotalAmount` must match `Header.TotalAmount` |
| Include all potentially useful fields | Admins may add conditions later without code changes |
| Use numeric types for comparisons | Don't serialize amounts as strings |

### Common Fields to Include

| Field | Type | Scope | Why |
|---|---|---|---|
| TotalAmount | decimal | Header | Amount-based approval thresholds |
| UnitId | int | Header | Unit-specific routing |
| DepartmentId | int | Header | Department-specific routing |
| PartyId | int | Header | Customer/vendor-specific rules |
| Rate | decimal | Line | Per-item rate thresholds |
| Quantity | decimal | Line | Volume-based thresholds |
| Amount | decimal | Line | Line-level amount thresholds |

---

## 7. Troubleshooting

| Problem | Cause | Fix |
|---|---|---|
| Outbox message stuck at "Pending" | SqlOutboxProcessorJob not polling this schema | Add schema to SqlOutboxProcessorJob (Step 11) |
| "Workflow not configured" error | No WorkflowType/StepDetail for this MenuName + UnitId | Admin: configure via `WorkflowConfigurationGuide.md` |
| Consumer not receiving messages | Queue not registered | Check BackgroundService DI (Step 10) |
| Approval result not updating entity | Dispatcher not routing to module | Check `ApprovalResultDispatcherConsumer` HashSet (Step 9) |
| Condition not evaluating | JsonPath doesn't match Payload | Compare `ApprovalDataField.JsonPath` with handler's JSON |
| `IOutboxEventPublisher` not resolved | Not registered in DI | Check module's `DependencyInjection.cs` (Step 8) |
| Build error: missing reference | Consumer project missing Contracts reference | Add `<ProjectReference>` to Contracts in `.csproj` |

---

## 8. File Summary — All Changes for One Module

| # | Action | File |
|---|---|---|
| 1 | NEW | `Contracts/Commands/{Module}/UpdateApprovedRejected{Module}Command.cs` |
| 2 | NEW | `Contracts/Events/{Module}/ApprovedRejected{Module}CompletedEvent.cs` |
| 3 | NEW | `{Module}.Domain/Entities/Outbox/OutboxMessage.cs` |
| 4 | NEW | `{Module}.Application/Common/Interfaces/IOutbox/IOutboxRepository.cs` |
| 5 | NEW | `{Module}.Application/Common/Interfaces/IOutbox/IOutboxEventPublisher.cs` |
| 6 | NEW | `{Module}.Infrastructure/Repositories/Outbox/OutboxRepository.cs` |
| 7 | NEW | `{Module}.Infrastructure/Services/Outbox/OutboxEventPublisher.cs` |
| 8 | NEW | `{Module}.Application/Consumers/ApprovedRejectedConsumer.cs` |
| 9 | MODIFY | `{Module}.Domain/Entities/{Entity}.cs` — add `ApprovalStatus` |
| 10 | MODIFY | `{Module}.Infrastructure/Data/ApplicationDbContext.cs` — add `DbSet<OutboxMessage>` + config |
| 11 | MODIFY | `I{Entity}CommandRepository.cs` — add `UpdateApprovalStatusAsync` |
| 12 | MODIFY | `{Entity}CommandRepository.cs` — implement `UpdateApprovalStatusAsync` |
| 13 | MODIFY | `Create{Entity}CommandHandler.cs` — add outbox publish + set `ApprovalStatus = "Pending"` |
| 14 | MODIFY | `Create{Entity}CommandValidator.cs` — add `IWorkflowLookup` check |
| 15 | MODIFY | `{Module}.Infrastructure/DependencyInjection.cs` — register outbox services |
| 16 | MODIFY | `ApprovalResultDispatcherConsumer.cs` — add module routing |
| 17 | MODIFY | `BackgroundService.Infrastructure/DependencyInjection.cs` — register consumer + queue |
| 18 | MODIFY | `SqlOutboxProcessorJob.cs` — add schema polling |

**Total: 8 new files + 10 modified files**

---

**Version:** 1.0
**Last Updated:** 2026-03-19
**Related:** `WorkflowConfigurationGuide.md` (Admin/Config guide)
