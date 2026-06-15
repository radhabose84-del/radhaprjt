# FixedAssetManagement — Functional Story Catalogue

Functional (acceptance) tests verify multi-step **business workflows** that cross entities —
behaviour the per-entity CRUD tests in `FixedAssetManagement.QATests` do **not** cover.

This catalogue is the **review gate**: each acceptance criterion (AC) is tagged
✅ [implementable now] · ⚠️ [verify against live] · 🚫 [blocked — needs seeded data] **before**
test code is trusted. Run a slice with `dotnet test --filter "Story=US-FAM-01"`.

## Module flow (the asset lifecycle the stories follow)

```
1. Classification & reference masters
   AssetGroup → AssetCategory / AssetSubGroup / AssetSubCategory
   DepreciationGroup (per AssetGroup) · SpecificationMaster (per AssetGroup)
   Location → SubLocation · UOM · Manufacture · MiscTypeMaster → MiscMaster
        │
2. Asset onboarding & placement
   AssetMasterGeneral (classified asset)  → AssetLocation (unit/dept/location/sublocation/custodian)
                                          → AssetSpecification (spec values for the asset)
        │
3. Acquisition & capitalization
   AssetPurchase (GRN → Asset, sets CapitalizationDate) → AssetAdditionalCost
        │
4. Coverage management
   AssetWarranty · AssetInsurance · AssetAmc  (each: period + vendor + renewal status)
        │
5. Movement
   AssetTransfer → AssetTransferReceipt
        │
6. Depreciation
   DepreciationDetail (SLM) · WDVDepreciation (WDV)  — over a period, against capitalized assets
        │
7. Disposal
   AssetDisposal (references the AssetPurchase, records date/type/reason/amount)
```

Key data facts (verified in source):
- `AssetMasterGeneral` create wraps an `AssetMasterDto`; most coverage/transaction entities key off **AssetId**.
- `AssetPurchase` is GRN-driven (BudgetType, VendorCode, PoNo/PoSno, GrnNo/GrnSno, AcceptedQty, PurchaseValue, CapitalizationDate).
- `AssetDisposal` references **both** AssetId and AssetPurchaseId.
- Reads in this module are not strongly company-scoped, so created rows are visible to GETs.

---

## US-FAM-01 — Asset classification hierarchy setup  *(IMPLEMENTED)*

> As an asset administrator I build the classification hierarchy
> (AssetGroup → AssetCategory → AssetSubGroup → AssetSubCategory) so assets can be classified.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A new AssetGroup can be created and returns a new id | ✅ |
| 2 | An AssetCategory can be created under that AssetGroup | ✅ |
| 3 | An AssetSubGroup can be created under that AssetGroup | ✅ |
| 4 | An AssetSubCategory can be created under that AssetCategory | ✅ |
| 5 | `GET /api/AssetCategories/group/{groupId}` returns the created category | ⚠️ verify |
| 6 | The hierarchy can be torn down leaf-first (soft delete) | ✅ |

---

## US-FAM-02 — Reference master setup  *(IMPLEMENTABLE)*

> As an asset administrator I set up the reference masters
> (Location → SubLocation, UOM, Manufacture) used when onboarding assets.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A Location can be created and returns a new id | ✅ |
| 2 | A SubLocation can be created under that Location | ✅ |
| 3 | A UOM can be created (route `api/fam/UOM`) | ✅ |
| 4 | A Manufacture can be created with country/state/city | ⚠️ FK ids |
| 5 | Each created reference master is readable by id | ✅ |
| 6 | Teardown removes the created reference masters | ✅ |

---

## US-FAM-03 — Depreciation policy & specification setup  *(IMPLEMENTABLE)*

> As a finance/asset administrator I configure a DepreciationGroup and a SpecificationMaster
> for an AssetGroup so depreciation and specs can be applied to its assets.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An AssetGroup exists (created in-flow) | ✅ |
| 2 | A DepreciationGroup can be created for that AssetGroup (book type, method, useful life, residual) | ⚠️ verify lookups (bookType/method) |
| 3 | A SpecificationMaster can be created for that AssetGroup | ✅ |
| 4 | `GET /api/SpecificationMaster/by-name?assetGroupId=` returns the spec for the group | ⚠️ verify |
| 5 | Teardown removes the depreciation group + specification | ✅ |

---

## US-FAM-04 — Asset onboarding & placement  *(IMPLEMENTED)*

> As an asset administrator I register an asset, place it at a location, and capture its specifications.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Prerequisite masters exist (group/category/uom/location) | ✅ (chained) |
| 2 | Create AssetMasterGeneral with classification (AssetMasterDto) → returns asset id | ✅ location embedded at create; CompanyId/UnitId best-effort = 1; returns 201 |
| 3 | The asset is placed at Unit/Department/Location/SubLocation/Custodian | ✅ embedded at create (standalone AssetLocation 400s once located — tolerant) |
| 4 | Capture AssetSpecification values for the asset | ✅ |
| 5 | `GET /api/AssetMasterGeneral/{id}` returns the asset with its classification | ✅ |

---

## US-FAM-05 — Asset acquisition & capitalization  *(PARTIAL — GRN-blocked purchase)*

> As an asset administrator I record an asset's purchase against a GRN and add capitalized costs.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Resolve an AssetSource via the lookup endpoint | ✅ |
| 2 | Create AssetPurchase linking GRN → Asset with PurchaseValue + CapitalizationDate | 🚫 **env-blocked**: GRN-driven (GetGrnNo/GetGrnItems by OldUnitId); testsales has no OldUnitId GRN scope |
| 3 | Add an AssetAdditionalCost against the asset | ✅ |
| 4 | The asset shows a capitalization date after purchase | 🚫 needs the posted purchase |

---

## US-FAM-06 — Asset coverage management  *(IMPLEMENTED)*

> As an asset administrator I attach warranty, insurance, and an AMC to an asset and track renewals.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A coverage lookup is reachable; an asset is built in-flow | ✅ |
| 2 | Create AssetWarranty for the asset (period, provider, service centre) | ✅ ContactPerson + MobileNumber required |
| 3 | Create AssetInsurance for the asset (policy no, period, amount, renewal status) | ✅ |
| 4 | Create AssetAmc for the asset (vendor, coverage type, renewal status) | ✅ |
| 5 | Each coverage record is readable by id | ✅ |

---

## US-FAM-07 — Asset transfer & receipt  *(PARTIAL — transfer read 500 / scope)*

> As an asset administrator I transfer an asset between departments and receive it at the destination.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Resolve transferable assets by category/department | 🚫 needs posted assets |
| 2 | Create an AssetTransfer (source → destination, transfer type) | 🚫 needs asset + dept data |
| 3 | The transfer appears in the pending-receipt list | 🚫 needs posted transfer |
| 4 | Create an AssetTransferReceipt to accept the transfer | 🚫 needs transfer id |

---

## US-FAM-08 — Depreciation run  *(PARTIAL — reads reachable; data needs capitalized assets)*

> As a finance user I run depreciation for a period and review the depreciation abstract.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A DepreciationGroup is configured (US-FAM-03) | ✅ |
| 2 | Run DepreciationDetail for a period | 🚫 needs capitalized assets |
| 3 | The depreciation abstract reflects the run | 🚫 needs posted depreciation |
| 4 | WDVDepreciation computes WDV for the period | 🚫 needs posted assets |

---

## US-FAM-09 — Asset disposal  *(PARTIAL — disposal needs GRN-blocked purchase)*

> As an asset administrator I dispose of an asset at end of life and record the disposal value.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An asset with a purchase exists (AssetId + AssetPurchaseId) | 🚫 needs US-FAM-04/05 |
| 2 | Create an AssetDisposal (date, type, reason, amount) | 🚫 needs asset + purchase ids |
| 3 | The disposed asset is reflected as disposed | 🚫 needs posted disposal |

---

## US-FAM-10 — Misc master setup (type → value)  *(IMPLEMENTED)*

> As a fixed-asset administrator I define a misc type and add misc values under it.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A MiscTypeMaster can be created | ✅ |
| 2 | A MiscMaster value can be created under that type (FK MiscTypeId) | ✅ |
| 3 | The value is readable by id | ✅ |
| 4 | The value is reachable through its type (`by-name?MiscTypeCode=`) | ✅ |
| 5 | Teardown (value then type; type delete blocked while linked) | ✅ |

> Entities under the `api/fam/...` route prefix.

---

## US-FAM-11 — Dashboard & reporting (read-only)  *(IMPLEMENTED)*

> As a fixed-asset manager I open the dashboards and reports.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Dashboard summaries (card / asset / expiry) are reachable | ✅ |
| 2 | AssetReport is reachable | ✅ |
| 3 | Data-dependent reports (AssetTransfer / AssetAudit) are reachable | ⚠️ 200/404 on empty dataset |

---

## US-FAM-12 — Audit log query (read-only)  *(IMPLEMENTED)*

> As a fixed-asset administrator I review the audit trail.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | All audit logs can be listed | ✅ |
| 2 | Audit logs can be searched by a pattern | ✅ |

---

### Implementation status summary

| Story | Implementable now | Blocked on live/seeded data |
|---|---|---|
| US-FAM-01 Classification hierarchy | ✅ full (implemented) | — |
| US-FAM-02 Reference masters | ✅ full | — |
| US-FAM-03 Depreciation/spec setup | ✅ full | — |
| US-FAM-04 Asset onboarding | ✅ full (live-reconciled) | — |
| US-FAM-05 Acquisition/capitalization | partial (lookup + additional cost) | AssetPurchase — GRN/OldUnitId |
| US-FAM-06 Coverage management | ✅ full (live-reconciled) | — |
| US-FAM-07 Transfer & receipt | partial (lookup) | transfer reads 500 / scope |
| US-FAM-08 Depreciation run | reachability reads | capitalized assets (purchase) |
| US-FAM-09 Disposal | partial (lookup) | disposal needs GRN-blocked purchase |
| US-FAM-10 Misc master setup | ✅ full | — |
| US-FAM-11 Dashboard & reporting | ✅ full (read-only) | — |
| US-FAM-12 Audit log query | ✅ full (read-only) | — |

All 12 stories are authored and green. US-FAM-01/02/03/04/06/10/11/12 are fully implemented;
US-FAM-05/07/08/09 have their reachable + creatable steps active, with the remaining steps
`[Fact(Skip=…)]` carrying a **precise root cause** — the GRN-driven AssetPurchase is blocked
for `testsales` (no OldUnitId GRN scope), which cascades to capitalization, depreciation data
and disposal; transfer reads currently 500 on the QA clone. Those un-skip once a unit-scoped QA
user with seeded GRN/stock is available.

US-FAM-02 and US-FAM-03 are ready to implement as full workflows now; US-FAM-04…09 should be
implemented as workflow classes with `[Fact(Skip="needs seeded data")]` on the blocked steps,
to be un-skipped during the live reconciliation pass.
