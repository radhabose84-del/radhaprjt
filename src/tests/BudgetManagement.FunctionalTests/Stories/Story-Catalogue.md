# BudgetManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `BudgetManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each AC is tagged ✅ [implementable now] · ⚠️ [verify] ·
🚫 [blocked — needs seeded data]. Run a slice with `dotnet test --filter "Story=US-BUD-01"`.

## Module flow

```
MiscTypeMaster → MiscMaster (budget reference values: allocation rule, budget type, request type, …)
BudgetGroup (unit/department/costCenter/currency + budget type + allocation rule)
        │
        ├── BudgetRequest (OPEX→budgetGroup | CAPEX→project/WBS; approval workflow; auto RequestCode)
        └── BudgetAllocation (approved allocation against a request/group; array create)
ActivityLogs · AuditLog (read-only trails)
```

Key live facts: BudgetGroup create needs cross-module unit/department/costCenter/currency + a Budget
MiscMaster budget-type code (ANNUAL/MONTHLY); BudgetRequest/BudgetAllocation are approval-workflow
transactional with conditional OPEX/CAPEX rules — not resolvable on the clone. So the budget
lifecycle chain is blocked; the reference-master chain + reads are exercised now. DELETE: Misc/
BudgetGroup ROUTE `/{id}`; BudgetRequest QUERY `?id=`.

---

## US-BUD-01 — Budget reference master setup  *(IMPLEMENTABLE)*

> As a budget administrator I define misc reference types/values used by budget groups and requests.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/budget/misctypemaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type | ✅ |
| 3 | The value is readable by id and reachable via `by-name` | ⚠️ verify filter |
| 4 | Deactivating the MiscMaster excludes it from active autocomplete, keeps it in GetAll | ⚠️ verify |
| 5 | Teardown leaf-first (misc value → misc type) | ✅ |

---

## US-BUD-02 — Budget group → request → allocation  *(BLOCKED — needs seeded FK chain + approval)*

> As a budget manager I create a budget group, raise a budget request, and record an allocation.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The budget-group / request / allocation list + report reads are reachable | ✅ |
| 2 | A BudgetGroup can be created (unit/department/costCenter/currency + budget type) | 🚫 cross-module FKs + Budget budget-type misc code |
| 3 | A BudgetRequest can be raised (OPEX/CAPEX) | 🚫 requestType misc + approval workflow |
| 4 | A BudgetAllocation can be recorded against the request | 🚫 depends on request + financial year/allocation type |

---

## US-BUD-03 — Audit & activity trail (read-only)  *(IMPLEMENTABLE)*

> As a budget administrator I review the audit + activity trails of budget actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All budget audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |
| 3 | Activity logs for an entity are reachable (`/logs/{entityName}/{entityId}`) | ✅ tolerant 200/404 |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-BUD-01 Reference master setup | ✅ misc chain | — |
| US-BUD-02 Budget group → request → allocation | ✅ read/reachability | create chain — cross-module FKs + budget-type misc + approval workflow |
| US-BUD-03 Audit & activity trail | ✅ read-only | — |
