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

## US-FAM-04 — Asset onboarding & placement  *(PARTIAL — asset create blocked)*

> As an asset administrator I register an asset, place it at a location, and capture its specifications.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Prerequisite masters exist (group/category/uom/location) | ✅ (chained) |
| 2 | Create AssetMasterGeneral with classification (AssetMasterDto) → returns asset id | 🚫 complex `AssetMasterDto` payload — author with live data |
| 3 | Assign the asset to Unit/Department/Location/SubLocation/Custodian via AssetLocation | 🚫 needs the created asset id |
| 4 | Capture AssetSpecification values for the asset | 🚫 needs asset id + spec master |
| 5 | `GET /api/AssetMasterGeneral/{id}` returns the asset with its classification | 🚫 needs asset id |

---

## US-FAM-05 — Asset acquisition & capitalization  *(BLOCKED — needs GRN data)*

> As an asset administrator I record an asset's purchase against a GRN and add capitalized costs.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Resolve an AssetSource and a GRN via the lookup endpoints | 🚫 needs real GRN data |
| 2 | Create AssetPurchase linking GRN → Asset with PurchaseValue + CapitalizationDate | 🚫 GRN-driven payload |
| 3 | Add an AssetAdditionalCost against the asset | 🚫 needs asset id |
| 4 | The asset shows a capitalization date after purchase | 🚫 needs posted purchase |

---

## US-FAM-06 — Asset coverage management  *(PARTIAL — needs asset id)*

> As an asset administrator I attach warranty, insurance, and an AMC to an asset and track renewals.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An asset exists | 🚫 depends on US-FAM-04 asset id |
| 2 | Create AssetWarranty for the asset (period, provider, service centre) | 🚫 needs asset id |
| 3 | Create AssetInsurance for the asset (policy no, period, amount, renewal status) | 🚫 needs asset id |
| 4 | Create AssetAmc for the asset (vendor, coverage type, renewal status) | 🚫 needs asset id |
| 5 | Each coverage record is readable for the asset | 🚫 needs asset id |

---

## US-FAM-07 — Asset transfer & receipt  *(BLOCKED — needs asset/department data)*

> As an asset administrator I transfer an asset between departments and receive it at the destination.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | Resolve transferable assets by category/department | 🚫 needs posted assets |
| 2 | Create an AssetTransfer (source → destination, transfer type) | 🚫 needs asset + dept data |
| 3 | The transfer appears in the pending-receipt list | 🚫 needs posted transfer |
| 4 | Create an AssetTransferReceipt to accept the transfer | 🚫 needs transfer id |

---

## US-FAM-08 — Depreciation run  *(BLOCKED — needs capitalized assets)*

> As a finance user I run depreciation for a period and review the depreciation abstract.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | A DepreciationGroup is configured (US-FAM-03) | ✅ |
| 2 | Run DepreciationDetail for a period | 🚫 needs capitalized assets |
| 3 | The depreciation abstract reflects the run | 🚫 needs posted depreciation |
| 4 | WDVDepreciation computes WDV for the period | 🚫 needs posted assets |

---

## US-FAM-09 — Asset disposal  *(BLOCKED — needs asset + purchase)*

> As an asset administrator I dispose of an asset at end of life and record the disposal value.

| # | Acceptance criterion | Status |
|---|---|---|
| 1 | An asset with a purchase exists (AssetId + AssetPurchaseId) | 🚫 needs US-FAM-04/05 |
| 2 | Create an AssetDisposal (date, type, reason, amount) | 🚫 needs asset + purchase ids |
| 3 | The disposed asset is reflected as disposed | 🚫 needs posted disposal |

---

### Implementation status summary

| Story | Implementable now | Blocked on live/seeded data |
|---|---|---|
| US-FAM-01 Classification hierarchy | ✅ full (implemented) | — |
| US-FAM-02 Reference masters | ✅ full | — |
| US-FAM-03 Depreciation/spec setup | ✅ full | — |
| US-FAM-04 Asset onboarding | partial | AssetMasterDto payload |
| US-FAM-05 Acquisition/capitalization | — | GRN data |
| US-FAM-06 Coverage management | — | asset id |
| US-FAM-07 Transfer & receipt | — | asset/dept data |
| US-FAM-08 Depreciation run | setup only | capitalized assets |
| US-FAM-09 Disposal | — | asset + purchase ids |

US-FAM-02 and US-FAM-03 are ready to implement as full workflows now; US-FAM-04…09 should be
implemented as workflow classes with `[Fact(Skip="needs seeded data")]` on the blocked steps,
to be un-skipped during the live reconciliation pass.
