# BackgroundService — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `BackgroundService.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-BGS-01"`.

> ✅ Contracts below were copied verbatim from the just-authored QA suites
> (`BackgroundService.QATests/Tests/{Entity}/{Entity}QATests.cs`, verified against source 2026-06-18).
> ✅/⚠️ ACs are active ordered steps; 🚫 ACs are `[Fact(Skip="needs seeded data: …")]`.

## Module flow (the BackgroundService lifecycle the stories follow)

```
1. Reference masters (the vocabulary notification & approval config references)
   MiscTypeMaster → MiscMaster        (FK miscTypeId → a MiscTypeMaster id)
   NotificationGroup                  (independent code+name master, no FK)
        │
2. Notification setup (the dispatch configuration chain)
   NotificationGroup
   NotificationConfig (notificationEventTypeId = a MiscMaster value)
        │  (deeper: NotificationTemplate → NotificationEventRule — needs more seeds)
        │
3. Approval-workflow engine (the routing/decision layer)
   WorkflowType (moduleId → /api/Modules, menuId → /api/Menu)
   ApprovalStepDetail (WorkflowType + ApprovalStep parents + unit/dept mappings)
   ApprovalRule (ApprovalStepDetail parent + nested condition graph)
```

Key data facts (verified in source during QA authoring):
- **DELETE bindings differ per entity** (copied from QA): `MiscTypeMaster` / `MiscMaster` bind id from the **ROUTE** (`DELETE …/{id}`); `NotificationGroup`, `NotificationConfig`, `WorkflowType`, `ApprovalStepDetail`, `ApprovalRule` bind id from the **QUERY** (`DELETE …?id={id}`).
- Create-response shapes are heterogeneous: `MiscTypeMaster` wraps `ApiResponseDTO<DTO>` (id at `data.Id`); `MiscMaster` echoes a **bare DTO** at `data` (id at `data.Id`); `NotificationGroup` / `NotificationConfig` return a **raw int** at `data`; `WorkflowType` returns a **List<int>** at `data` (capture `data[0]`). `CreatedId()` tolerates number + object; the array case is captured manually.
- Several controllers always return HTTP 200 on create/update/delete (no IsSuccess branch); `MiscTypeMaster` does branch (400 on IsSuccess=false). Happy creates accept `BeOneOf(200, 201)`.
- `MiscMaster.miscTypeId` is a same-module FK → `MiscTypeMaster`; `NotificationConfig.notificationEventTypeId` is a FK → `MiscMaster` (resolved at runtime via `FirstIdAsync`).
- `MiscMaster.GetById` may NRE (500) for a missing id (live bug, reconciled in QA); `MiscTypeMaster.GetById` has a proper 404 guard.
- `MiscTypeMaster` delete has an over-broad SoftDeleteValidation dependent-link guard — a childless run-unique type may still be blocked (400). Teardown tolerates 200/400.
- `ApprovalStepDetail` / `ApprovalRule` create chains need seeded parents (WorkflowType + ApprovalStep + unit/dept mappings; nested condition graph) — their create is blocked; their **reads** (GetAll smoke, by-name, GetById) and **auth protection** are reachable.

---

## US-BGS-01 — Reference master vocabulary  *(IMPLEMENTABLE)*

> As a BackgroundService administrator I build the reference vocabulary (MiscTypeMaster → MiscMaster, plus an independent NotificationGroup) so notification and approval configuration can reference consistent codes.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created and returns a new id (ApiResponseDTO-wrapped) | ✅ |
| 2 | A MiscMaster value can be created under that type (FK miscTypeId; bare-DTO return) | ✅ |
| 3 | A NotificationGroup can be created (independent name master, raw-int return) | ✅ |
| 4 | Each created master is readable by id | ⚠️ verify (MiscType 200/404 guard; MiscMaster may NRE 500; NotificationGroup has no GetById → list read) |
| 5 | Teardown leaf-first — MiscMaster/MiscType delete by ROUTE `/{id}`; NotificationGroup by QUERY `?id=`; dependent-delete probe on MiscType while child exists | ⚠️ verify dependent-delete block (200/400) |

---

## US-BGS-02 — Notification setup chain  *(PARTIAL)*

> As a BackgroundService administrator I set up the notification vocabulary — a group, then a config referencing an event-type misc value — so events can be routed to recipients.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A NotificationGroup can be created (independent, no FK) | ✅ |
| 2 | A NotificationConfig can be created referencing a misc event-type value (FK notificationEventTypeId) | ✅ |
| 3 | The NotificationConfig is readable by id | ⚠️ verify (always-200 controller; 404 on missing) |
| 4 | A full template→event-rule chain (NotificationTemplate + NotificationEventRule) can be configured | 🚫 needs seeded data: template needs notificationConfigId + notification-type misc + language; rule needs channel/recipient/target misc values |
| 5 | Teardown — NotificationConfig then NotificationGroup, both delete by QUERY `?id=` | ✅ |

---

## US-BGS-03 — Approval-workflow readiness  *(MOSTLY BLOCKED — readiness + security)*

> As a platform operator I expect the approval-workflow engine endpoints to be reachable and auth-protected even before any approval configuration is seeded, and a clean WorkflowType master to be creatable.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A WorkflowType can be created (moduleId → /api/Modules, menuId → /api/Menu; List<int> return) | ✅ (self-skips if FK parents unresolved) |
| 2 | The ApprovalStepDetail read surface is reachable | ⚠️ reachability (200/404/500 tolerant) |
| 3 | The ApprovalRule read surface is reachable | ⚠️ reachability (200/404/500 tolerant) |
| 4 | A full approval chain (ApprovalStepDetail + ApprovalRule nested conditions) can be exercised | 🚫 needs seeded data: WorkflowType + ApprovalStep parents + unit/dept mappings + nested condition graph |
| 5 | The approval endpoints reject anonymous callers (401) | ✅ security |
| 6 | Teardown — created WorkflowType deletes by QUERY `?id=` | ✅ |

---

### Implementation status summary

| Story | Implementable now | Blocked on seeded data |
|---|---|---|
| US-BGS-01 Reference master vocabulary | ✅ full | — |
| US-BGS-02 Notification setup chain | ✅ group + config + reads | template→event-rule chain |
| US-BGS-03 Approval-workflow readiness | ✅ WorkflowType create + reachability + security | ApprovalStepDetail→ApprovalRule chain |

US-BGS-01 implements as a fully-active master chain with leaf-first teardown and a dependent-delete
probe. US-BGS-02 implements with the group + config creates active and the deeper template→event-rule
chain `[Fact(Skip="needs seeded data: …")]`. US-BGS-03 implements as a WorkflowType create + read
reachability + anonymous-401 security active, with the ApprovalStepDetail→ApprovalRule create chain
`[Fact(Skip=…)]`, to be un-skipped once a QA user with seeded approval config is available on
`BannariERP_QATest`.
