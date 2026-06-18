# PurchaseManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `PurchaseManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-PUR-01"`.

> ✅ Contracts below were copied verbatim from the just-authored QA suites
> (`PurchaseManagement.QATests/Tests/{Entity}/{Entity}QATests.cs`, verified against source 2026-06-17).
> ✅/⚠️ ACs are active ordered steps; 🚫 ACs are `[Fact(Skip="needs seeded data: …")]`.

## Module flow (the procure-to-receipt lifecycle the stories follow)

```
1. Reference masters (the vocabulary every PO & document references)
   MiscTypeMaster → MiscMaster   ·   MixCodeMaster (independent)
        │
2. Vendor terms & logistics masters
   PaymentTermMaster (baselineType = a MiscMaster value)
   VendorRatingGrade (clean, no FK)   ·   PortMaster (country + portType FKs)
        │
3. Return policy chain
   ReturnType → ReturnReason (returnTypeId FK)
        │
4. Procure-to-receipt pipeline (the transactional document flow)
   PurchaseIndent → PurchaseOrderLocal → GRNEntry
   (blocked: needs vendor/item/paymentTerm + budget + doc-numbering seed)
```

Key data facts (verified in source during QA authoring):
- Purchase masters' reads are filtered only by `IsDeleted = 0` → created rows are visible in GET-all/by-id within the run.
- **DELETE bindings:** every Purchase master in these stories binds id from the **ROUTE** (`DELETE …/{id}`).
- Create-response shapes are heterogeneous: some wrap `ApiResponseDTO<int>` (bare-int `data`), others echo a 201 wrapper inside an `Ok(200)` envelope; `CreatedId()` tolerates both — happy creates accept `BeOneOf(200, 201)`.
- Routes are case-insensitive: `MiscTypeMaster`/`MiscMaster` sit under `/api/purchase/…`; `MixCodeMaster`, `PaymentTermMaster`, `VendorRatingGrade`, `PortMaster`, `ReturnType`, `ReturnReason` sit under `/api/…` (no `purchase` prefix).
- Same-module FKs resolved at runtime via `FirstIdAsync`: `PaymentTermMaster.baselineTypeId` → `MiscMaster`; `PortMaster.countryId` → `Country`, `.portTypeId` → `MiscMaster`; `ReturnReason.returnTypeId` → `ReturnType`.
- `VendorRatingGrade` is a clean master (`actionTypeId` optional) → no FK needed.
- Procurement documents (`PurchaseIndent`, `PurchaseOrderLocal`, `GRNEntry`) need vendor/item/paymentTerm + budget + doc-numbering config — their create chain is blocked; their **read** surfaces are reachable (200/404 tolerant on the clone) and auth-protected.

---

## US-PUR-01 — Reference master vocabulary  *(IMPLEMENTABLE)*

> As a purchasing administrator I build the reference vocabulary (MiscTypeMaster → MiscMaster, plus an independent MixCodeMaster) so purchase orders and documents can reference consistent codes.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created and returns a new id | ✅ |
| 2 | A MiscMaster value can be created under that type (FK miscTypeId) | ✅ |
| 3 | A MixCodeMaster can be created (independent code+description master) | ✅ |
| 4 | Each created master is readable by id | ⚠️ verify (GetById guards differ per entity) |
| 5 | Teardown leaf-first — misc child → type → mixcode (all delete by ROUTE `/{id}`); dependent-delete probe on the type while child exists | ⚠️ verify dependent-delete block |

---

## US-PUR-02 — Vendor terms & logistics masters  *(IMPLEMENTABLE)*

> As a buyer I set up the payment-terms, vendor-rating and port vocabulary so purchase orders carry valid commercial and logistics references.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A baseline MiscMaster value is resolvable for the payment-term FK | ⚠️ verify (clone must hold ≥1 MiscMaster; create self-skips at 0) |
| 2 | A PaymentTermMaster can be created (baselineTypeId FK + creditDays) | ✅ |
| 3 | A VendorRatingGrade can be created (clean master, no FK) | ✅ |
| 4 | A PortMaster can be created (countryId + portTypeId FKs) | ⚠️ verify (needs a Country + MiscMaster on the clone; create self-skips if either is 0) |
| 5 | Each created master is readable by id | ⚠️ verify |
| 6 | Teardown — each master deletes by ROUTE `/{id}` (tolerant) | ✅ |

---

## US-PUR-03 — Return policy chain  *(IMPLEMENTABLE)*

> As a procurement administrator I configure a ReturnType, then a ReturnReason that references it, so goods returns are categorised consistently.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A ReturnType can be created (code + description + flags) | ✅ |
| 2 | A ReturnReason can be created under that type (FK returnTypeId) | ✅ |
| 3 | The by-return-type lookup is reachable, and each row readable by id | ⚠️ verify (200/404 tolerant) |
| 4 | Dependent-delete probe — deleting the ReturnType while a reason links it is blocked (400) or permitted (200); then teardown reason → type | ⚠️ verify dependent-delete block |

---

## US-PUR-04 — Procure-to-receipt readiness  *(MOSTLY BLOCKED — readiness/reachability)*

> As a procurement user I expect the procure-to-receipt pipeline endpoints (indent → PO → GRN) to be reachable and secured even before seeded transactional data exists.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A full Purchase Order can be created end-to-end | 🚫 needs seeded data: vendor/item/paymentTerm + budget + doc-numbering "Purchase Order" |
| 2 | The PurchaseIndent pending list is reachable | ⚠️ reachability (200/404 tolerant) |
| 3 | The PurchaseOrderLocal pending-PO list is reachable | ⚠️ reachability (200/404 tolerant) |
| 4 | The GRNEntry pending-header list is reachable | ⚠️ reachability (200/404 tolerant) |
| 5 | Each pipeline read rejects anonymous callers (401) | ✅ security |

---

### Implementation status summary

| Story | Implementable now | Blocked on seeded data |
|---|---|---|
| US-PUR-01 Reference master vocabulary | ✅ full | — |
| US-PUR-02 Vendor terms & logistics masters | ✅ full (FK creates self-skip if clone lacks a MiscMaster/Country) | — |
| US-PUR-03 Return policy chain | ✅ full | — |
| US-PUR-04 Procure-to-receipt readiness | ✅ reachability + auth-protection | full PO document flow (vendor/item/paymentTerm + budget + doc-numbering) |

US-PUR-01/02/03 implement as fully-active master chains with leaf-first teardown.
US-PUR-04 implements as reachability + security active and the full PO document create
`[Fact(Skip="needs seeded data: …")]`, to be un-skipped once a unit-scoped QA user with seeded
vendor/item/paymentTerm + budget + doc-numbering config is available on `BannariERP_QATest`.
