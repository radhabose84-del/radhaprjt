# LogisticsManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `LogisticsManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] before test
code is trusted. Run a slice with `dotnet test --filter "Story=US-LOG-01"`.

LogisticsManagement is a small **reference/support** module (freight rates + misc reference data
consumed cross-module, e.g. by SalesManagement DispatchAddressMaster). Its workflows are
master-setup chains; there are no transactional documents here.

## Module flow

```
MiscTypeMaster → MiscMaster (typed reference values: FreightMode, RateMethod, …)
                      │
                      ▼
                 FreightMaster (freightMode + rateMethod + rate, per module)
AuditLog (read-only trail of the above)
```

---

## US-LOG-01 — Logistics reference master setup  *(IMPLEMENTABLE)*

> As a logistics administrator I define misc reference types/values and a freight rate so other
> modules can reference freight + logistics lookups.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created | ✅ |
| 2 | A MiscMaster value can be created under that type (FK MiscTypeId) | ✅ |
| 3 | The value is readable by id and reachable via `by-name` | ⚠️ verify autocomplete filter |
| 4 | Deactivating the MiscMaster excludes it from active autocomplete but keeps it in GetAll | ⚠️ verify |
| 5 | A FreightMaster rate can be created (freightMode + rateMethod misc + module) | ⚠️ needs a valid FreightMode↔RateMethod combo on the clone |
| 6 | Teardown leaf-first (freight → misc value → misc type) | ✅ |

---

## US-LOG-02 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a logistics administrator I review the audit trail of logistics actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All logistics audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-LOG-01 Reference master setup | ✅ misc chain; ⚠️ freight combo | freight create if no valid combo on clone |
| US-LOG-02 Audit trail | ✅ read-only | — |
