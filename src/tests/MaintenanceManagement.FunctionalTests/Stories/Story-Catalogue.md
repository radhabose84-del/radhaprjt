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

## US-MNT-02 — Machine onboarding  *(PARTIAL — machine create payload)*

> As a maintenance administrator I onboard a machine under a group and record its specification.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MachineGroup exists (created in-flow) | ✅ |
| 2 | A Machine can be created under that group | 🚫 complex machine payload — author with live data |
| 3 | A MachineSpecification can be recorded for the machine | 🚫 needs machine id |
| 4 | `GET /api/Machine/MachineGroup/{groupId}` returns the machine | 🚫 needs machine id |

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

## US-MNT-04 — Activity & checklist setup  *(PARTIAL — nested DTO)*

> As a maintenance planner I define an activity and its checklist items.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An ActivityMaster can be created (nested CreateActivityMasterDto) | 🚫 nested DTO — author with live data |
| 2 | An ActivityCheckListMaster can be created for that activity | 🚫 needs activity id |
| 3 | `POST /api/ActivityCheckListMaster/ByActivityId` returns the checklist for the activity | 🚫 needs activity id |

---

## US-MNT-05 — Preventive maintenance scheduling  *(BLOCKED — needs machine/activity data)*

> As a maintenance planner I configure a preventive schedule for a machine and an activity,
> map machines to it, and have work orders generated.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A machine and an activity exist | 🚫 depends on US-MNT-02/04 |
| 2 | Create a PreventiveScheduler (machine + activity + frequency) | 🚫 complex payload |
| 3 | Map machines to the schedule (`MapMachines`) | 🚫 needs schedule id |
| 4 | The scheduler abstract reflects the schedule by date | 🚫 needs posted schedule |

---

## US-MNT-06 — Maintenance request → work order lifecycle  *(BLOCKED — needs machine data)*

> As a maintenance user I raise a maintenance request, convert it to a work order,
> move it through its statuses, and see it in service history.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Raise an internal MaintenanceRequest | 🚫 complex payload |
| 2 | Create a WorkOrder (from the request or directly) | 🚫 needs request/machine ids |
| 3 | Move the work order through status values (`/api/WorkOrder/Status`) | 🚫 needs posted WO |
| 4 | The completed work appears in ServiceHistory | 🚫 needs posted WO |

---

## US-MNT-07 — Spares requisition & stock movement  *(BLOCKED — needs stock data)*

> As a maintenance user I raise a material requisition slip (MRS) and issue spares,
> moving stock in the ledger.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Resolve department / category / item via MRS lookups | 🚫 needs real unit/item data |
| 2 | Create an MRS (`POST /CreateMRS`) | 🚫 GRN/stock-driven payload |
| 3 | The MRS appears in `pending-issue` | 🚫 needs posted MRS |
| 4 | StockLedger reflects the issued quantity | 🚫 needs posted issue |

---

## US-MNT-08 — Power consumption tracking  *(PARTIAL — feeder hierarchy)*

> As a maintenance user I build the feeder hierarchy and record meter readings.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A FeederGroup can be created (`POST /create`) | ✅ |
| 2 | A Feeder can be created under that group | 🚫 many FKs (feeder type/meter/unit) |
| 3 | A PowerConsumption reading can be recorded for the feeder | 🚫 needs feeder id |
| 4 | `GET /api/PowerConsumption/GetOpeningReaderValue/{feederId}` returns the opening reading | 🚫 needs feeder id |

---

### Implementation status summary

| Story | Implementable now | Blocked on live/seeded data |
|---|---|---|
| US-MNT-01 Machine group setup | ✅ full (implemented) | — |
| US-MNT-02 Machine onboarding | partial | machine payload |
| US-MNT-03 Maintenance reference setup | ✅ (FK ids best-effort) | — |
| US-MNT-04 Activity & checklist | partial | nested DTO / activity id |
| US-MNT-05 Preventive scheduling | — | machine/activity data |
| US-MNT-06 Request → work order | — | machine/request data |
| US-MNT-07 Spares requisition | — | stock/item data |
| US-MNT-08 Power consumption | partial (group only) | feeder FKs |

US-MNT-03 is ready to implement as a full workflow now; US-MNT-02/04…08 should be implemented as
workflow classes with `[Fact(Skip="needs seeded data")]` on blocked steps, un-skipped during the
live reconciliation pass.
