# Transaction Safety Patterns — BSOFT Standard

**Scope:** All modules in `src/Modules/` that publish cross-service events
**Last Updated:** 2026-03-09

---

## Why This Exists

Handlers that both write to SQL Server AND need to notify another service (via RabbitMQ) face a split-brain risk:

```
❌ UNSAFE — two independent operations:
  await _repo.CreateAsync(entity);       // SQL commit #1
  await _bus.Publish(event);             // if this crashes → event lost forever
```

The **Transactional Outbox Pattern** solves this: the event is written to the same SQL database as the domain data, inside the **same transaction**. `SqlOutboxProcessorJob` (in BSOFT.Worker) polls the outbox table and publishes to RabbitMQ separately — so a crash between commit and publish is safely recovered on the next poll.

```
✅ SAFE — one atomic SQL transaction:
  BEGIN TX
    domain write   → SQL
    outbox insert  → SQL (same tx)
  COMMIT TX
  ← crash here is safe: Worker picks up the outbox row on next poll
```

---

## Pattern Decision Guide

Before writing a handler, identify the scenario and apply the matching pattern:

| Scenario | Use Pattern |
|---|---|
| Create/Update + need to publish a cross-service event (standard case) | **Pattern A — Full UoW** |
| Create entity A, then create entity B that requires A's generated Id as FK, + outbox event | **Pattern B — Repository CommitAsync** |
| Publish notification/event that does NOT need to be atomic with a preceding domain write | **Pattern C — Standalone ScheduleAsync** |
| Write to primary module (EF Core) + update financial/balance field in another module (Dapper) atomically | **Pattern D — Shared Transaction** |
| In-process audit log (MongoDB, MediatR domain event) — no cross-service messaging needed | **No outbox — use `IMediator.Publish` directly** |

---

## Pattern A — Full UoW (Standard Case)

**When:** A single domain write (or multiple writes that don't need each other's generated Id) + one outbox event.

**Used in:**
- `CreatePreventiveSchedulerCommandHandler` — creates header + details + outbox in one TX
- `UpdatePreventiveSchedulerCommandHandler` — updates header + details + outbox in one TX
- `UpdateWorkOrderCommandHandler` — updates WO + conditionally inserts next schedule detail + outbox in one TX

**Required dependencies:**
```csharp
private readonly IMaintenanceUnitOfWork _unitOfWork;
private readonly IOutboxEventPublisher _outboxEventPublisher;
```

**Template:**
```csharp
public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
{
    // ── All reads / lookups BEFORE the transaction (never inside) ─────────
    var lookupData = await _queryRepo.GetSomethingAsync(...);
    if (lookupData == null)
        throw new ExceptionRules("...");

    // Build domain entity(ies) before opening the transaction
    var entity = _mapper.Map<Domain.Entities.YourEntity>(request);

    // ── Atomic transaction block ───────────────────────────────────────────
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    try
    {
        // Write 1 — primary domain entity
        var newId = await _commandRepo.CreateAsync(entity);

        // Write 2 — optional conditional domain write (e.g., next schedule detail)
        if (someCondition)
            await _commandRepo.CreateNextDetailAsync(...);

        // Write 3 — outbox event (no SaveChanges — CommitAsync flushes it atomically)
        var correlationId = Guid.NewGuid();
        var @event = new YourEvent { CorrelationId = correlationId, ... };
        await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);  // SaveChangesAsync + CommitAsync
    }
    catch
    {
        await _unitOfWork.RollbackAsync(cancellationToken);  // Safe if no TX open
        throw;                                               // Never swallow
    }

    // ── Post-commit (ALWAYS outside the transaction) ─────────────────────
    await _mediator.Publish(new AuditLogsDomainEvent(...), cancellationToken);
    await _logService.CaptureLogs(...);

    return ...;
}
```

**Key rules for Pattern A:**
1. `BeginTransactionAsync` must be called before any write inside the try block
2. `ScheduleWithoutSaveAsync` — NOT `ScheduleAsync` — inside the UoW block
3. `CommitAsync` calls `SaveChangesAsync` first then commits: do NOT call `SaveChangesAsync` yourself inside the tx block
4. `catch { RollbackAsync; throw; }` — always rethrow; never swallow exceptions
5. Audit logs, file ops, log service calls — always AFTER the try/catch, never inside

---

## Pattern B — Repository CommitAsync (FK Dependency)

**When:** Entity A must be saved first (to get its generated `Id`) because entity B needs that Id as a FK. Entity B + the outbox event are then tracked without saving and committed together.

**Used in:**
- `CreateMaintenanceRequestCommandHandler` — `MaintenanceRequest` saved first for its Id → `WorkOrder` (FK to MaintenanceRequest.Id) + `OutboxMessage` tracked → one CommitAsync

**Required dependencies:**
```csharp
private readonly IOutboxEventPublisher _outboxEventPublisher;
// No IMaintenanceUnitOfWork — the command repository exposes CommitAsync
```

**Template:**
```csharp
public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
{
    // ── Step 1: Save entity A immediately (need generated Id) ──────────────
    var entityA = _mapper.Map<Domain.Entities.EntityA>(request);
    var entityAId = await _repoA.CreateAsync(entityA); // Calls SaveChangesAsync internally
    if (entityAId <= 0)
        return FailureResponse("Failed to create EntityA.");

    // ── Step 2: Track entity B (requires entityA.Id as FK — no save yet) ──
    var entityB = _mapper.Map<Domain.Entities.EntityB>(request);
    entityB.EntityAId = entityA.Id;                    // Real Id from Step 1
    await _repoB.CreateWithoutSaveAsync(entityB, cancellationToken);

    // ── Step 3: Track outbox event (no save — same transaction as Step 2) ──
    var correlationId = Guid.NewGuid();
    var @event = new YourEvent { CorrelationId = correlationId, EntityAId = entityA.Id, ... };
    await _outboxEventPublisher.ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);

    // ── Commit: one SaveChangesAsync flushes EntityB + OutboxMessage ───────
    await _repoA.CommitAsync(cancellationToken);

    // ── Post-commit: audit log ─────────────────────────────────────────────
    await _mediator.Publish(new AuditLogsDomainEvent(...), cancellationToken);

    return SuccessResponse(entityAId);
}
```

**Key rules for Pattern B:**
1. Use only when you genuinely need entity A's generated `Id` before building entity B — otherwise use Pattern A
2. `CreateWithoutSaveAsync` on entity B's repository + `ScheduleWithoutSaveAsync` on outbox are flushed together in `CommitAsync`
3. Entity A's own `CreateAsync` already committed by the time Step 2 runs — if Steps 2/3 fail, entity A stays in DB but entity B and the outbox do not. Design the domain so a partial state here is acceptable (e.g., a maintenance request without a work order is a valid recoverable state)
4. No `IMaintenanceUnitOfWork` needed — the repository's `CommitAsync` does the flush

---

## Pattern C — Standalone ScheduleAsync

**When:** The outbox event is NOT required to be atomic with a domain write. The domain is already saved. You just want to dispatch an event independently.

**Used for:**
- Events triggered by a read result that doesn't involve a write
- Secondary events where the domain write already committed in a previous step
- Notification-only events where eventual delivery is acceptable

**Template:**
```csharp
// Domain write already committed at this point
var correlationId = Guid.NewGuid();
var @event = new YourEvent { CorrelationId = correlationId, ... };

// ScheduleAsync saves immediately with its own SaveChangesAsync
await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);
```

**Key rules for Pattern C:**
1. ONLY use when the event does not need to share a transaction with any domain write
2. If used inside a UoW try block by mistake, it would call `SaveChangesAsync` mid-transaction — this is a bug; use `ScheduleWithoutSaveAsync` inside UoW blocks
3. For notifications triggered from within an existing Pattern A or B handler, always use `ScheduleWithoutSaveAsync`

---

## Batch Variants

Both `ScheduleWithoutSaveAsync` and `ScheduleAsync` have batch equivalents for publishing multiple events atomically:

```csharp
// Inside UoW block — all events in the same TX
await _outboxEventPublisher.ScheduleBatchWithoutSaveAsync(events, correlationId, cancellationToken);

// Standalone — each event saved immediately (separate SaveChangesAsync per event internally)
await _outboxEventPublisher.ScheduleBatchAsync(events, correlationId, cancellationToken);
```

Use `ScheduleBatchWithoutSaveAsync` when one action produces N events (e.g., N machines in a schedule).

---

## What Goes Inside vs Outside the Transaction

| Operation | Inside TX | Outside TX | Reason |
|---|---|---|---|
| Domain entity write (`CreateAsync`, `UpdateAsync`) | Yes | — | Must be atomic with outbox |
| `ScheduleWithoutSaveAsync` (outbox) | Yes | — | Must be atomic with domain write |
| `ScheduleAsync` (standalone outbox) | Never | Yes | Has its own internal SaveChanges |
| `SaveChangesAsync` directly | Never inside UoW | — | `CommitAsync` calls it — double call corrupts the tracker |
| Read queries / lookups | No (before TX) | — | Reads inside TX hold unnecessary locks |
| `IMediator.Publish(AuditLogsDomainEvent)` | Never | Yes | MongoDB write — failure must not rollback domain |
| File rename / file system operations | Never | Yes | File system is not transactional |
| Log service calls (`CaptureLogs`) | Never | Yes | Informational only; must not rollback domain |
| `throw` on domain validation failure | Yes (before TX open) | — | Throw before `BeginTransactionAsync` if possible |

---

## IOutboxEventPublisher — Method Reference

| Method | Saves? | When to Use |
|---|---|---|
| `ScheduleAsync<T>` | Yes (immediate) | Pattern C — standalone, not inside a UoW |
| `ScheduleWithoutSaveAsync<T>` | No | Pattern A/B — inside UoW or CommitAsync-coordinated block |
| `ScheduleBatchAsync<T>` | Yes per event | Pattern C — multiple events standalone |
| `ScheduleBatchWithoutSaveAsync<T>` | No | Pattern A/B — multiple events in same TX |

---

## OutboxMessage Schema

```
maintenance.OutboxMessages
────────────────────────────────────────────────────────────
Id             bigint IDENTITY — PK
CorrelationId  uniqueidentifier — distributed trace ID
EventType      nvarchar(500)    — assembly-qualified type name (required for deserialization)
EventData      nvarchar(MAX)    — JSON payload (camelCase, not indented)
Status         int              — 0=Pending, 1=Published, 2=Failed
CreatedAt      datetimeoffset   — when the outbox message was created
PublishedAt    datetimeoffset?  — when successfully published to RabbitMQ
RetryCount     int              — number of publish attempts (starts at 0)
MaxRetries     int              — default 5; permanent failure after this
LastError      nvarchar(2000)?  — last error message (truncated at 2000 chars)
NextRetryAt    datetimeoffset?  — exponential backoff: 2^RetryCount seconds
ModuleName     nvarchar(100)    — e.g. "MaintenanceManagement" (for filtering/debug)
CreatedBy      int?             — user Id who triggered the action
```

**Retention:** Published rows older than 7 days are deleted by `SqlOutboxProcessorJob` at the top of each hour.

**Exponential backoff formula:** `NextRetryAt = UtcNow + 2^RetryCount seconds`
- Retry 1: +2s, Retry 2: +4s, Retry 3: +8s, Retry 4: +16s, Retry 5: → Status=Failed

---

## EventType Field — Critical Requirement

`EventType` must be the **full assembly-qualified name** of the event class. `OutboxEventPublisher.CreateOutboxMessage` sets this automatically:

```csharp
EventType = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name
```

`SqlOutboxProcessorJob.ResolveEventType()` uses this to deserialise the JSON back to the correct CLR type. If an event class is moved to a different namespace or assembly, existing outbox rows with the old `EventType` will fail to deserialise — update the `EventType` value in SQL before deploying.

---

## How SqlOutboxProcessorJob Picks Up Events

1. Hangfire fires `SqlOutboxProcessorJob.ProcessAsync` every minute (queue: `sql-outbox-queue`)
2. Queries each registered outbox table: `SELECT TOP 100 WHERE Status=0 AND NextRetryAt <= NOW ORDER BY CreatedAt`
3. Calls `ResolveEventType(message.EventType)` — resolves CLR type from assembly-qualified name
4. Deserialises `EventData` JSON to the resolved type
5. Calls `IPublishEndpoint.Publish(@event, eventType)` → RabbitMQ
6. On success: `UPDATE Status=1, PublishedAt=NOW`
7. On failure: increments `RetryCount`, sets `NextRetryAt`, sets `Status=2` when `RetryCount >= MaxRetries`

**Adding a new module's outbox table** — add to `OutboxTables[]` in `SqlOutboxProcessorJob.cs`:
```csharp
private static readonly (string Schema, string Table)[] OutboxTables =
[
    ("purchase",     "OutboxMessages"),
    ("maintenance",  "OutboxMessages"),
    ("your_module",  "OutboxMessages"),  // ← add here
];
```

---

## Adding Outbox Support to a New Module

Follow these steps in order:

**1. Domain entity** — copy from `MaintenanceManagement.Domain.Entities.Outbox.OutboxMessage` into your module's domain, change `ModuleName` default.

**2. EF Core configuration** — copy `OutboxMessageConfiguration.cs`, update schema name (`your_module`), register in `ApplicationDbContext.OnModelCreating`.

**3. Migration:**
```bash
dotnet ef migrations add OutboxMessages --startup-project ../../../BSOFT.Api
dotnet ef database update --startup-project ../../../BSOFT.Api
```

**4. Interface + repository** — copy `IOutboxRepository` and `OutboxRepository` into your module. Update namespaces.

**5. IOutboxEventPublisher + implementation** — copy `OutboxEventPublisher.cs` into your module's Infrastructure. Update `ModuleName = "YourModule"` in `CreateOutboxMessage`.

**6. IMaintenanceUnitOfWork equivalent** — copy `IMaintenanceUnitOfWork` and `MaintenanceUnitOfWork`, rename to `IYourModuleUnitOfWork` and `YourModuleUnitOfWork`.

**7. DI registrations** in `YourModule.Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<IOutboxRepository, OutboxRepository>();
services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
services.AddScoped<IYourModuleUnitOfWork, YourModuleUnitOfWork>();
```

**8. Register outbox table** in `SqlOutboxProcessorJob.OutboxTables[]`.

---

## Critical Rules

1. **NEVER call `IPublishEndpoint.Publish` or `IBus.Publish` directly from a command handler.**
   Use `IOutboxEventPublisher` always. Direct publish bypasses the outbox — a crash loses the event permanently.

2. **NEVER call `SaveChangesAsync` inside a UoW-managed try block.**
   `CommitAsync` calls it. A second call mid-transaction causes double-tracking issues.

3. **NEVER use `ScheduleAsync` inside a UoW try block.**
   It calls `SaveChangesAsync` internally. Use `ScheduleWithoutSaveAsync` inside UoW.

4. **ALWAYS rethrow after `RollbackAsync`.**
   ```csharp
   catch { await _unitOfWork.RollbackAsync(ct); throw; }  // ✅
   catch { await _unitOfWork.RollbackAsync(ct); return null; }  // ❌ swallowed
   ```

5. **ALWAYS put reads/lookups before `BeginTransactionAsync`.**
   Reads inside a transaction hold read locks unnecessarily. Do all lookups before opening the TX.

6. **ALWAYS put audit logs after `CommitAsync` (post-commit).**
   `AuditLogsDomainEvent` writes to MongoDB. If this fails, it must NOT roll back the SQL domain write.

7. **NEVER place `BeginTransactionAsync` in a validator or pre-handler.**
   Transactions belong in the command handler. Validators are read-only.

8. **`EventType` must be resolvable by `SqlOutboxProcessorJob`.**
   Always use `typeof(YourEvent).AssemblyQualifiedName`. Never store just a short name.

9. **`RollbackAsync` is safe to call when no transaction is open.**
   The implementation checks `if (_transaction is not null)` — no exception thrown.

10. **Batch events in one outbox call, not N separate calls.**
    Use `ScheduleBatchWithoutSaveAsync` for collections — reduces EF tracking overhead.

---

## Pattern D — Shared Transaction (Cross-Module Dapper Write)

**When:** A single user action writes to the primary module's DbContext (EF Core) AND must also update a row in a **different module** via Dapper — and both writes must be atomic (one rollback covers both).

**Currently used in:**
- `CreatePurchaseOrderCommandHandler` — PO header/details (EF Core) + Budget `RemainingBalance` (Budget module Dapper)
- `CreateImportPOCommandHandler` — Import PO (EF Core) + Budget `RemainingBalance` (Dapper)
- `UpdateImportPOCommandHandler` — Import PO update (EF Core) + Budget delta adjustments (Dapper)

**Why not the Outbox Pattern here?**
The Outbox provides *eventual consistency* — Module A commits, Worker polls, Module B updates later. For financial ledger fields like `RemainingBalance`, the number must be correct immediately and simultaneously. Eventual consistency is not acceptable.

**How it works:**
EF Core opens the transaction on its `SqlConnection`. The underlying `SqlConnection` + `SqlTransaction` are extracted and passed explicitly to the Dapper call in the other module. Both writes share the same physical ADO.NET transaction — one `COMMIT` covers both.

**Required interface additions** (on the primary module's Command repository interface):
```csharp
IExecutionStrategy CreateExecutionStrategy();
Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)> BeginTransactionWithConnectionAsync(CancellationToken ct);
Task<int> CreateWithoutTransactionAsync(TAggregate aggregate, TCreateDto dto, CancellationToken ct);
Task<int> UpdateWithoutTransactionAsync(TAggregate incoming, TUpdateDto dto, CancellationToken ct);
```

**Required interface addition** (on the cross-module lookup/service):
```csharp
// Add a shared-transaction overload to the lookup interface in Contracts
Task<bool> ApplyRemainingBalanceDeltaAsync(
    ...,
    DbConnection connection, DbTransaction transaction,
    CancellationToken ct = default);
```

**Implementation in Infrastructure** (`BeginTransactionWithConnectionAsync` — only in Infrastructure where EF Core Relational is available):
```csharp
public async Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)>
    BeginTransactionWithConnectionAsync(CancellationToken ct)
{
    var efTx  = await _db.Database.BeginTransactionAsync(ct);
    var dbTx  = efTx.GetDbTransaction();   // EF Core Relational extension method
    var conn  = dbTx.Connection!;
    return (efTx, conn, dbTx);
}
```

> ⚠️ `GetDbTransaction()` requires `using Microsoft.EntityFrameworkCore;` and the Relational assembly.
> It is ONLY called in Infrastructure — NEVER in Application handlers.
> Application handlers receive `(DbConnection, DbTransaction)` as BCL types via tuple deconstruction.

**Handler template:**
```csharp
public async Task<TResponse> Handle(TCommand request, CancellationToken ct)
{
    // ── All reads/lookups BEFORE the transaction ──────────────────────────
    var existing = await _queryRepo.GetAggregateAsync(request.Id, ct);

    var entity = BuildDomainEntity(request);

    // ── Atomic block with EF Core execution strategy (retry on transient failures) ──
    var strategy = _repo.CreateExecutionStrategy();
    var result = await strategy.ExecuteAsync(async () =>
    {
        var (transaction, dbConn, dbTx) = await _repo.BeginTransactionWithConnectionAsync(ct);
        await using var _ = transaction;   // ensures Dispose (= Rollback) on unhandled exception

        // Write 1 — primary module (EF Core, no internal transaction)
        var newId = await _repo.CreateWithoutTransactionAsync(entity, dto, ct);

        // Write 2 — cross-module Dapper write on the SAME connection + transaction
        if (newId > 0 && entity.BudgetGroupId.HasValue)
        {
            await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                entity.BudgetGroupId.Value, ...,
                dbConn, dbTx, ct);          // ← shared transaction overload
        }

        await transaction.CommitAsync(ct); // one COMMIT covers both writes
        return newId;
    });

    // ── Post-commit: audit log ─────────────────────────────────────────────
    await _mediator.Publish(new AuditLogsDomainEvent(...), ct);
    return SuccessResponse(result);
}
```

**Key rules for Pattern D:**
1. `CreateExecutionStrategy()` wraps the entire block — if SQL Server retries (deadlock/transient), both the EF Core write AND the Dapper write retry together
2. `await using var _ = transaction` — on any unhandled exception, `Dispose()` triggers rollback automatically; no explicit `catch { Rollback; throw }` needed
3. Use `CreateWithoutTransactionAsync` / `UpdateWithoutTransactionAsync` — the "WithoutTransaction" variants do NOT open their own inner transaction (the handler already controls the transaction)
4. The Dapper call in the other module receives `DbConnection` + `DbTransaction` — both are BCL types (`System.Data.Common`), not EF Core types; Application handlers can reference them without adding EF Core Relational dependency
5. `GetDbTransaction()` stays in Infrastructure only — Application handlers never call it
6. Both modules MUST connect to the **same physical SQL Server database** — a single `SqlTransaction` cannot span databases
7. **Do NOT combine with `IMaintenanceUnitOfWork`** — they would conflict on transaction ownership

**When to use Pattern D vs Outbox:**

| Need | Use |
|---|---|
| Financial fields that must be correct immediately (balance, quantity) | **Pattern D** |
| Workflow triggers, notifications, scheduling events | **Pattern A/B/C (Outbox)** |
| Cross-module domain write where eventual consistency is acceptable | **Pattern A/B/C (Outbox)** |
| Two DbContexts that connect to different databases | ❌ Neither — design issue |

---

## Future Pattern — Cross-Module Atomic Write via ICrossModuleUnitOfWork (NOT YET IMPLEMENTED)

> **Do not implement this until a real scenario requires it.**
> All current handlers are covered by Pattern A, B, or C above.

### When You Would Need It

When a single user action must write to **two different module DbContexts atomically** and eventual consistency (via outbox) is not acceptable for that business case.

**Example scenario:**
A stock issue for a Work Order must simultaneously:
- Write to `maintenance.ItemTransactions` (Maintenance DbContext)
- Deduct from `inventory.StockLedger` (Inventory DbContext)

If Maintenance commits but Inventory crashes → stock is wrong. One shared SQL transaction across both contexts is required.

### Why the Outbox Pattern Cannot Help Here

The outbox (Pattern A/B/C) provides **eventual consistency** — Module A commits, Worker polls, Module B's consumer handles the second write later. This is acceptable for scheduling, notifications, and workflow triggers.

It is **not** acceptable when two writes must be simultaneously visible or simultaneously absent — e.g., financial ledger entries, stock deductions, double-entry accounting.

### What Needs to Be Built (When Required)

Three new files + one DI edit:

| File | Location | Purpose |
|---|---|---|
| `ICrossModuleUnitOfWork.cs` | `src/Shared/Contracts/Interfaces/Transaction/` | Interface — `BeginAsync(params DbContext[])`, `CommitAsync`, `RollbackAsync`. In Contracts so any module can inject it |
| `CrossModuleUnitOfWork.cs` | `src/Shared/Shared.Infrastructure/Transaction/` | Opens one raw `SqlConnection`, begins one `SqlTransaction`, calls `UseTransactionAsync` on each DbContext |
| `SharedSqlConnectionFactory.cs` | `src/Shared/Shared.Infrastructure/Transaction/` | Builds connection string from `IConfiguration` + env var substitution (`{SERVER}`, `{USER_ID}`, `{ENC_PASSWORD}`) — same logic as `SqlOutboxProcessorJob` |
| Edit: `Shared.Infrastructure/DependencyInjection.cs` | existing file | `services.AddScoped<ICrossModuleUnitOfWork, CrossModuleUnitOfWork>()` |

### Constraints (must all hold before using this pattern)

1. Both DbContexts must connect to the **same SQL Server database** (`BannariERP`) — a `SqlTransaction` cannot span databases
2. Both DbContexts must be registered as **Scoped** (`AddDbContext<>` default) — Transient gets a new connection and cannot join an existing transaction
3. **Do NOT use `IMaintenanceUnitOfWork` alongside this** — they would conflict on transaction ownership
4. Outbox row goes into **whichever DbContext owns the outbox table** — still participates in the same shared transaction
5. Each DbContext needs its own `SaveChangesAsync` call before `CommitAsync` — each has its own change tracker

### Handler shape (when built)

```csharp
// ── Reads before transaction ──────────────────────────────────────────
var lookup = await _queryRepo.GetAsync(...);

// ── Cross-module atomic block ─────────────────────────────────────────
await using (_crossUoW)
{
    await _crossUoW.BeginAsync(ct, _moduleADb, _moduleBDb);
    try
    {
        _moduleADb.EntityAs.Add(entityA);
        await _moduleADb.SaveChangesAsync(ct);         // inside shared tx

        entityB.SourceId = entityA.Id;
        _moduleBDb.EntityBs.Add(entityB);
        await _moduleBDb.SaveChangesAsync(ct);         // same tx

        await _outbox.ScheduleWithoutSaveAsync(new YourEvent { ... }, correlationId, ct);
        await _moduleADb.SaveChangesAsync(ct);         // flush outbox row

        await _crossUoW.CommitAsync(ct);               // one commit for all three writes
    }
    catch
    {
        await _crossUoW.RollbackAsync(ct);
        throw;
    }
}

// ── Post-commit ───────────────────────────────────────────────────────
await _mediator.Publish(new AuditLogsDomainEvent(...), ct);
```

> When you have a confirmed scenario that requires this pattern, raise it and the three files will be implemented at that time.
