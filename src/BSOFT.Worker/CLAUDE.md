# BSOFT.Worker — Developer Instructions

**Project:** `src/BSOFT.Worker/BSOFT.Worker.csproj`
**SDK:** `Microsoft.NET.Sdk.Worker` (generic host — NOT a web host)
**Role:** Sole background processing process for the BSOFT ERP system.
**Last Updated:** 2026-03-06

---

## What BSOFT.Worker Is

BSOFT.Worker is a **Windows Service** that owns ALL background processing:

| Responsibility | Mechanism |
|---|---|
| Execute all Hangfire jobs | Registered as the Hangfire **server** |
| Process all MassTransit consumers | Registered `AddMassTransit` inside `AddInfrastructureServices()` |
| Poll SQL outbox tables every minute | `SqlOutboxProcessorJob` recurring Hangfire job |
| Push real-time notifications to Next.js clients | `IWorkerNotificationService` → SignalR client → BSOFT.Api hub |

**BSOFT.Api** is the HTTP host — it only has the Hangfire dashboard + job enqueueing. It does NOT run Hangfire jobs or MassTransit consumers.

---

## Project Structure

```
src/BSOFT.Worker/
├── Program.cs                            ← Host bootstrap — DO NOT reorder DI registrations
├── BSOFT.Worker.csproj
├── Configurations/
│   └── HangfireJobsSetup.cs              ← RegisterRecurringJobs() extension on IHost
├── JobProcessors/
│   └── MaintenanceJobProcessor.cs        ← Thin Hangfire wrapper; delegates to IMaintenance
├── Services/
│   ├── IWorkerNotificationService.cs     ← Abstraction for pushing to BSOFT.Api SignalR hub
│   ├── SignalRWorkerNotificationService.cs ← SignalR CLIENT (connects outward to BSOFT.Api)
│   └── WorkerInAppNotifier.cs            ← Overrides IInAppNotifier for Worker context
├── appsettings.json                      ← Base config (placeholder {SERVER} etc.)
├── appsettings.Development.json          ← Dev overrides (real connection strings)
└── appsettings.Production.json           ← Prod overrides (env-var-substituted strings)
```

**BackgroundService layer** (shared, referenced by both BSOFT.Api and BSOFT.Worker):
```
src/Modules/BackgroundService/
├── BackgroundService.Application/
│   ├── Consumer/                         ← All MassTransit consumers
│   │   ├── PreventiveSchedule/           ← Outbox bridge + scheduling consumers
│   │   ├── Workflow/                     ← Approval workflow consumer
│   │   └── Notification*.cs              ← Notification channel consumers
│   └── DependencyInjection.cs            ← AddApplicationServices() — MediatR + AutoMapper + SignalR
└── BackgroundService.Infrastructure/
    ├── DependencyInjection.cs            ← AddInfrastructureServices() — Hangfire storage, MassTransit, repos
    └── Jobs/
        └── SqlOutboxProcessorJob.cs      ← MUST stay here — not in BSOFT.Worker
```

---

## Program.cs — Startup Order (DO NOT CHANGE ORDER)

```csharp
builder.Services.AddHttpContextAccessor();          // 1. Required by IPAddressService
builder.Services.AddMemoryCache();                  // 2. Required by lookup caching
builder.Services.AddApplicationServices();          // 3. MediatR + AutoMapper + SignalR server
builder.Services.AddInfrastructureServices(...);    // 4. Hangfire storage + MassTransit + all repos
                                                    //    Also registers IInAppNotifier = InAppNotifier
builder.Services.AddHangfireServer(...);            // 5. WORKER IS THE SOLE HANGFIRE SERVER
builder.Services.AddSingleton<IWorkerNotificationService, SignalRWorkerNotificationService>();  // 6.
builder.Services.AddScoped<IInAppNotifier, WorkerInAppNotifier>();  // 7. MUST BE LAST — overrides step 4
```

> **Rule:** Step 7 (`WorkerInAppNotifier`) MUST register AFTER `AddInfrastructureServices()`.
> ASP.NET Core DI resolves the last registration for an interface. If this is moved before step 4, `InAppNotifier` wins and notifications silently drop.

---

## Hangfire Server

### Queues (defined in Program.cs)

| Queue Name | Purpose |
|---|---|
| `schedule_work_order_queue` | Preventive scheduler — create / cancel / reschedule WO Hangfire jobs |
| `forgot_password_queue` | Password reset email dispatch |
| `user_unlock_queue` | Automatic user account unlock after lockout period |
| `sql-outbox-queue` | `SqlOutboxProcessorJob` — polls SQL outbox tables every minute |

> **Adding a new queue:** Add the name to the `Queues` array in `Program.cs` AND decorate the job class method with `[Queue("your-new-queue")]`.

### Recurring Jobs (HangfireJobsSetup.cs)

All recurring jobs are registered in `HangfireJobsSetup.RegisterRecurringJobs()` — called on `IHost` after `Build()`, before `Run()`.

**Current recurring jobs:**

| Job ID | Queue | Schedule | Class |
|---|---|---|---|
| `sql-outbox-processor` | `sql-outbox-queue` | Every minute (`Cron.Minutely()`) | `SqlOutboxProcessorJob` |

> **Adding a new recurring job:**
> 1. Create the job class in `BackgroundService.Infrastructure/Jobs/` (NEVER in `BSOFT.Worker/`)
> 2. Register: `services.AddScoped<YourJob>()` in `BackgroundService.Infrastructure/DependencyInjection.cs`
> 3. Add queue name to `Queues[]` in `BSOFT.Worker/Program.cs`
> 4. Add `jobManager.AddOrUpdate<YourJob>(...)` in `HangfireJobsSetup.cs`

### CRITICAL — Job Classes Must Live in BackgroundService.Infrastructure

> **NEVER place Hangfire job classes inside `BSOFT.Worker/`.**
>
> Hangfire stores job type as an assembly-qualified name in SQL Server. When BSOFT.Api's Hangfire dashboard loads, it must be able to resolve the job type from its own loaded assemblies. BSOFT.Api references `BackgroundService.Infrastructure` but NOT `BSOFT.Worker` — so any job class in `BSOFT.Worker` will cause a `TypeLoadException` in the Hangfire dashboard.
>
> **Rule:** Job classes → `BackgroundService.Infrastructure/Jobs/`
> **Rule:** Job wrappers (thin, UI/process-specific) → `BSOFT.Worker/JobProcessors/` (only if absolutely needed)

---

## MassTransit Consumers

All consumers are registered inside `AddInfrastructureServices()` — ONLY when no `IBus` is already registered (guards against double-registration when called from BSOFT.Api):

```csharp
if (!services.Any(d => d.ServiceType == typeof(IBus)))
{
    services.AddMassTransit(x => { ... });
}
```

This means: **BSOFT.Api never runs these consumers** — they exclusively run in BSOFT.Worker.

### Registered Consumers & Queues

| Consumer | Queue | Receives | Publishes / Does |
|---|---|---|---|
| `NotificationDispatcherConsumer` | `notification-dispatcher-queue` | `SendNotificationCommand` | Routes to channel queues |
| `SendEmailNotificationConsumer` | `email-notification-queue` | `SendEmailNotificationCommand` | SMTP send |
| `SendSmsNotificationConsumer` | `sms-notification-queue` | `SendSmsNotificationCommand` | SMS API call |
| `SendInAppNotificationConsumer` | `inapp-notification-queue` | `SendInAppNotificationCommand` | SignalR push |
| `SendWhatsappNotificationConsumer` | `whatsapp-notification-queue` | `SendWhatsappNotificationCommand` | WhatsApp API call |
| `ApprovalRequestConsumer` | `approval-request-task-queue` | `ApprovalRequestCommand` | Creates approval task |
| `ScheduleWorkOrderConsumer` | `hangfire-workorder-schedule-queue` | `HangfireWorkOrderScheduleCommand` | Creates delayed Hangfire WO job |
| `NewScheduleWorkOrderTaskConsumer` | `schedule-workorder-queue` | `SheduleWorkOrderCommand` | Creates delayed Hangfire WO jobs per detail |
| `RollBackScheduleWorkOrderConsumer` | `rollback-ScheduleWorkOrder-queue` | `RollBackScheduleWorkOrderCommand` | Cancels Hangfire WO jobs |
| `ScheduleWorkOrderUpdateConsumer` | `update-scheduleWorkOrder-task-queue` | `UpdateScheduleWorkOrderCommand` | Cancels old + creates new Hangfire WO jobs |
| `MachineWiseScheduleCreationConsumer` | `machine-wise-schedule-creation-queue` | `MachineWiseScheduleCreationEvent` | Bridge → publishes `SheduleWorkOrderCommand` |
| `HeaderUpdateEventConsumer` | `header-update-event-queue` | `HeaderUpdateEvent` | Bridge → publishes `UpdateScheduleWorkOrderCommand` |
| `NextSchedulerCreatedEventConsumer` | `next-scheduler-created-queue` | `NextSchedulerCreatedEvent` | Bridge → publishes `HangfireWorkOrderScheduleCommand` |

### Adding a New Consumer

1. Create consumer class in `BackgroundService.Application/Consumer/` implementing `IConsumer<TMessage>`
2. Add inbox deduplication (see **Inbox Pattern** section below)
3. Register in `BackgroundService.Infrastructure/DependencyInjection.cs`:
   ```csharp
   x.AddConsumer<YourConsumer>();
   // ...inside UsingRabbitMq:
   cfg.ReceiveEndpoint("your-queue-name", e =>
   {
       e.UseMessageRetry(r => r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30)));
       e.ConfigureConsumer<YourConsumer>(context);
   });
   ```
4. Build BackgroundService.Application + BackgroundService.Infrastructure (0 errors)

### Retry Policies by Consumer Type

| Type | Retry Policy | Reason |
|---|---|---|
| Email | 4 retries: 10s, 30s, 2m, 5m | SMTP transient failures |
| SMS / WhatsApp | 3 retries: 10s, 30s, 2m | External API failures |
| InApp | Immediate × 3 | Fast local DB write |
| Scheduling commands | None (default) | Idempotency handled by Hangfire |
| Bridge consumers (outbox events) | None (default) | Inbox dedup prevents double-processing |

---

## SqlOutboxProcessorJob

**Location:** `BackgroundService.Infrastructure/Jobs/SqlOutboxProcessorJob.cs`
**Schedule:** Every minute via Hangfire recurring job (`sql-outbox-processor`)
**Queue:** `sql-outbox-queue`

### What It Does

1. Opens one SQL connection to `DefaultConnection`
2. For each registered outbox table (`purchase.OutboxMessages`, `maintenance.OutboxMessages`):
   - Reads up to 100 `Pending` rows ordered by `CreatedAt` where `NextRetryAt` is due
   - Deserialises `EventData` JSON to the CLR type stored in `EventType` (assembly-qualified name)
   - Publishes the event to RabbitMQ via `IPublishEndpoint`
   - Marks row `Status = Published` on success
   - On failure: increments `RetryCount`, sets exponential `NextRetryAt`, sets `Status = Failed` when `RetryCount >= MaxRetries`
3. At top of each hour: deletes published rows older than 7 days

### Outbox Table Status Values

| Status | Meaning |
|---|---|
| `0` | Pending — not yet published |
| `1` | Published — successfully sent to RabbitMQ |
| `2` | Failed — exceeded `MaxRetries` |

### Adding a New Module's Outbox Table

Add the schema + table name to `OutboxTables` in `SqlOutboxProcessorJob.cs`:
```csharp
private static readonly (string Schema, string Table)[] OutboxTables =
[
    ("purchase",    "OutboxMessages"),
    ("maintenance", "OutboxMessages"),
    ("your_schema", "OutboxMessages"),  // ← add here
];
```

The new module's outbox table must follow the same schema as existing ones (columns: `Id`, `CorrelationId`, `EventType`, `EventData`, `Status`, `RetryCount`, `MaxRetries`, `NextRetryAt`, `CreatedAt`, `PublishedAt`, `LastError`).

---

## Inbox Pattern (Deduplication)

All bridge consumers and any consumer that must be idempotent use `IInboxRepository`:

```csharp
public async Task Consume(ConsumeContext<TMessage> context)
{
    var messageId = context.MessageId ?? Guid.NewGuid();
    const string consumerName = nameof(YourConsumer);

    // 1. Dedup check
    if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
    {
        _logger.LogInformation("Inbox dedup: skipped. Consumer={C}, MessageId={M}", consumerName, messageId);
        return;
    }

    // 2. Business logic
    await context.Publish(new YourCommand { ... });

    // 3. Mark processed
    await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
}
```

**When to use inbox dedup:**
- All outbox bridge consumers (MachineWiseScheduleCreationConsumer, HeaderUpdateEventConsumer, NextSchedulerCreatedEventConsumer)
- Any consumer where processing twice would create duplicate DB records or duplicate Hangfire jobs
- DO NOT use it for notification consumers where retries are intentional

---

## SignalR Architecture

Next.js clients connect to the hub in **BSOFT.Api** (`/notificationHub`), not in BSOFT.Worker. BSOFT.Worker pushes notifications indirectly:

```
WorkerInAppNotifier
  → IWorkerNotificationService.SendAsync("ReceiveNotification", payload)
    → SignalRWorkerNotificationService
      → HubConnection.InvokeAsync("BroadcastFromWorker", method, payload)
        → BSOFT.Api NotificationHub.BroadcastFromWorker()
          → Clients.All.SendAsync(method, payload)
            → Next.js clients
```

### IWorkerNotificationService vs IHubContext

| Use in BSOFT.Worker | Correct Approach |
|---|---|
| Push in-app notification to Next.js | `IWorkerNotificationService.SendAsync()` |
| ~~`IHubContext<NotificationHub>`~~ | NEVER — Next.js clients are not connected to the Worker hub |

`InAppNotifier` (from `BackgroundService.Infrastructure`) uses `IHubContext<NotificationHub>` — it works in BSOFT.Api where Next.js clients connect, but silently drops messages in BSOFT.Worker. That is why `WorkerInAppNotifier` overrides it as the last DI registration.

### SignalR Configuration

`SignalR:HubUrl` is environment-specific. The active file is chosen by the `DOTNET_ENVIRONMENT` variable (read in `Program.cs` line 13).

```json
// appsettings.json  (base / local fallback)
"SignalR": { "HubUrl": "https://localhost:7001/notificationHub" }

// appsettings.Development.json  (DOTNET_ENVIRONMENT=Development)
"SignalR": { "HubUrl": "http://192.168.1.126:81/notificationHub" }

// appsettings.Production.json  (DOTNET_ENVIRONMENT=Production)
"SignalR": { "HubUrl": "https://bsoft.bannarimill.com:81/notificationHub" }
```

The hub URL must always point to **BSOFT.Api's address** — never to the Worker's own process.

---

## Connection String Pattern

`appsettings.json` uses placeholder tokens:
```
"Server={SERVER};Database=BannariERP;User Id={USER_ID};Password={ENC_PASSWORD};..."
```

`AddInfrastructureServices()` substitutes them from environment variables:
```csharp
.Replace("{SERVER}",       Environment.GetEnvironmentVariable("DATABASE_SERVER")   ?? "")
.Replace("{USER_ID}",      Environment.GetEnvironmentVariable("DATABASE_USERID")   ?? "")
.Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "")
```

**In Development:** Set real values directly in `appsettings.Development.json` (no placeholders needed — the file overrides `appsettings.json` entirely for those keys).

**In Production/Staging:** Set OS environment variables `DATABASE_SERVER`, `DATABASE_USERID`, `DATABASE_PASSWORD`. The placeholders in `appsettings.Production.json` will be replaced at runtime.

### Connection Strings Used

| Key | Database | Used By |
|---|---|---|
| `DefaultConnection` | `BannariERP` | `SqlOutboxProcessorJob` (outbox polling) |
| `HangfireConnection` | `Hangfire` | Hangfire storage (jobs, queues, locks) |
| `NotificationConnection` | `BannariERP` | Notification repositories (EF Core) |
| `MongoDbConnectionString` | `WorkFlow` (MongoDB) | Outbox MongoDB context (workflow) |

---

## NuGet Package Rules

### MassTransit Must Be Pinned to 8.0.0

```xml
<!-- BSOFT.Worker.csproj — CORRECT -->
<PackageReference Include="MassTransit.RabbitMQ" Version="8.0.0" />
```

> **NEVER use a wildcard like `Version="8.*"`** — it resolves to a newer 8.x at restore time. The `AddConsumer()` method signature changed in MassTransit 8.1+, causing `MissingMethodException` at startup because `BackgroundService.Infrastructure` was compiled against 8.0.0.
>
> Both `BSOFT.Worker.csproj` and `BackgroundService.Infrastructure.csproj` must pin `MassTransit.RabbitMQ` to exactly `8.0.0`.

### Serilog Versions (aligned with BackgroundService transitive)

```xml
<PackageReference Include="Serilog.Extensions.Hosting" Version="9.*" />
<PackageReference Include="Serilog.Sinks.File"         Version="6.*" />
<PackageReference Include="Serilog.Sinks.Console"      Version="6.*" />
```

### AspNetCore Framework Reference (Required)

```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

Required because `BackgroundService.Application` calls `AddSignalR()` and uses `IHubContext<>`, `IHttpContextAccessor`. Without this reference, those types are not available in the generic Worker host.

---

## What BSOFT.Worker CANNOT Have

| Forbidden | Reason |
|---|---|
| `app.UseRouting()` / `app.MapControllers()` | No HTTP pipeline — generic host only |
| `IWebHostEnvironment` | Only `IHostEnvironment` is available (generic host) |
| `WebRootPath` | No web root in a Worker — use `ContentRootPath` for file paths |
| `[ApiController]` / `ControllerBase` | No MVC — no HTTP request handling |
| `app.UseSwagger()` | No Swagger — no HTTP pipeline |
| JWT middleware | No HTTP pipeline — authentication is BSOFT.Api's concern |
| `AddHangfireServer()` in a business module's DI | Only BSOFT.Worker registers the Hangfire server |
| Hangfire job classes | Must live in `BackgroundService.Infrastructure/Jobs/` |
| Direct `IBus.Publish()` from a handler (API side) | Handlers use `IOutboxEventPublisher` — never direct MassTransit |

---

## HttpClient Configuration

`BackgroundService.Infrastructure` registers no named HttpClients — cross-module communication uses in-process MediatR or RabbitMQ events instead. Module-specific outbound HttpClients (e.g. `IFileFetcher` typed client, `NicEInvoice`, `IGSTAuthService`, `ISsrsClient`, `IFrankfurterClient`) are registered in their own module's `Infrastructure/DependencyInjection.cs` and wrapped with `AddBsoftHttpResilience` (Polly v8 — see `Shared.Infrastructure/Resilience/`).

> **History:** `UserManagementClient` and `MaintenanceClient` were both removed during the modular-monolith consolidation. `UserUnlockService` now dispatches `UnlockUserCommand` via in-process MediatR; `MaintenanceClient` had no consumers and was dead code from the pre-monolith era.

---

## MongoDB

Used for:
1. **Workflow outbox** (`WorkFlow` database, `OutboxMessages` collection) — approval workflow events
2. **Audit logs** — written by `AuditLogsDomainEvent` handlers in business modules

```json
"MongoDb": {
  "DatabaseName": "WorkFlow",
  "OutboxCollectionName": "OutboxMessages"
}
```

> The SQL outbox (`purchase.OutboxMessages`, `maintenance.OutboxMessages`) is **separate** from the MongoDB outbox. The MongoDB outbox is for workflow/approval events; the SQL outbox is for domain events from MaintenanceManagement and PurchaseManagement modules.

---

## Run & Deploy

### Development

```bash
dotnet run --project src/BSOFT.Worker --environment Development
```

Logs written to `logs/bsoft-worker-YYYYMMDD.log` (rolling daily, 30 days retained) and console.

### Windows Service (Production)

```bash
# Publish
dotnet publish src/BSOFT.Worker -c Release -o C:\Services\BSoftWorker

# Register service
sc create "BSoftWorker" binPath="C:\Services\BSoftWorker\BSOFT.Worker.exe" start=auto DisplayName="BSOFT Worker Service"

# Start
sc start BSoftWorker

# Stop
sc stop BSoftWorker

# Remove
sc delete BSoftWorker
```

Service name: `BSOFT Worker Service` (matches `options.ServiceName` in `Program.cs`).

### Hangfire Dashboard (BSOFT.Api)

The dashboard is in BSOFT.Api — not in BSOFT.Worker.

- Dev: `http://localhost:5239/hangfire`
- Server: `http://192.168.1.126:81/hangfire`
- Auth: Basic auth (`HangfireServer:DashboardUser` / `HangfireServer:DashboardPassword` in BSOFT.Api appsettings)

---

## Build Checklist (Before Modifying Worker)

1. `dotnet build src/Modules/BackgroundService/BackgroundService.Application/BackgroundService.Application.csproj`
2. `dotnet build src/Modules/BackgroundService/BackgroundService.Infrastructure/BackgroundService.Infrastructure.csproj`
3. `dotnet build src/BSOFT.Worker/BSOFT.Worker.csproj`
4. Expected: `0 Warning(s), 0 Error(s)` on all three

---

## Common Issues & Fixes

| Issue | Cause | Fix |
|---|---|---|
| `MissingMethodException: AddConsumer` at startup | MassTransit version mismatch (Worker != Infrastructure) | Pin both to `8.0.0` exactly |
| Notifications silently dropped in Worker | Using `InAppNotifier` (IHubContext) instead of `WorkerInAppNotifier` | Ensure `WorkerInAppNotifier` is registered LAST in Program.cs |
| `TypeLoadException` in Hangfire dashboard | Job class placed in `BSOFT.Worker` assembly | Move job class to `BackgroundService.Infrastructure/Jobs/` |
| `IWebHostEnvironment` not found | BSOFT.Worker uses generic host, not web host | Change to `IHostEnvironment`; use `ContentRootPath` not `WebRootPath` |
| Outbox messages stuck in Pending | `SqlOutboxProcessorJob` not polling the new table | Add schema/table to `OutboxTables[]` in `SqlOutboxProcessorJob.cs` |
| Duplicate Hangfire jobs created | Bridge consumer processed same event twice | Add `IInboxRepository` inbox dedup to the consumer |
| Consumer not receiving messages | Consumer registered in `AddMassTransit` but missing `ReceiveEndpoint` | Add `cfg.ReceiveEndpoint(...)` block in `DependencyInjection.cs` |
