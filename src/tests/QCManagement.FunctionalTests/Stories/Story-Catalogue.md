# QCManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `QCManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each AC is tagged ✅ [implementable now] · ⚠️ [verify] ·
🚫 [blocked — needs seeded data]. Run a slice with `dotnet test --filter "Story=US-QC-01"`.

## Module flow

```
MiscTypeMaster → MiscMaster (QC reference values: QP_PARAMETER_GROUP, QP_DATA_TYPE,
                 QP_VALIDATION_TYPE, QP_APPLICABLE_LEVEL, QP_QC_TYPE, QP_SOURCE_TYPE, …)
QualityParameter → QualityTemplate (parameters[]) → QualitySpecification (item/category + criteria)
QcInspection (from GRN/Arrival; snapshots the spec; record readings → disposition)
AuditLog (read-only trail)
```

Key live facts: QualityParameter/Template/Specification creates depend on QC MiscMaster reference
values under specific MiscTypeCodes (QP_PARAMETER_GROUP/QP_DATA_TYPE/QP_VALIDATION_TYPE/…) that may
not exist on the clone, and QualitySpecification/QcInspection additionally need cross-module
Inventory items/categories + Purchase GRN/Arrival source documents. So the deep quality-definition
+ inspection chain is blocked; the reference-master chain + reads are exercised now. All DELETE bind
id from QUERY `?id=`; Misc validators enforce alphanumeric code + isActive 0/1.

---

## US-QC-01 — QC reference master setup  *(IMPLEMENTABLE)*

> As a QC administrator I define misc reference types/values used by quality parameters and specs.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/qc/misctypemaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type | ✅ |
| 3 | The value is readable by id and reachable via `by-name` | ⚠️ verify filter |
| 4 | Deactivating the MiscMaster excludes it from active autocomplete, keeps it in GetAll | ⚠️ verify |
| 5 | Teardown leaf-first (misc value → misc type) | ✅ |

---

## US-QC-02 — Quality definition & inspection  *(PARTIAL — deep chain blocked)*

> As a QC engineer I define a parameter, build a template/specification, and run an inspection.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The parameter / template / specification / inspection list reads are reachable | ✅ |
| 2 | A QualityParameter can be created (group/dataType/validationType QC misc) | ⚠️ needs QC misc values (QP_PARAMETER_GROUP/QP_DATA_TYPE/QP_VALIDATION_TYPE) on the clone |
| 3 | A QualityTemplate can be built from that parameter | 🚫 depends on AC2 |
| 4 | A QualitySpecification can be created (template + item/category + criteria) | 🚫 needs template + Inventory item/category + matching params |
| 5 | A QcInspection can be created from a GRN/Arrival and dispositioned | 🚫 needs Purchase GRN/Arrival source + resolved spec |

---

## US-QC-03 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a QC administrator I review the audit trail of QC actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All QC audit logs can be listed | ✅ (raw list; status-only, tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern (`searchPattern=`) | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-QC-01 Reference master setup | ✅ misc chain | — |
| US-QC-02 Quality definition & inspection | ✅ read/reachability; ⚠️ parameter | template/spec/inspection — QC misc + Inventory + Purchase GRN |
| US-QC-03 Audit trail | ✅ read-only | — |
