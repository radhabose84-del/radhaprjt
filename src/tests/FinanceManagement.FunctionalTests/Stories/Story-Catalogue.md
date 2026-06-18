# FinanceManagement — Functional Story Catalogue

Review gate: approve a story's AC table here **before** trusting its test code.
Tags: ✅ [implementable] · ⚠️ [verify live] · 🚫 [blocked — needs seeded data].

---

## US-GL02-02 — Account Group Hierarchy Builder (4-level tree, approval-gated Move)

**User story:** As a Finance Controller, I maintain the Segment → Group → Sub-group → Account hierarchy
so balances roll up at every level and accounts attach only at the leaf; structural re-parenting changes
statutory presentation, so a Move is gated behind Finance Controller approval (tracked in
`Finance.AccountGroupChangeRequest`).

**Pre-condition (seed):** `Finance.AccountTypeMaster` is seeded (Asset = 1) in the QA clone. The
approval half (Move → applied) needs **RabbitMQ + BSOFT.Worker** running and the workflow config
(`WorkflowType` for Menu 1288 + Finance-Controller `ApprovalStepDetail` + step-unit mappings). GL
leaf-only assign needs `GlAccountMaster` reference lookups; FR-003 map needs a seeded `ScheduleIIILineItem`.

Base route: `api/finance/accountgroup`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — levels summarise; leaf-only | Given a branch L1→L4 is created, When GET `/tree`, Then each level nests its children and only the bottom node is `IsLeaf` (accounts attach there). | ✅ implementable |
| AC2 — reject non-leaf account assign | Given a GL account, When POST `/api/finance/glaccountmaster` with `accountGroupId` = a non-leaf, Then 400 "Accounts attach only at leaf level — select a leaf group." | 🚫 needs GL lookups |
| AC3 — circular / wrong-level move blocked | Given a node, When POST `/move` under its own descendant or a parent not exactly one level above, Then 400. | ✅ implementable |
| AC4 — parent totals = Σ children | Given accounts under a leaf, When balances change, Then parent totals reflect the sum at all levels. | 🚫 blocked — no GL posting/ledger source |
| AC5 — account in exactly one group | Given an account, When (re)assigned, Then it belongs to exactly one group (single `GlAccountMaster.AccountGroupId` FK). | ✅ structural |
| Move — submitted for approval | Given a valid Move, When POST `/move`, Then 200 "submitted for Finance Controller approval", an `AccountGroupChangeRequest` is Pending and the group is NOT yet re-parented. | ⚠️ verify live |
| Move — applied on approval | Given a Pending request, When the Finance Controller approves, Then the consumer re-parents the group + marks the request Approved (old parent → leaf, new parent → non-leaf). | 🚫 needs RabbitMQ + Worker + workflow |
| FR-003 — Schedule III mapping | Given a group, When PUT `/schedule-iii-mapping {scheduleIIILineItemId}`, Then GET `/{id}` shows `scheduleIIILineName` (null clears it). | 🚫 needs seeded line |

> Single self-referencing `AccountGroup` table (adjacency list); `Level` derived (parent+1), `IsLeaf`
> maintained on create/move/delete. The Move uses a transactional outbox + `AccountGroupChangeRequest`
> so the request is raised atomically and applied only after approval (engine is unit-scoped — payload
> wraps `UnitId` at `$.Header.UnitId`). See `docs/AccountGroupHierarchy_HLD.md`.

---

## US-GL02-03A — Schedule III Line-Item & Sub-total Configuration

**User story:** As a Finance Controller, I maintain the Schedule III statement structure — its
line items, sections, sub-total nodes, Division variant and the traded/manufactured split — so
that the 03B mapping screen and the auto-generated statements draw from one governed, versioned
definition rather than hardcoded lines.

**Pre-condition (seed):** the screen has **no create-structure endpoint**. A
`Finance.ScheduleIIIStructure` + `Finance.ScheduleIIISection` for `(CompanyId=1, DivisionId=7)`
plus the `S3_*` MiscMaster rows (`ScheduleIIIMiscSeed.sql`) must exist in the QA clone. Steps that
need them are 🚫 and `[Fact(Skip=...)]` until seeded.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| Structure read | Given a seeded structure, When GET `/structure?companyId=1&divisionId=7`, Then 200 with `data` (sections + nested lines). | ⚠️ verify live |
| AC1 — add line → 03B | Given a BS line added under a section, When GET `/preview-03b/{structureId}`, Then the new line appears in the Balance-Sheet leaf list (no code change). | 🚫 needs seed |
| AC2 — EBITDA Include-Other-Income | Given a sub-total, When PUT `/subtotal` with `includeOtherIncome=true` + operands, Then `GetSubTotals` reflects the new formula/flag (recompute). | 🚫 needs seed |
| AC5 — delete blocked when mapped | Given a line with mapped account groups (US-GL02-03B), When DELETE, Then blocked: "Cannot delete — account group(s) are mapped…". *(Mapped count is stubbed to 0 until 03B ships → not assertable yet.)* | 🚫 needs 03B |
| Lock + FR-008 | Given a Draft structure, When POST `/lock`, Then status → Locked and subsequent edits are rejected ("change control (FR-008)"). | 🚫 needs seed |

> AC3 (division switch) and AC4 (textile split) were de-scoped to **query-only** (toggle state is
> read from stored data via `GetStructure`/`GetSubTotals`); no dedicated command exists, so they are
> not separate functional steps.

---

## US-GL02-05A / 05B — Tax Code Catalogue + Tax-Account Linkage

**User story:** As a Tax Lead, I maintain the tax-code catalogue (GST in/out, IGST, TDS, customs)
with rates, statutory sections and effective-dated versions, and link those codes to GL accounts,
so the AR / AP / TX modules and the linkage screen draw from one governed, versioned source.

**Pre-condition (seed):** tax codes & GSTR sections are **self-seeding** (create endpoints exist).
Linkage steps need a real `Finance.GlAccountMaster` Id (FK) — `[Fact(Skip=...)]` until a GL account
is resolvable in the QA clone.

Base route: `api/finance/TaxCode`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1-A — create → available | Given a new code `GST-OUT-5` is created, When GET `/tax-code/by-name` and `/tax-code/effective`, Then it is returned (available to the linkage screen + AR/AP/TX, no code change). | ✅ implementable |
| AC3-A — rate change versioned | Given a rate change via POST `/tax-code/{id}/rate-version` with an effective date, When GET `/tax-code/{id}/versions`, Then the prior version is retained and `/tax-code/effective` returns the old rate before the date and the new rate on/after it (never retroactive). | ✅ implementable |
| AC4-A — reject invalid | Given a GST code with no rate (or a TDS code with no section), When POST `/tax-code`, Then 400 field-level error. | ✅ implementable |
| Deactivate | Given an active code, When PUT with `isActive=0`, Then it is excluded from `/tax-code/by-name` (autocomplete) but still present in `/tax-code` GetAll (`IsDeleted=0`). | ✅ implementable |
| AC5-A — delete blocked when linked | Given a code linked to a GL account (05B), When DELETE `/tax-code/{id}`, Then blocked: "Cannot delete — code is linked to [N] GL account(s). Unlink first." | 🚫 needs GL account |
| AC2-B — activation needs GL | Given a linkage with a GL mapping, When POST `/linkage/{id}/activate`, Then 200; without a GL mapping it is rejected. | 🚫 needs GL account |
| AC4-B — dual-approval change | Given a linkage change via POST `/linkage/change-request`, When submitted, Then it appears in `/linkage/change-audit` as PENDING and dual approval (FC + Tax Lead) is driven by the BackgroundService Workflow module. | 🚫 needs GL account + workflow |

> Composite GST is modelled as child component codes (CGST/SGST) under a COMBINED header (Tax Lead
> ruling); TaxCode allows hyphens **and dots** (`^[A-Za-z0-9.-]+$`) so codes like `GST-OUT-CGST-2.5`
> validate. TDS `StatutorySection` uses legacy 194x placeholders pending the IT Act 2025 clause numbers.

---

## US-GL02-12 — Account Currency & Forex Configuration

**User story:** As a Finance Controller, I maintain the currency-type master (INR-only / Forex /
Multi-currency, …) so the GL Account screen's single "Currency Type" dropdown draws from one
governed list, instead of free-form values, ahead of forex postings and period-end revaluation.

**Pre-condition (seed):** the currency-type master is **self-seeding** (create endpoint exists,
CompanyId comes from the token). The configuration lifecycle runs live. Enforcement (postings,
revaluation, EEFC report, currency-type lock) is **GL-04 (Sprint 2)** — those steps are 🚫.

Base route: `api/finance/CurrencyForexConfig`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| Create → available in dropdown | Given a new currency type is created, When GET `/by-name`, Then it is returned (offered to the GL "Currency Type" dropdown, no code change). | ✅ implementable |
| Edit | Given an existing type, When PUT with a new name, Then GET `/{id}` reflects it (code immutable). | ✅ implementable |
| Deactivate | Given an active type, When PUT with `isActive=0`, Then it is excluded from `/by-name` (dropdown) but still present in GetAll (`IsDeleted=0`). | ✅ implementable |
| Delete blocked when linked (Rule 25) | Given a type referenced by a GL account (`GlAccountMaster.CurrencyTypeId`), When DELETE, Then blocked: "linked with other records". | 🚫 needs GL account |
| AC1 — reject FC posting to INR-only | Given an INR-only account, When a USD posting is attempted, Then rejected. | 🚫 GL-04 |
| AC2 — permit forex posting | Given a forex export-debtor account, When USD/EUR posting, Then permitted. | 🚫 GL-04 |
| AC3 — revaluation routing | Given a revaluation-account mapping, When period-end revaluation posts, Then the difference routes to the configured forex gain/loss account. | 🚫 GL-04 |
| AC4 — EEFC balance report | Given EEFC accounts, When the EEFC balance report runs, Then balances are listed for FEMA. | 🚫 GL-04 |
| AC5 — lock currency type after posting | Given a type set and the account has postings, When edited, Then currency type is read-only. | 🚫 GL-04 |

> Scope now = the currency-type **master + the GL dropdown wired to it** (`GlAccountMaster.CurrencyTypeId`
> → FK). EEFC flag, unrealised/realised forex G/L accounts, allowed currency and all enforcement
> (AC1–AC5) land with the GL-04 posting/revaluation engine.

---

## US-GL05-01 — Cost Centre Master & 3-level Hierarchy

**User story:** As a Finance Controller, I maintain a unit-wise cost-centre master with a
Plant (L1) → Department Group (L2) → Department (L3) hierarchy, a responsible manager and effective
dates, so costs can be tagged and rolled up at every level for departmental reporting.

**Pre-condition (seed):** the `COSTCENTRELEVEL` MiscType + its 3 level rows (`CCL1`/`CCL2`/`CCL3`,
SortOrder 1/2/3) must exist in the QA clone; level ids are resolved at runtime via
`miscmaster/by-name?MiscTypeCode=COSTCENTRELEVEL` (never hardcoded). L1 (Plant) is self-contained;
L2/L3 need a UserManagement Department Group / Department (resolved at runtime, self-skip if none).
`UnitId`+`CompanyId` come from the token (unit-wise). Enforcement (open-txn deactivation guard,
manager-alert routing, rollup) is journal-engine (Sprint 2) / reporting (FR-004) — those steps are 🚫.

Base route: `api/finance/CostCentre`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| Create → available in parent picker | Given a new L1 Plant is created, When GET `/by-name?level={L1}`, Then it is returned (offered to the L2 parent-CC picker, no code change). | ✅ implementable |
| Edit | Given an existing CC, When PUT with a new name, Then GET `/{id}` reflects it (code/level/plant immutable). | ✅ implementable |
| Deactivate | Given an active CC, When PUT with `isActive=0`, Then it is excluded from `/by-name` (picker) but still present in GetAll (`IsDeleted=0`). | ✅ implementable |
| AC1 — 3-level hierarchy + plant inheritance | Given an L2 created under an L1, Then it inherits the parent's plant and shows `parentCostCentreName`; the parent must be exactly one level above and in the same unit. | ⚠️ verify live (needs a Department Group) |
| AC2 — duplicate code rejected (per unit) | Given a code that exists in the unit, When saving another with that code, Then 400; the same code is allowed in a different unit. | ✅ implementable |
| AC3 — deactivation blocked by open transactions | Given a CC with open transactions in the current period, When deactivation is attempted, Then 400 "open transactions… close or reassign". | 🚫 journal engine (Sprint 2) — `HasOpenTransactionsAsync` stubbed to false |
| AC4 — manager change → alert routing | Given a CC with a responsible manager, When the manager changes, Then budget-alert routing updates to the new manager. | 🚫 needs Budget consumer |
| AC5 — rollup totals | Given department costs change, Then division & plant totals reflect the sum. | 🚫 reporting (FR-004) |

> Single self-referencing `Finance.CostCentre` table; `CentreLevelId` → `Finance.MiscMaster`
> (ordinal from the stable `SortOrder`, never the id). Code unique per `(UnitId, CostCentreCode)`;
> `UnitId`/`CompanyId` from the JWT. `ResponsibleManagerId` + effective dates are nullable/optional
> columns (FE adds the inputs later). See the **🌐 QA** suite `CostCentreQATests` for endpoint coverage.
