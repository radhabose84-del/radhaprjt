# BSOFT ERP — Notification & Consumer Architecture

**Module:** MaintenanceManagement (primary), BackgroundService (consumers)
**Pattern:** Transactional Outbox + Inbox Dedup + MassTransit/RabbitMQ
**Date:** 2026-04-16

---

## 1. End-to-End Flow Overview

```
User Action (e.g., Create MaintenanceRequest)
    |
    v
CreateMaintenanceRequestCommandHandler
    |
    |-- Step 1: Save MaintenanceRequest (immediate — Id needed as FK)
    |-- Step 2: Save WorkOrder (immediate — Id needed for param1)
    |-- Step 3: Build NotificationCreatedEvent
    |-- Step 4: ScheduleWithoutSaveAsync(event) -- track OutboxMessage (no save yet)
    |-- Step 5: CommitAsync() -- atomic commit (WorkOrder + OutboxMessage)
    |-- Step 6: PublishDirectAsync(event) -- attempt direct RabbitMQ send
    |
    v
    +--- Immediate path: RabbitMQ --> NotificationDispatcherConsumer
    |
    +--- Fallback path (every 1 min):
         SqlOutboxProcessorJob polls OutboxMessages WHERE Status=Pending
         --> Publish to RabbitMQ --> Update Status=Published
```

---

## 2. Notification Event Structure

### NotificationCreatedEvent

**File:** `src/Shared/Contracts/Events/Notifications/NotificationCreatedEvent.cs`

| Property | Type | Purpose |
|---|---|---|
| `CorrelationId` | `Guid` | Distributed trace ID for end-to-end tracking |
| `CreatedByName` | `string` | User who triggered the action |
| `UnitId` | `int` | Business unit context |
| `EventTypeId` | `int` | Event type from MiscMaster (Create, Update, Approve) |
| `ModuleName` | `string` | Source module (e.g., `"WorkOrder"`, `"WorkOrder-BreakDown"`) |
| `Email` | `string` | Primary email recipient |
| `ccMail` | `string` | CC field (can hold department ID for resolution) |
| `Mobile` | `string` | Primary mobile number |
| `param1` | `string` | WorkOrder ID |
| `param2` | `string` | Machine Name or Production Dept (context-dependent) |
| `param3` | `DateTimeOffset` | Creation date |
| `param4` | `string` | Machine Name or Production Dept (inverse of param2) |
| `param5` | `string` | Maintenance Department Name |
| `param6` | `string` | Remarks |
| `param7` | `string` | Created By (breakdown only) |
| `param8`–`param10` | `string` | Reserved for additional template vars |
| `TablePresetKey` | `string?` | SQL preset key for HTML table rendering |
| `TableRowsJson` | `string?` | JSON rows for table rendering |
| `Attachments` | `List<NotificationAttachment>` | File attachments (blob URL, filename, content type) |

### Parameter Mapping (Maintenance Module)

| Scenario | param1 | param2 | param4 | param5 | param6 | param7 |
|---|---|---|---|---|---|---|
| **WorkOrder (normal)** | WO ID | Machine Name | Production Dept | Maintenance Dept | Remarks | *(empty)* |
| **WorkOrder-BreakDown** | WO ID | Production Dept | Machine Name | Maintenance Dept | Remarks | Created By |

**ccMail for BreakDown:** Set to `departmentId.ToString()` for recipient resolution by the notification template engine.

---

## 3. Outbox Pattern (Durable Event Publishing)

### OutboxMessage Entity

**File (per module):** `src/Modules/{Module}/Core/{Module}.Domain/Entities/Outbox/OutboxMessage.cs`

| Column | Type | Purpose |
|---|---|---|
| `Id` | `long` | PK, Identity |
| `CorrelationId` | `Guid` | Matches event CorrelationId |
| `EventType` | `string` | Assembly-qualified CLR type name |
| `EventData` | `string` | JSON-serialized NotificationCreatedEvent |
| `Status` | `enum` | `Pending=0`, `Published=1`, `Failed=2` |
| `CreatedAt` | `DateTimeOffset` | When the outbox record was created |
| `PublishedAt` | `DateTimeOffset?` | When it was published to RabbitMQ |
| `RetryCount` | `int` | Incremented on each failure |
| `MaxRetries` | `int` | Default: 5 |
| `LastError` | `string?` | Truncated exception message |
| `NextRetryAt` | `DateTimeOffset?` | Exponential backoff timestamp |
| `ModuleName` | `string` | Module name for tracing |
| `CreatedBy` | `int?` | User ID |
| `ProcessorHint` | `string?` | `"maintenance"` for MaintenanceOutboxProcessorJob |

### IOutboxEventPublisher Interface

**File:** `src/Modules/{Module}/Core/{Module}.Application/Common/Interfaces/IOutbox/IOutboxEventPublisher.cs`

| Method | Behavior |
|---|---|
| `ScheduleAsync(event, correlationId)` | Create OutboxMessage + save immediately |
| `ScheduleWithoutSaveAsync(event, correlationId)` | Track OutboxMessage, defer SaveChangesAsync |
| `PublishDirectAsync(event, correlationId)` | Send directly to RabbitMQ (after transaction commit) |

### Dual Publishing Strategy

```
                      +---> PublishDirectAsync() ---> RabbitMQ (immediate)
                      |     (best effort, may fail)
COMMIT TRANSACTION ---+
                      |
                      +---> OutboxMessage persisted (durable safety net)
                            |
                            +---> SqlOutboxProcessorJob (every 1 min)
                                  polls Pending records, publishes to RabbitMQ
```

**Why dual?**
- Direct publish gives near-instant notifications
- Outbox ensures no events are lost if RabbitMQ is temporarily unavailable
- Inbox dedup prevents duplicate processing when both paths succeed

---

## 4. Outbox Background Processors

### 4.1 SqlOutboxProcessorJob (General — All Modules)

**File:** `src/Modules/BackgroundService/BackgroundService.Infrastructure/Jobs/SqlOutboxProcessorJob.cs`
**Schedule:** Every 1 minute (Hangfire)
**Queue:** `sql-outbox-queue`

**Processed Tables:**

| Schema | Table | Notes |
|---|---|---|
| `purchase` | `OutboxMessages` | |
| `maintenance` | `OutboxMessages` | Skips `ProcessorHint='maintenance'` |
| `Budget` | `OutboxMessages` | |
| `Inventory` | `OutboxMessages` | |
| `Party` | `OutboxMessages` | |
| `Project` | `OutboxMessages` | |
| `Sales` | `OutboxMessages` | |

**Processing Logic:**

```
FOR EACH schema.OutboxMessages:
  1. SELECT TOP 100 WHERE Status=0 (Pending)
     AND (NextRetryAt IS NULL OR NextRetryAt <= GETUTCDATE())

  2. FOR EACH message:
     a. Resolve CLR type from EventType (assembly-qualified name)
     b. Deserialize EventData JSON to typed object
     c. _publishEndpoint.Publish(event) --> RabbitMQ
     d. UPDATE Status=1 (Published), PublishedAt=NOW

     ON ERROR:
     a. RetryCount++
     b. NextRetryAt = NOW + 2^RetryCount seconds (exponential backoff)
     c. IF RetryCount >= MaxRetries: Status=2 (Failed)
     d. LastError = exception.Message (truncated)

  3. HOURLY CLEANUP: DELETE WHERE Status=1 AND PublishedAt < 7 days ago
```

### 4.2 MaintenanceOutboxProcessorJob (Maintenance-Specific)

**File:** `src/Modules/MaintenanceManagement/MaintenanceManagement.Infrastructure/Jobs/MaintenanceOutboxProcessorJob.cs`
**Schedule:** Every 1 minute (queue: `maintenance-jobs`)
**Filter:** Only processes `WHERE ProcessorHint = 'maintenance'`

**Handles preventive maintenance scheduling events (not notification dispatch):**

| Event Type | Action |
|---|---|
| `MachineWiseScheduleCreationEvent` | Schedule `ScheduleWorkOrderJob` via Hangfire |
| `HeaderUpdateEvent` | Delete old Hangfire job, schedule new one |
| `NextSchedulerCreatedEvent` | Schedule next preventive maintenance cycle |

---

## 5. Consumer Pipeline (MassTransit)

### 5.1 NotificationDispatcherConsumer (Orchestrator)

**File:** `src/Modules/BackgroundService/BackgroundService.Application/Consumer/NotificationDispatcherConsumer.cs`
**Queue:** Listens for `NotificationCreatedEvent`

**Flow:**

```
Consume(NotificationCreatedEvent)
  |
  1. Inbox dedup check:
     if IsAlreadyProcessedAsync(consumerName, messageId) --> skip
  |
  2. Resolve active channels:
     channels = ResolveNotificationChannelsAsync(unitId, module, eventTypeId, email, ccMail, mobile)
     --> SQL stored procedure: usp_GetNotificationDetails
     --> Returns: ["EMAIL", "SMS", "WHATSAPP", "INAPP"] (based on configuration)
  |
  3. Dispatch to channel-specific consumers:
     +-- "EMAIL"    --> publish SendEmailNotificationInternalCommand
     +-- "SMS"      --> publish SendSmsNotificationInternalCommand
     +-- "WHATSAPP" --> publish SendWhatsappNotificationInternalCommand
     +-- "INAPP"    --> publish SendInAppNotificationInternalCommand
  |
  4. MarkAsProcessedAsync(consumerName, messageId, correlationId)
```

### 5.2 SendEmailNotificationConsumer

**File:** `src/Modules/BackgroundService/BackgroundService.Application/Consumer/SendEmailNotificationConsumer.cs`
**Input:** `SendEmailNotificationInternalCommand`

```
1. Inbox dedup check

2. Resolve MiscMaster IDs:
   - Channel = "Email" from MiscMaster
   - Status = "Success"/"Failed"
   - ReadStatus = "Unread"

3. Fetch template + build email:
   a. ResolveNotificationTemplatesAsync(unitId, module, eventTypeId, ...)
      --> Returns: To, Cc, Bcc, Subject, Header, Body, Footer, Lang, IsTable
   b. Build token map: {Module, param1, param2, ..., param10}
   c. Replace tokens in HTML template: {{param1}} --> actual value

4. Handle table rendering (if IsTable=true):
   if (param10 has JSON data):
     param4Html = RenderFromTemplateAsync(templateId, param10_json)

5. Send email:
   success = _emailSender.SendEmailAsync(to, subject, header, body, footer, cc, bcc, attachments)

6. Log result:
   _loggerNotification.LogAsync(NotificationEventLog) --> MongoDB/SQL

7. On failure:
   publish SendEmailNotificationFailed
   throw exception --> MassTransit retries (exponential backoff)
   Failed messages --> email-notification-queue_error (dead letter)
```

### 5.3 SendWhatsappNotificationConsumer

**File:** `src/Modules/BackgroundService/BackgroundService.Application/Consumer/SendWhatsappNotificationConsumer.cs`
**Input:** `SendWhatsappNotificationInternalCommand`

```
1. Inbox dedup check

2. Resolve templates (reuse SMS mobile numbers):
   waNumbers = result.Sms (mobile list)

3. HTML --> Plain Text conversion:
   - Remove <br>, </p> tags --> newlines
   - Strip all HTML tags
   - Decode HTML entities
   - Normalize whitespace
   - Bold first non-empty line: *text*

4. Get ApiToken from SQL (not appsettings):
   apiKey = result.ApiToken ?? fallback

5. Send via IWhatsAppSender:
   _waSender.SendWhatsAppAsync(mobileNumber, resolvedBody, apiKey)

6. Log result + mark inbox
```

### 5.4 SendSmsNotificationConsumer

**Input:** `SendSmsNotificationInternalCommand`
**Pattern:** Resolve template, extract mobile numbers, replace tokens, send via `ISMSSender`, log result.

### 5.5 SendInAppNotificationConsumer

**Input:** `SendInAppNotificationInternalCommand`
**Pattern:** Resolve InApp user IDs from SQL, publish to SignalR `NotificationHub`, log result.

---

## 6. Inbox Dedup Mechanism

### InboxMessage Entity

**File:** `src/Modules/BackgroundService/BackgroundService.Infrastructure/Persistence/InboxMessage.cs`

| Column | Type | Purpose |
|---|---|---|
| `Id` | `long` | PK |
| `ConsumerName` | `string` | e.g., `"SendEmailNotificationConsumer"` |
| `MessageId` | `Guid` | MassTransit MessageId (unique per logical message) |
| `CorrelationId` | `Guid?` | Business CorrelationId for tracing |
| `ProcessedAt` | `DateTimeOffset` | When the message was processed |

**Unique Index:** `(ConsumerName, MessageId)` — enforced at DB level.

### Dedup Flow

```
BEFORE processing:
  if (IsAlreadyProcessedAsync(consumerName, messageId) == true)
    --> skip (already handled, return silently)

AFTER successful processing:
  MarkAsProcessedAsync(consumerName, messageId, correlationId)
    --> INSERT INTO InboxMessages
    --> ON DUPLICATE KEY: log warning, ignore (concurrent race)
```

**Race Condition Handling:**
```csharp
try {
    _context.InboxMessages.Add(entry);
    await _context.SaveChangesAsync();
} catch (DbUpdateException ex) when (ex.InnerException contains "duplicate key") {
    _logger.LogWarning("Concurrent insert detected -- safely ignored");
}
```

---

## 7. Notification Template Resolution

### NotificationResolverHandler

**File:** `src/Modules/BackgroundService/BackgroundService.Application/Notification/NotificationResolverHandler.cs`

#### ResolveNotificationChannelsAsync()

- Calls SQL stored procedure: `usp_GetNotificationDetails`
- Input: `UnitId`, `ModuleName`, `EventTypeId`, `Email`, `ccMail`, `Mobile`
- Returns: List of active channel names (`["Email", "SMS", "InApp", "WhatsApp"]`)

#### ResolveNotificationTemplatesAsync()

- Returns detailed tuple:

| Field | Type | Purpose |
|---|---|---|
| `To` | `List<string>` | Email TO recipients |
| `Cc` | `List<string>` | Email CC recipients |
| `Bcc` | `List<string>` | Email BCC recipients |
| `Sms` | `List<string>` | SMS mobile numbers (also used for WhatsApp) |
| `InApp` | `List<int>` | User IDs for in-app notifications |
| `Subject` | `string` | Email subject template |
| `Header` | `string` | Email header HTML |
| `Body` | `string` | Email body HTML with `{{param1}}` tokens |
| `Footer` | `string` | Email footer HTML |
| `Lang` | `string` | Template language |
| `EventTypeId` | `int` | Event type for logging |
| `EventRuleId` | `int` | Event rule for logging |
| `ChannelId` | `int` | Channel for logging |
| `TemplateId` | `int` | Template ID for table rendering |
| `IsTable` | `bool` | Whether to render HTML table from JSON |
| `ApiToken` | `string` | SMS/WhatsApp API key from SQL |

### Token Replacement

```csharp
var tokenMap = new Dictionary<string, string>
{
    { "{{Module}}", event.ModuleName },
    { "{{param1}}", event.param1 },
    { "{{param2}}", event.param2 },
    { "{{param3}}", event.param3.ToString("dd-MMM-yyyy HH:mm") },
    { "{{param4}}", event.param4 },
    // ... param5 through param10
};

foreach (var token in tokenMap)
{
    subject = subject.Replace(token.Key, token.Value);
    body = body.Replace(token.Key, token.Value);
    footer = footer.Replace(token.Key, token.Value);
}
```

---

## 8. Internal Command Classes (Channel Dispatch)

All extend `CorrelatedBy<Guid>` for MassTransit correlation tracking.

### SendEmailNotificationInternalCommand

**File:** `src/Shared/Contracts/Events/Notifications/Email/SendEmailNotificationInternalCommand.cs`

```csharp
public class SendEmailNotificationInternalCommand : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public int UnitId { get; set; }
    public string ModuleName { get; set; }
    public int EventTypeId { get; set; }
    public string Email { get; set; }
    public string ccMail { get; set; }
    public string Mobile { get; set; }
    public string param1 through param10 { get; set; }
    public int RetryCount { get; set; }
    public List<NotificationAttachment>? Attachments { get; set; }
}
```

### SendSmsNotificationInternalCommand

**File:** `src/Shared/Contracts/Events/Notifications/Sms/SendSmsNotificationInternalCommand.cs`

### SendWhatsappNotificationInternalCommand

**File:** `src/Shared/Contracts/Events/Notifications/Whatsapp/SendWhatsappNotificationInternalCommand.cs`

### SendInAppNotificationInternalCommand

**File:** `src/Shared/Contracts/Events/Notifications/InApp/SendInAppNotificationInternalCommand.cs`

---

## 9. Notification Constants

**File:** `src/Shared/Contracts/Events/Notifications/NotificationEnum.cs`

```csharp
public static class NotificationEnum
{
    // MiscType codes (lookup codes in MiscMaster)
    public const string NotificationChannel = "NotificationChannel";
    public const string NotificationStatus  = "NotificationStatus";
    public const string NotificationEvent   = "EventType";
    public const string NotificationReadStatus = "ReadStatus";

    // Channel names
    public const string Email    = "Email";
    public const string SMS      = "SMS";
    public const string InApp    = "InApp";
    public const string WhatsApp = "WhatsApp";

    // Status values
    public const string Pending  = "Pending";
    public const string Sent     = "Sent";
    public const string Failed   = "Failed";
    public const string Success  = "Success";
    public const string Unread   = "UnRead";

    // Event types
    public const string Create   = "Create";
    public const string Update   = "Update";
    public const string Approve  = "Approve";
}
```

---

## 10. Error Handling & Retry Strategy

### Outbox Retry (SqlOutboxProcessorJob)

| Attempt | NextRetryAt Delay | Status |
|---|---|---|
| 1st failure | +2 seconds | Pending (retry) |
| 2nd failure | +4 seconds | Pending (retry) |
| 3rd failure | +8 seconds | Pending (retry) |
| 4th failure | +16 seconds | Pending (retry) |
| 5th failure | +32 seconds | **Failed** (manual intervention) |

### MassTransit Consumer Retry

- Configured per queue endpoint in `Program.cs`
- Exponential backoff (5 retries, starting at 5 seconds)
- Failed messages move to `{queue}_error` dead-letter queue

### Cleanup

- Published outbox messages older than 7 days are deleted hourly by `SqlOutboxProcessorJob`

---

## 11. Complete Architecture Diagram

```
+===========================================================================+
|                        USER ACTION (HTTP Request)                          |
+====================================+======================================+
                                     |
                                     v
+------------------------------------+--------------------------------------+
|           CreateMaintenanceRequestCommandHandler                           |
|                                                                            |
|   BEGIN TRANSACTION                                                        |
|     1. CreateAsync(MaintenanceRequest) --> saved, Id generated             |
|     2. CreateAsync(WorkOrder)          --> saved, Id generated             |
|     3. ScheduleWithoutSaveAsync(NotificationCreatedEvent) --> tracked      |
|   COMMIT (CommitAsync) --> atomic: WorkOrder + OutboxMessage               |
|                                                                            |
|   PublishDirectAsync(event) --> RabbitMQ (best effort)                     |
|   Return 200 OK immediately                                               |
+------------------------------------+--------------------------------------+
                                     |
              +----------------------+----------------------+
              |                                             |
              v                                             v
+-------------+----------------+          +-----------------+----------------+
| Direct RabbitMQ Publish      |          | SqlOutboxProcessorJob            |
| (immediate, may fail)        |          | (every 1 min, Hangfire)          |
|                              |          |                                  |
| On success: consumer gets it |          | Polls OutboxMessages             |
| On failure: outbox fallback  |          | WHERE Status=Pending             |
+-------------+----------------+          | Publishes to RabbitMQ            |
              |                           | Marks Published or retries       |
              +----------+----------------+-----------------+
                         |
                         v
+------------------------+-------------------------------------------+
|         NotificationDispatcherConsumer (MassTransit)                |
|                                                                    |
|   1. Inbox dedup check (skip if already processed)                 |
|   2. ResolveNotificationChannelsAsync()                            |
|      --> SQL: usp_GetNotificationDetails                           |
|      --> Returns active channels for (UnitId, Module, EventType)   |
|   3. Dispatch to channel consumers:                                |
+----+----------+-----------+----------+-----------------------------+
     |          |           |          |
     v          v           v          v
+--------+ +--------+ +----------+ +--------+
| Email  | | SMS    | | WhatsApp | | InApp  |
| Queue  | | Queue  | | Queue    | | Queue  |
+---+----+ +---+----+ +----+-----+ +---+----+
    |          |            |           |
    v          v            v           v
+---+----+ +---+----+ +----+-----+ +---+----+
|Send    | |Send    | |Send      | |Send    |
|Email   | |SMS     | |WhatsApp  | |InApp   |
|Consumer| |Consumer| |Consumer  | |Consumer|
|        | |        | |          | |        |
|Resolve | |Resolve | |Reuse SMS | |Resolve |
|template| |template| |mobiles   | |user IDs|
|Replace | |Replace | |HTML->Text| |Push to |
|tokens  | |tokens  | |Get API   | |SignalR |
|Send via| |Send via| |token     | |Hub     |
|SMTP    | |SMS API | |Send via  | |        |
|        | |        | |WA API    | |        |
|Log     | |Log     | |Log       | |Log     |
+--------+ +--------+ +----------+ +--------+
```

---

## 12. How to Create a Notification for a New Module

### Step 1 — Define Outbox Infrastructure in Your Module

1. Create `OutboxMessage` entity in `{Module}.Domain/Entities/Outbox/`
2. Create `OutboxMessages` table configuration in EF Core
3. Create `IOutboxEventPublisher` interface in `{Module}.Application/Common/Interfaces/IOutbox/`
4. Create `OutboxEventPublisher` implementation in `{Module}.Infrastructure/Services/Outbox/`
5. Register in `DependencyInjection.cs`

### Step 2 — Build NotificationCreatedEvent in Command Handler

```csharp
var notificationEvent = new NotificationCreatedEvent
{
    CorrelationId = correlationId,
    CreatedByName = _ipAddressService.GetUserName(),
    UnitId = _ipAddressService.GetUnitId() ?? 0,
    ModuleName = "YourModule",               // Must match SQL notification config
    EventTypeId = eventTypeId,               // From MiscMaster (Create/Update/Approve)
    ccMail = "",                             // Optional: department ID for CC resolution
    param1 = entity.Id.ToString(),           // Template parameter 1
    param2 = entity.Name,                    // Template parameter 2
    param3 = DateTimeOffset.UtcNow,          // Creation date
    param4 = "",                             // Additional params as needed
    // ... param5-param10
};
```

### Step 3 — Schedule + Commit + Direct Publish

```csharp
// Track outbox message (no save yet)
await _outboxEventPublisher.ScheduleWithoutSaveAsync(notificationEvent, correlationId, ct);

// Atomic commit (entity + outbox message)
await _commandRepository.CommitAsync(ct);

// Attempt direct publish for immediate delivery
await _outboxEventPublisher.PublishDirectAsync(notificationEvent, correlationId, ct);
```

### Step 4 — Register Outbox Table in SqlOutboxProcessorJob

Add your module's schema to the `OutboxTables` array in `SqlOutboxProcessorJob.cs`:

```csharp
private static readonly (string Schema, string Table)[] OutboxTables =
[
    // ... existing entries
    ("YourSchema", "OutboxMessages"),
];
```

### Step 5 — Configure Notification Templates in SQL

1. Add notification channel config in SQL for `(UnitId, ModuleName, EventTypeId)`
2. Create HTML templates with `{{param1}}`, `{{param2}}`, etc. tokens
3. Map channels (Email, SMS, WhatsApp, InApp) to the event

---

## 13. Key File Reference

| Component | Path |
|---|---|
| **NotificationCreatedEvent** | `src/Shared/Contracts/Events/Notifications/NotificationCreatedEvent.cs` |
| **NotificationEnum** | `src/Shared/Contracts/Events/Notifications/NotificationEnum.cs` |
| **Email Command** | `src/Shared/Contracts/Events/Notifications/Email/SendEmailNotificationInternalCommand.cs` |
| **SMS Command** | `src/Shared/Contracts/Events/Notifications/Sms/SendSmsNotificationInternalCommand.cs` |
| **WhatsApp Command** | `src/Shared/Contracts/Events/Notifications/Whatsapp/SendWhatsappNotificationInternalCommand.cs` |
| **InApp Command** | `src/Shared/Contracts/Events/Notifications/InApp/SendInAppNotificationInternalCommand.cs` |
| **OutboxMessage (Maintenance)** | `src/Modules/MaintenanceManagement/Core/MaintenanceManagement.Domain/Entities/Outbox/OutboxMessage.cs` |
| **IOutboxEventPublisher** | `src/Modules/{Module}/Core/{Module}.Application/Common/Interfaces/IOutbox/IOutboxEventPublisher.cs` |
| **OutboxEventPublisher** | `src/Modules/{Module}/{Module}.Infrastructure/Services/Outbox/OutboxEventPublisher.cs` |
| **SqlOutboxProcessorJob** | `src/Modules/BackgroundService/BackgroundService.Infrastructure/Jobs/SqlOutboxProcessorJob.cs` |
| **MaintenanceOutboxProcessorJob** | `src/Modules/MaintenanceManagement/MaintenanceManagement.Infrastructure/Jobs/MaintenanceOutboxProcessorJob.cs` |
| **NotificationDispatcherConsumer** | `src/Modules/BackgroundService/BackgroundService.Application/Consumer/NotificationDispatcherConsumer.cs` |
| **SendEmailNotificationConsumer** | `src/Modules/BackgroundService/BackgroundService.Application/Consumer/SendEmailNotificationConsumer.cs` |
| **SendWhatsappNotificationConsumer** | `src/Modules/BackgroundService/BackgroundService.Application/Consumer/SendWhatsappNotificationConsumer.cs` |
| **NotificationResolverHandler** | `src/Modules/BackgroundService/BackgroundService.Application/Notification/NotificationResolverHandler.cs` |
| **InboxMessage** | `src/Modules/BackgroundService/BackgroundService.Infrastructure/Persistence/InboxMessage.cs` |
| **InboxRepository** | `src/Modules/BackgroundService/BackgroundService.Infrastructure/Repositories/Inbox/InboxRepository.cs` |
| **Handler (reference)** | `src/Modules/MaintenanceManagement/Core/MaintenanceManagement.Application/MaintenanceRequest/Command/CreateMaintenanceRequest/CreateMaintenanceRequestCommandHandler.cs` |

---

## 14. Design Patterns Used

| Pattern | Purpose |
|---|---|
| **Transactional Outbox** | Atomicity guarantee — event + entity in same transaction |
| **Inbox Dedup** | Idempotent message processing — prevents duplicates on redelivery |
| **Dual Publishing** | Direct publish for speed + outbox for durability |
| **Saga/Orchestrator** | NotificationDispatcherConsumer routes to channel consumers |
| **Exponential Backoff** | Graceful retry on transient failures |
| **Polling Processor** | Hangfire jobs as failsafe outbox publishers |
| **Template Engine** | SQL-based HTML templates with token replacement |
| **Dead Letter Queue** | Failed messages after max retries go to `{queue}_error` |
| **Strategy + DI Override** | InApp notifier swapped at runtime — Worker overrides default implementation |

---

## 15. InApp Notification — Worker Relay Architecture

### Why InApp Is Different From Email/SMS/WhatsApp

Email, SMS, and WhatsApp consumers call **external APIs** (SMTP, SMS gateway, WhatsApp API) — reachable from any process. InApp notifications must push **real-time updates to Angular clients** via SignalR WebSocket, which introduces a **process boundary problem**.

```
BSOFT.Api process     <-- Angular clients connect HERE (SignalR /notificationHub)
BSOFT.Worker process  <-- MassTransit consumers run HERE
```

`IHubContext<NotificationHub>` is **process-local**. If the Worker used it directly, notifications would silently fail — no Angular clients are connected to the Worker's hub.

### The Solution: DI Override + SignalR Client Relay

```
SendInAppNotificationConsumer (BackgroundService.Application)
    | injects
IInAppNotifier (interface -- consumer doesn't know WHERE it runs)
    | resolved by DI at runtime to:
    |
    +-- BSOFT.Api:    InAppNotifier         (DB save only, no SignalR push)
    +-- BSOFT.Worker: WorkerInAppNotifier   (DB save + SignalR client relay)
```

**DI Override in `BSOFT.Worker/Program.cs`:**
```csharp
// BackgroundService.Infrastructure registers default:
services.AddScoped<IInAppNotifier, InAppNotifier>();       // DB-only

// Worker overrides (last registration wins):
builder.Services.AddScoped<IInAppNotifier, WorkerInAppNotifier>();  // DB + SignalR relay
```

### Worker-to-Api SignalR Relay Flow

```
BSOFT.Worker (MassTransit consumer process)
  |
  v
WorkerInAppNotifier
  |  1. Save NotificationEventLog to DB (source of truth)
  |  2. Push via IWorkerNotificationService (SignalR CLIENT)
  v
SignalRWorkerNotificationService
  |  _connection = new HubConnectionBuilder()
  |      .WithUrl("http://localhost:5050/notificationHub")
  |      .Build();
  |
  |  await _connection.InvokeAsync("BroadcastFromWorker", method, payload);
  v
BSOFT.Api / NotificationHub (SignalR SERVER)
  |
  |  await Clients.Group("user-{userId}").SendAsync("ReceiveNotification", payload);
  v
Angular Client (browser WebSocket)
```

### Per-User Error Isolation

`WorkerInAppNotifier` processes each user independently:

```csharp
foreach (var userId in userIds)
{
    // Step 1: Save to DB (source of truth) -- if fails, skip this user, continue others
    try { savedLogId = await _notificationLogger.LogAsync(log); }
    catch { continue; }

    // Step 2: Push via SignalR (best-effort) -- if fails, DB log exists for reconnect fetch
    try { await _workerNotificationService.SendAsync("ReceiveNotification", payload); }
    catch { _logger.LogWarning(...); }  // One user's failure does NOT block others
}
```

**Guarantees:**
- DB log is **always** saved first (user can fetch on reconnect)
- SignalR push is **best-effort** per user
- One user's failure does **not** block other users

### Why Not Alternative Approaches?

| Alternative | Problem |
|---|---|
| Host SignalR hub in Worker | Angular clients would need to connect to 2 hosts — fragile, CORS issues |
| Use `IHubContext` directly in Worker | Process-local — silently drops messages (no clients connected) |
| Redis backplane for SignalR | Adds infrastructure complexity for a problem that doesn't need it |
| Skip MassTransit, call SignalR from Api | Loses retry, inbox dedup, outbox durability |

### Channel Retry Comparison

| Channel | Retry Policy | Reason |
|---|---|---|
| Email | Exponential backoff, 5 retries | External SMTP may be temporarily unavailable |
| SMS | Exponential backoff, 5 retries | External API rate limits |
| WhatsApp | Exponential backoff, 5 retries | External API rate limits |
| InApp | Immediate x3 | Local DB + local SignalR relay — fast, minimal retry sufficient |

### Key Files

| Component | Path |
|---|---|
| **IInAppNotifier (interface)** | `src/Modules/BackgroundService/BackgroundService.Application/Interfaces/Notification/IInAppNotifier.cs` |
| **InAppNotifier (default, DB-only)** | `src/Modules/BackgroundService/BackgroundService.Infrastructure/` |
| **WorkerInAppNotifier (Worker relay)** | `src/BSOFT.Worker/Services/WorkerInAppNotifier.cs` |
| **IWorkerNotificationService** | `src/BSOFT.Worker/Services/IWorkerNotificationService.cs` |
| **SignalRWorkerNotificationService** | `src/BSOFT.Worker/Services/SignalRWorkerNotificationService.cs` |
| **NotificationHub (Api)** | `src/BSOFT.Api/Hubs/NotificationHub.cs` |
| **SendInAppNotificationConsumer** | `src/Modules/BackgroundService/BackgroundService.Application/Consumer/SendInAppNotificationConsumer.cs` |
| **DI Override (Worker Program.cs)** | `src/BSOFT.Worker/Program.cs` |

---

## 16. Code Review Fixes Applied (2026-04-17)

A comprehensive deep review of the notification pipeline identified 35 issues across Critical, High, and Medium severity levels. All fixes were implemented:

### 16.1 Critical Fixes

| # | File | Issue | Fix |
|---|---|---|---|
| 1 | `NotificationDispatcherConsumer.cs` | Missing inbox mark on no-channels early exit caused infinite retries | Added `_inbox.MarkAsProcessedAsync()` before return |
| 2 | `NotificationDispatcherConsumer.cs` | `Task.WhenAll` dispatched all channels in parallel — one failure threw `AggregateException` and lost the others | Replaced with sequential per-channel `try-catch` loop with `dispatched`/`failed` counters |
| 3 | `NotificationDispatcherConsumer.cs` | `channels.Distinct()` was case-sensitive — "Email" and "email" both dispatched | Changed to `channels.Distinct(StringComparer.OrdinalIgnoreCase)` |
| 4 | `SqlOutboxProcessorJob.cs` | Single shared `SqlConnection` for all 7 outbox tables — one table's timeout/broken pipe killed all subsequent tables | Fresh `SqlConnection` per table with per-table `try-catch` |

### 16.2 High Priority Fixes — Inbox Mark on Early Exits

All channel consumers had early-exit paths (no recipients, empty body, etc.) that returned WITHOUT marking the message as processed. This caused MassTransit to redeliver the message endlessly.

| # | Consumer | Early Exit Path | Fix |
|---|---|---|---|
| 5 | `SendEmailNotificationConsumer` | No recipients found (line ~141) | Added `_inbox.MarkAsProcessedAsync()` |
| 6 | `SendInAppNotificationConsumer` | No user IDs resolved | Added `_inbox.MarkAsProcessedAsync()` |
| 7 | `SendWhatsappNotificationConsumer` | No WhatsApp numbers + empty body | Added `_inbox.MarkAsProcessedAsync()` on both paths |
| 8 | `SendSmsNotificationConsumer` | No mobile numbers + empty body | Added `_inbox.MarkAsProcessedAsync()` on both paths |

### 16.3 Medium Priority Fixes

| # | File | Issue | Fix |
|---|---|---|---|
| 9 | `SendEmailNotificationConsumer` | `_htmlTableRenderer.RenderFromTemplateAsync` failure was unhandled — crashed the entire email consumer | Wrapped in `try-catch` with fallback to empty `param4Html` |
| 10 | `SendEmailNotificationConsumer` | `Regex.Replace()` in `NormalizeTemplate` recompiled regex on every call (3 patterns x every email) | Pre-compiled to `static readonly Regex` fields with `RegexOptions.Compiled` |
| 11 | `WorkerInAppNotifier.cs` | Returned `bool` with no visibility into partial failures (e.g., 3/5 DB saves, 2/5 SignalR pushes) | Added conditional `LogWarning` for partial failures with `DB_Failed`/`SignalR_Failed` counters |
| 12 | `WorkerInAppNotifier.cs` | Had `#nullable disable` | Removed — nullable-safe now |
| 13 | `SignalRWorkerNotificationService.cs` | Had `#nullable disable` | Removed — nullable-safe now |
| 14 | `SignalRWorkerNotificationService.cs` | No `Reconnecting` event handler for observability | Added `_connection.Reconnecting` handler with `LogWarning` |
| 15 | All 4 channel consumers | Hardcoded `"dd-MMM-yyyy"` date format string duplicated across all consumers | Extracted to `NotificationEnum.DateFormat` constant; all consumers now use `msg.param3.ToString(NotificationEnum.DateFormat)` |

### 16.4 Files Modified

```
src/Shared/Contracts/Events/Notifications/NotificationEnum.cs                      + DateFormat constant
src/Modules/BackgroundService/BackgroundService.Application/Consumer/
    NotificationDispatcherConsumer.cs                                               inbox mark + per-channel try-catch + case-insensitive Distinct
    SendEmailNotificationConsumer.cs                                                inbox mark + table renderer try-catch + regex caching + DateFormat
    SendInAppNotificationConsumer.CS                                                inbox mark + DateFormat
    SendWhatsappNotificationConsumer.cs                                             inbox mark (2 paths) + DateFormat
    SendSmsNotificationConsumer.CS                                                  inbox mark (2 paths) + DateFormat
src/Modules/BackgroundService/BackgroundService.Infrastructure/Jobs/
    SqlOutboxProcessorJob.cs                                                        fresh connection per table + per-table error isolation
src/BSOFT.Worker/Services/
    WorkerInAppNotifier.cs                                                          removed #nullable disable + partial failure logging
    SignalRWorkerNotificationService.cs                                             removed #nullable disable + Reconnecting handler
```
