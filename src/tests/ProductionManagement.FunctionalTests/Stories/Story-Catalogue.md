# ProductionManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `ProductionManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each AC is tagged ✅ [implementable now] · ⚠️ [verify] ·
🚫 [blocked — needs seeded data]. Run a slice with `dotnet test --filter "Story=US-PROD-01"`.

ProductionManagement is the textile production module: count/yarn/process/quality reference
masters → lots → production-pack entries / repacking (document-numbered, stock-bearing).

## Module flow

```
MiscTypeMaster → MiscMaster (production reference values: count type, lot type, status, …)
CountGroup → CountMaster · ProcessMaster · QualityMaster · YarnType · YarnTwistMaster ·
            CertificationMaster · RawMaterialType · PackType   (reference masters)
LotMaster (item lot) ──► ProductionPackEntry (pack production; doc-numbered 'PackMaster')
                        RepackingHeader (repack / yarn-conversion; doc-numbered)
AuditLog (read-only)
```

Key live facts: the reference masters (Misc/MiscType/Process/Quality/CountGroup/YarnTwist/
Certification/RawMaterialType/PackType/YarnType) are clean creatable masters; CountMaster + LotMaster
need cross-module Inventory item/UOM + production misc; ProductionPackEntry + RepackingHeader are
document-numbered ('PackMaster'/'RePackMaster'/'YarnConversion' — not seeded) and stock-bearing →
blocked. DELETE binds id from QUERY `?id=` for the Misc + reference masters.

---

## US-PROD-01 — Production reference master setup  *(IMPLEMENTABLE)*

> As a production administrator I define misc reference values and a process master.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created (`/api/production/misctypemaster`) | ✅ |
| 2 | A MiscMaster value can be created under that type | ✅ |
| 3 | A ProcessMaster can be created (`/api/processmaster`) | ✅ |
| 4 | Each is readable by id and reachable via autocomplete | ⚠️ verify |
| 5 | Teardown leaf-first | ✅ |

---

## US-PROD-02 — Lot & production pack  *(BLOCKED — needs seeded item/stock + doc-numbering)*

> As a production user I create a lot and record a production-pack entry against it.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The lot / production-pack / repacking list reads are reachable | ✅ |
| 2 | A LotMaster can be created (lotType/status misc + Inventory item) | 🚫 cross-module Inventory item + production misc (lotType/status) |
| 3 | A ProductionPackEntry can be recorded against the lot | 🚫 doc-numbering 'PackMaster' + warehouse/item/lot/packType chain |
| 4 | A RepackingHeader can repack/convert packed stock | 🚫 doc-numbering + packed stock |

---

## US-PROD-03 — Audit trail (read-only)  *(IMPLEMENTABLE)*

> As a production administrator I review the audit trail of production actions.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All production audit logs can be listed | ✅ (raw list; tolerant 200/404) |
| 2 | Audit logs can be searched by a pattern | ⚠️ verify endpoint shape |

---

### Implementation status summary

| Story | Implementable now | Blocked |
|---|---|---|
| US-PROD-01 Reference master setup | ✅ misc + process | — |
| US-PROD-02 Lot & production pack | ✅ read/reachability | lot/pack/repack — Inventory item + doc-numbering + packed stock |
| US-PROD-03 Audit trail | ✅ read-only | — |
