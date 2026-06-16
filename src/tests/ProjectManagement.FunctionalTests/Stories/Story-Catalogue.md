# ProjectManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `ProjectManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] before test
code is trusted. Run a slice with `dotnet test --filter "Story=US-PRJ-01"`.

## Module flow

```
MiscTypeMaster → MiscMaster (project reference values: project type/category/status, …)
ProjectMaster (project + budget/cost-centre/currency + approval workflow, auto ProjectCode)
        │
        └── ProjectWorkBreakdownStructure (hierarchical WBS nodes / milestones under a project)
AuditLog (read-only trail)
```

Key live facts (verified during QA authoring): ProjectMaster create needs a broad cross-module FK
chain (projectType / projectCategory / unit / department / budgetYear / costCenter / currency /
assetGroup) **and** an approval-workflow config — not resolvable on the clone — so the project →
WBS create chain is blocked; the reference-master chain + reads are exercised now. All
ProjectManagement DELETEs bind id from the **ROUTE** (`/{id}`).

---

## US-PRJ-01 — Project reference master setup  *(IMPLEMENTABLE)*

> As a project administrator I define misc reference types/values used to classify projects.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/project/MiscTypeMaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type (FK MiscTypeId) | ✅ |
| 3 | The value is readable by id and reachable via `by-name` | ⚠️ verify filter |
| 4 | Deactivating the MiscMaster excludes it from active autocomplete, keeps it in GetAll | ⚠️ verify |
| 5 | Teardown leaf-first (misc value → misc type) | ✅ |

---

## US-PRJ-02 — Project → WBS lifecycle  *(BLOCKED — needs seeded FK chain)*

> As a project manager I create a project and build its work-breakdown structure.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The project list + autocomplete + pending-approvals reads are reachable | ✅ |
| 2 | A ProjectMaster can be created (type/category/unit/dept/budget/costcentre/currency/assetgroup) | 🚫 cross-module FK chain + approval-workflow config not resolvable on clone |
| 3 | A WBS node can be created under that project | 🚫 depends on AC2 |
| 4 | The WBS tree (by-project / parent lookup) reflects the node | 🚫 depends on AC3 |
| 5 | Teardown leaf-first (WBS → project) | 🚫 depends on the create chain |

---

## US-PRJ-03 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a project administrator I review the audit trail of project actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All project audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-PRJ-01 Reference master setup | ✅ misc chain | — |
| US-PRJ-02 Project → WBS lifecycle | ✅ read/reachability | create chain — project FK chain + approval workflow, then WBS |
| US-PRJ-03 Audit trail | ✅ read-only | — |
