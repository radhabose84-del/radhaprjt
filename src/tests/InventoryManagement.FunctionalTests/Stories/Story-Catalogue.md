# InventoryManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `InventoryManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-INV-01"`.

> ✅ Contracts below were copied verbatim from the just-authored QA suites
> (`InventoryManagement.QATests/Tests/{Entity}/{Entity}QATests.cs`, verified against source 2026-06-17).
> ✅/⚠️ ACs are active ordered steps; 🚫 ACs are `[Fact(Skip="needs seeded data: …")]`.

## Module flow (the inventory lifecycle the stories follow)

```
1. Reference masters (the vocabulary every item & document references)
   MiscTypeMaster → MiscMaster → UOM (uomType = a MiscMaster value)
   ItemGroup · ItemCategory · HSNMaster · PriceGroupMaster · UsageType
        │
2. Item catalogue (the catalogued goods themselves)
   ItemSpecificationMaster → ItemSpecificationValue
   ItemMaster (nested: itemGroup / itemCategory / stockUom / hsn + spec tabs/variants)
        │
3. Material flow (the transactional movement of stock)
   Mrs (Material Requisition Slip) → Issue (material issue against an approved MRS)
   → StockMovement / StockLedger
        │
4. Reporting & audit (the read-only insight layer)
   Reports (CurrentStock · SubStoresCurrentStock · CurrentStockUnitWise · by-division)
   AuditLog (Mongo-backed action trail)
```

Key data facts (verified in source during QA authoring):
- Inventory misc + UOM + spec masters' reads are filtered only by `IsDeleted = 0` → created rows are visible in GET-all/by-id within the run.
- **DELETE bindings differ per entity** (copied from QA): inventory `MiscTypeMaster` / `MiscMaster` / `UOM` bind id from the **ROUTE** (`DELETE …/{id}`); `ItemGroup`, `ItemSpecificationMaster`, `ItemSpecificationValue` bind id from the **QUERY** (`DELETE …?id={id}`).
- Create-response shapes are heterogeneous: some wrap `ApiResponseDTO<int>` (bare-int `data`), others echo a bare DTO under `data`; `CreatedId()` tolerates both — happy creates accept `BeOneOf(200, 201)`.
- `UOM.uomTypeId` is a same-module FK → `MiscMaster`; `ItemSpecificationValue.specificationMasterId` is a same-module FK → `ItemSpecificationMaster` (resolved at runtime via `FirstIdAsync`).
- `ItemMaster` create needs a fully-formed nested DTO (itemGroup / itemCategory / stockUom / hsn + tabs) — its create is blocked; its **reads** (GetAll smoke, autocomplete) are reachable.
- Transactional documents (`Mrs`, `Issue`) need the requesting Unit's workflow configuration plus warehouse stock — their create chain is blocked; their **read** surfaces are reachable (often 200/404 tolerant on the clone).

---

## US-INV-01 — Reference master chain  *(IMPLEMENTABLE)*

> As an inventory administrator I build the reference vocabulary (MiscTypeMaster → MiscMaster → UOM, plus ItemGroup) so items and documents can reference consistent codes.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created and returns a new id | ✅ |
| 2 | A MiscMaster value can be created under that type (FK miscTypeId) | ✅ |
| 3 | A UOM can be created with uomTypeId pointing at a MiscMaster value | ✅ |
| 4 | An ItemGroup can be created (independent code+name master) | ✅ |
| 5 | Each created master is readable by id | ⚠️ verify (GetById guards differ per entity) |
| 6 | Teardown leaf-first — inventory misc/UOM delete by ROUTE `/{id}`; ItemGroup deletes by QUERY `?id=` | ⚠️ verify dependent-delete block |

---

## US-INV-02 — Item catalogue setup  *(PARTIAL)*

> As an inventory administrator I define item specification masters and values, then catalogue an item that references them.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An ItemSpecificationMaster can be created (code + name + order, each unique) | ✅ |
| 2 | An ItemSpecificationValue can be created under that master (FK specificationMasterId) | ✅ |
| 3 | The ItemMaster read surface is reachable (GetAll + autocomplete) | ✅ reachability |
| 4 | An ItemMaster can be created from group/category/stockUom/hsn + the spec | 🚫 needs nested ItemMaster payload (itemGroup/itemCategory/stockUom/hsn) |
| 5 | Teardown — spec value then spec master, both delete by QUERY `?id=` | ✅ |

---

## US-INV-03 — Material flow & audit  *(BLOCKED on transactional create; reads reachable)*

> As a stores user I requisition material (MRS), issue it against the approved MRS, then review current stock and the audit trail.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | The MRS / Issue / CurrentStock read surfaces are reachable | ✅ reachability (200/404 tolerant) |
| 2 | The MRS entry-details endpoint rejects anonymous callers (401) | ✅ |
| 3 | An MRS can be raised and issued through to stock movement | 🚫 needs Unit workflow config + warehouse stock |
| 4 | The inventory audit trail can be listed | ✅ read-only (Mongo-backed; empty set valid) |

---

### Implementation status summary

| Story | Implementable now | Blocked on seeded data |
|---|---|---|
| US-INV-01 Reference master chain | ✅ full | — |
| US-INV-02 Item catalogue setup | ✅ specs + reachability | ItemMaster nested-payload create |
| US-INV-03 Material flow & audit | ✅ reachability + audit reads | MRS→Issue transactional chain (Unit workflow + stock) |

US-INV-01 implements as a fully-active master chain with leaf-first teardown.
US-INV-02 implements with the spec master/value creates active and the ItemMaster create
`[Fact(Skip="needs seeded data: …")]`. US-INV-03 implements as reachability + audit reads active
and the MRS→Issue transactional create chain `[Fact(Skip=…)]`, to be un-skipped once a unit-scoped
QA user with seeded warehouse stock + workflow config is available on `BannariERP_QATest`.
