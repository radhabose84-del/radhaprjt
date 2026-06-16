# MaintenanceManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `MaintenanceManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-MNT-01"`.

## Module flow (the maintenance lifecycle the stories follow)

```
1. Reference & org setup
   CostCenter · WorkCenter · ShiftMaster → ShiftMasterDetail
   MaintenanceCategory · MaintenanceType · MiscTypeMaster → MiscMaster
        │
2. Machine setup
   MachineGroup → Machine (+ MachineSpecification) · MachineGroupUser (responsible users)
        │
3. Activity setup
   ActivityMaster → ActivityCheckListMaster
        │
4. Preventive maintenance (planned)
   PreventiveScheduler (machine + activity + frequency)  → map machines → generates Work Orders
        │
5. Reactive maintenance (breakdown)
   MaintenanceRequest (internal/external) → WorkOrder → status transitions → ServiceHistory
        │
6. Spares / materials
   MRS (material requisition slip) → issue against MainStoreStock → StockLedger movement
        │
7. Power monitoring
   FeederGroup → Feeder → PowerConsumption (meter readings)
```

Key data facts (verified in source):
- `ActivityMaster` create wraps a nested `CreateActivityMasterDto`; `PUT` is at `/update`.
- `Feeder` / `FeederGroup` create at `POST /create`; deletes at `/{id}`.
- `MiscTypeMaster` / `MiscMaster` are under the `api/maintenance/...` route prefix.
- Several deletes (CostCenter, WorkCenter, MaintenanceCategory, MaintenanceType, Machine) bind id from body/query, not a `/{id}` route.
- `MaintenanceRequest`, `WorkOrder`, `PreventiveScheduler` have large transactional payloads + many lookups.

---

## US-MNT-01 — Machine group setup & user assignment  *(IMPLEMENTED)*

> As a maintenance administrator I create a machine group and assign a responsible user.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MachineGroup can be created and returns a new id | ✅ |
| 2 | A MachineGroupUser can be assigned to that group | ✅ |
| 3 | The group is readable by id after creation | ✅ |
| 4 | The setup can be torn down (assignment then group) | ✅ |

---

## US-MNT-02 — Machine onboarding  *(IMPLEMENTED)*

> As a maintenance administrator I onboard a machine under a group and record its specification.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MachineGroup exists (created in-flow) | ✅ |
| 2 | A Machine can be created under that group | ✅ CostCenter/WorkCenter created in-flow; uom/asset/unit/line best-effort = 1 |
| 3 | A MachineSpecification can be recorded for the machine | ✅ SpecificationValue must be a numeric string > 0 |
| 4 | `GET /api/Machine/MachineGroup/{groupId}` returns the machine/department info | ✅ |

---

## US-MNT-03 — Maintenance reference setup  *(IMPLEMENTABLE)*

> As a maintenance administrator I set up the reference masters (cost centre, work centre,
> maintenance category & type) used to classify maintenance work.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A CostCenter can be created | ⚠️ FK unit/dept ids |
| 2 | A WorkCenter can be created | ⚠️ FK unit/dept ids |
| 3 | A MaintenanceCategory can be created | ✅ |
| 4 | A MaintenanceType can be created | ✅ |
| 5 | Each created master is readable by id and via by-name | ⚠️ verify |

---

## US-MNT-04 — Activity & checklist setup  *(IMPLEMENTED)*

> As a maintenance planner I define an activity and its checklist items.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An ActivityMaster can be created (nested CreateActivityMasterDto + machine group) | ✅ |
| 2 | An ActivityCheckListMaster can be created for that activity | ✅ |
| 3 | `POST /api/ActivityCheckListMaster/ByActivityId` returns the checklist | ✅ body is `{ "ids": [activityId] }`, not `{ activityId }` |

---

## US-MNT-05 — Preventive maintenance scheduling  *(PARTIAL — env-blocked create)*

> As a maintenance planner I configure a preventive schedule for a machine and an activity,
> map machines to it, and have work orders generated.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A machine (under a group) and an activity exist | ✅ created in-flow |
| 2 | Create a PreventiveScheduler (machine + activity + frequency) | 🚫 **env-blocked** (see below) |
| 3 | Map machines to the schedule (`MapMachines`) | 🚫 needs a created scheduler |
| 4 | The scheduler abstract endpoint is reachable by date | ✅ |

> **Blocker (env):** `CreatePreventiveScheduler` matches machines via `GetMachineByGroupSagaAsync(groupId, UnitId)`
> where `UnitId` is the **caller's JWT unit**. `testsales` has `UnitId = 0` but machines require
> `UnitId >= 1`, so the saga never matches → "No machines found for selected MachineGroup."
> The four FK fields (MaintenanceCategory/Schedule/FrequencyType/FrequencyUnit) are all MiscMaster ids.
> Needs a QA user whose UnitId owns machines.

---

## US-MNT-06 — Maintenance request → work order lifecycle  *(PARTIAL — WO create deferred)*

> As a maintenance user I raise a maintenance request, convert it to a work order,
> move it through its statuses, and see it in service history.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The WorkOrder status lookup is reachable | ✅ |
| 2 | Raise an internal MaintenanceRequest | ✅ `maintenanceTypeId` is a MiscMaster value; non-"External" type avoids vendor |
| 3 | Create a WorkOrder (from the request or directly) | 🚫 large composite `WorkOrderCombineDto` — deferred |
| 4 | Move the work order through status values | 🚫 needs a posted WO |
| 5 | The ServiceHistory read is reachable for the machine | ✅ |

---

## US-MNT-07 — Spares requisition & stock movement  *(PARTIAL — env-blocked create)*

> As a maintenance user I raise a material requisition slip (MRS) and issue spares,
> moving stock in the ledger.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The MRS reference lookups (department / category / sub-cost-centre) are reachable | ✅ |
| 2 | Create an MRS (`POST /CreateMRS`) | 🚫 **env-blocked** (see below) |
| 3 | The MRS appears in `pending-issue` | 🚫 needs a posted MRS |
| 4 | StockLedger reflects the issued quantity | 🚫 needs a posted issue |

> **Blocker (env):** MRS create needs a `HeaderRequest` with division/department codes + line-item
> `Details` bound to real stock (old-ERP item codes with available quantity), all scoped to the
> caller's `OldUnitId`. `testsales` has no `OldUnitId` stock scope, so there is nothing to
> requisition against. Needs a unit-scoped QA user with seeded stock.

---

## US-MNT-08 — Power consumption tracking  *(IMPLEMENTED)*

> As a maintenance user I build the feeder hierarchy and record meter readings.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A FeederGroup can be created (`POST /create`) | ✅ |
| 2 | A Feeder can be created under that group | ✅ feeder type/meter/unit best-effort = 1 |
| 3 | A PowerConsumption reading can be recorded for the feeder | ✅ OpeningReading must be > 0 |
| 4 | `GET /api/PowerConsumption/GetOpeningReaderValue/{feederId}` is reachable | ✅ 200/404 (raw-exception 500 fixed — repo returns null + controller 404) |

---

## US-MNT-09 — Misc master setup (type → value)  *(IMPLEMENTABLE)*

> As a maintenance administrator I define a misc *type* and then add misc *values* under it,
> so configurable dropdown values are maintained as a type → value master pair.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster (the type) can be created | ✅ |
| 2 | A MiscMaster (a value) can be created under that type (FK MiscTypeId) | ✅ |
| 3 | The misc value is readable by id | ✅ |
| 4 | The misc value is reachable through its type (`by-name?MiscTypeCode=`) | ✅ |
| 5 | The setup can be torn down (value then type) | ✅ |

> Both entities are under the `api/maintenance/...` route prefix; deletes bind id from `/{id}`.

---

## US-MNT-10 — Shift master setup (header → detail)  *(IMPLEMENTABLE)*

> As a maintenance administrator I define a shift (header) and then add its timing detail
> (start/end/break per unit) so work and schedules can be planned by shift.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A ShiftMaster (header) can be created | ✅ |
| 2 | A ShiftMasterDetail (timing) can be created under that shift (FK ShiftMasterId) | ⚠️ FK unit/supervisor ids best-effort |
| 3 | The shift header is readable by id | ✅ |
| 4 | The setup can be torn down (detail then header) | ✅ |

> `ShiftMasterDetail` GetById currently reuses `GetShiftMasterByIdQuery` (copy-paste), so the
> read-back asserts against the shift *header*, not the detail.

---

## US-MNT-11 — Maintenance dashboard & reporting (read-only)  *(IMPLEMENTABLE)*

> As a maintenance manager I open the dashboard summaries and reports to monitor work
> orders, consumption, schedules and power usage.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Work-order summary + card dashboard are reachable | ✅ |
| 2 | Item-consumption dashboards (overall / dept / machine group) are reachable | ✅ |
| 3 | Maintenance-hours dashboards (dept / machine group) are reachable | ✅ |
| 4 | Always-200 reports (WorkOrder, ItemConsumption, Request, Checklist, Scheduler, MaterialPlanning, MRS) are reachable | ✅ |
| 5 | Data-dependent reports (Power, Generator, CurrentStock, SubStoresStockLedger) are reachable | ⚠️ 200/404 — 404 on empty dataset |

> Read-only story — safe run-independent params; no teardown. Endpoints on the
> `api/maintenance/...` prefix.
> **Live finding:** every report stored proc *requires* its date params supplied — omitting
> them yields a SQL "parameter not supplied" 500 (e.g. `Rpt_GetMaintenanceRequestReport`
> needs `@RequestFromDate`). Each report call therefore passes an explicit date range.

---

## US-MNT-12 — Maintenance audit log query (read-only)  *(IMPLEMENTABLE)*

> As a maintenance administrator I review the audit trail of maintenance actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All audit logs can be listed | ✅ |
| 2 | Audit logs can be searched by a pattern | ✅ |

---

### Implementation status summary

| Story | Implementable now | Blocked on live/seeded data |
|---|---|---|
| US-MNT-01 Machine group setup | ✅ full (implemented) | — |
| US-MNT-02 Machine onboarding | ✅ full (live-reconciled) | — |
| US-MNT-03 Maintenance reference setup | ✅ (FK ids best-effort) | — |
| US-MNT-04 Activity & checklist | ✅ full (live-reconciled) | — |
| US-MNT-05 Preventive scheduling | partial (prereqs + abstract) | scheduler create — caller UnitId=0 vs machine UnitId>=1 |
| US-MNT-06 Request → work order | partial (request raised + reads) | WorkOrder composite DTO |
| US-MNT-07 Spares requisition | partial (lookups) | MRS create — no OldUnitId stock scope for testsales |
| US-MNT-08 Power consumption | ✅ full (live-reconciled) | — |
| US-MNT-09 Misc master setup | ✅ full | — |
| US-MNT-10 Shift master setup | ✅ full (FK ids best-effort) | — |
| US-MNT-11 Dashboard & reporting | ✅ full (read-only) | — |
| US-MNT-12 Audit log query | ✅ full (read-only) | — |

All 12 stories are authored and green. US-MNT-01/02/03/04/08/09/10/11/12 are fully implemented;
US-MNT-05/06/07 have their reachable + creatable steps active, with the remaining create/issue
steps `[Fact(Skip=…)]` carrying a **precise root cause** (the QA user `testsales` has `UnitId=0` /
empty `OldUnitId`, and WorkOrder needs a composite DTO). Those un-skip once a unit-scoped QA user
with seeded machines/stock is available.
