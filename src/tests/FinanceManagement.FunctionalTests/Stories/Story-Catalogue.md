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

## US-GL02-07 — Account Search / Type-ahead Component (reusable, all entry screens)

**User story:** As a journal-entry user, I want a fast type-ahead account search with filters, aliases and
favourites embedded in every entry screen, so that I find accounts instantly and can never pick an inactive one.

**Pre-condition (seed):** runs off the existing `Finance.GlAccountMaster` chart of accounts (company-scoped
by token) — no seed table needed for search. Favourites / recently-used persist in **MongoDB** collections
(`GlAccountFavourite` / `GlAccountRecentUse`), created on first write. The per-user ranking steps only show
data once the QA user has starred / selected accounts in that run.

Base route: `api/finance/glaccountmaster` (search/favourites/recent live under the GlAccountMaster owner).

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — search code/name/type/status | Given a COA, When GET `/search?term=110`, Then matches return code, name, type and active status (TOP-N, prefix-first). *(300ms target is a live/perf check, not asserted in code.)* | ⚠️ verify live |
| AC2 — inactive visible, not selectable | Given an inactive account matches, When results render, Then it is returned with `isActive=false` (FE greys + blocks select; `activeOnly=true` excludes it for entry fields; FR-001 validators reject on submit). | ✅ implementable (API returns flag) |
| AC3 — alias "yarn" | Given a user types `yarn`, When suggestions render, Then accounts whose **name / description / group / type** contains it are suggested. *(Arbitrary aliases present in no name/desc need a keyword store — DEFERRED.)* | ⚠️ partial |
| AC4 — favourites + recently-used first | Given the user has favourites / recent (Mongo), When the component opens (empty term) or searches, Then those shortcuts rank first. | ✅ implementable |
| AC5 — keyboard navigation | Given keyboard-only nav, When arrow + Enter, Then selection works without a mouse. | 🚫 FE-only (no API) |

> Zero-migration build: search/filter/status are pure reads off `GlAccountMaster` (+ AccountType/AccountGroup
> joins; group filter expands to the **subtree** via a recursive CTE). Per-user favourites + recently-used live in
> **MongoDB** (`IGlAccountUserPrefStore`), so no SQL table/column was added. Recently-used is **record-on-select**
> (`POST /recent`) — there is no journal/posting module yet to auto-capture usage. See `docs/AccountSearchTypeahead_HLD.md`.

---

## US-GL02-FR-008a — COA Freeze Engine & DB Triggers

**User story:** As a CFO, I want the Chart of Accounts locked at the database level once frozen, with a
visible freeze state and an automatic re-freeze, so that no structural change can slip through while the
books are sealed.

**Pre-condition (seed):** the migration `CoaFreezeTriggers` must be applied so the enforcement triggers
(`Finance.trg_GlAccountMaster_CoaFreeze`, `trg_AccountGroup_CoaFreeze`) exist (drives `dbTriggerActive`).
⚠️ Freezing locks the **whole company COA** in the shared QA clone, so the freeze/blocked-write steps are
guarded — they always re-open an unfreeze window in teardown. Auto-re-freeze needs BSOFT.Api's
`coa-auto-refreeze` recurring job running.

Base route: `api/finance/coa-freeze`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC2 — freeze banner | Given any COA screen, When GET `/state`, Then it returns the freeze state + `dbTriggerActive` + Total Accounts/Groups + Blocked Attempts. | ⚠️ verify live |
| AC1 — frozen write blocked | Given the COA is frozen, When a structural write (create account/group via API) is attempted, Then it is rejected — 400 "Chart of Accounts is frozen" (DB trigger rolled it back). | ⚠️ verify live (guarded — unfreezes after) |
| AC4 — enforcement at DB-trigger level | Given the engine, When inspected, Then `dbTriggerActive` is true (triggers exist + enabled in `sys.triggers`); enforcement is the trigger, not app code. | ⚠️ verify live |
| AC3 — auto-re-freeze on window expiry | Given an unfreeze window opened (`set-state isFrozen=false`), When it lapses, Then the `coa-auto-refreeze` Hangfire job returns the COA to frozen. | 🚫 needs Worker/Hangfire + wait |

> Enforcement is **DB triggers** (shipped in the `CoaFreezeTriggers` migration via `migrationBuilder.Sql`;
> entities declare `.HasTrigger(...)` so EF8 keeps saving). The flag is `Finance.CoaFreezeState` (one row/company);
> blocked attempts are logged to MongoDB (`CoaFreezeViolationLog`) by the `CoaFreezeViolationBehavior` safety-net.
> Governed dual-approval freeze/unfreeze (the mockup's Dual-Approval / Unfreeze-Requests / Change-Requests / Alerts
> tabs) is **US-GL02-08B**; the audit viewer is FR-009. See `docs/CoaFreezeEngine_HLD.md`.

---

## US-GL02-08B — COA Change-Request & Dual-Approval Unfreeze Workflow

**User story:** As a CFO, I want post-freeze changes to require a change request with an impact
assessment and a dual-approval unfreeze by two distinct people, fully logged, so any change to a
sealed COA is authorised and traceable. Drives the freeze state that US-GL02-08A enforces.

**Pre-condition (config):** the `CoaUnfreeze` section in `appsettings.json` must map `CfoRoleId`,
`SystemAdminRoleId`, `FcRoleId`, `InternalAuditRoleId` to real `AppSecurity.UserRole` IDs, and the
unfreeze/approve screen must be registered as a menu (so the `CanApprove` permission gate enforces).
Until then, role-gated steps refuse by design and are guarded as 🚫.

Base route: `api/finance/coa-change-request`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC5 — impact assessment required | Given a change request, When raised without an impact assessment, Then it is rejected (400); with one it lands in PendingImpactApproval, and the CFO must approve the impact before any unfreeze approval proceeds. | ✅ implementable (raise + 400); ⚠️ CFO impact-approve needs role config |
| AC3 — post-freeze change log | Given committed post-freeze changes, When GET `/post-freeze-log`, Then each row carries account, change type, BOTH approvers + timestamps, justification, flagged Post-Freeze; accounts also carry the flag in the GL listing. | ⚠️ verify live (report shape) |
| AC1 — distinct approvers | Given an unfreeze request, When the same person tries both approvals, Then it is blocked: "Dual approval required — approvers must be different users." | 🚫 needs RoleIds + dual-role login |
| AC2 — dual approval opens window + alerts | Given CFO + System Admin (distinct) approve, When the second approval lands, Then a time-boxed window opens (08A auto-re-freezes) and alerts go to CFO/FC/Internal Audit. | 🚫 needs RoleIds + Worker (email) |
| AC4 — lapse on auto-re-freeze | Given a window expires with incomplete change requests, When auto-re-freeze runs, Then those requests are cancelled as 'Lapsed' (by the `coa-lapse-expired-requests` job). | 🚫 needs Worker/Hangfire + wait |

> Workflow tables are `Finance.CoaChangeRequest` + `Finance.CoaUnfreezeRequest` (two distinct approver
> slots). Window-open drives 08A's `OpenUnfreezeWindowAsync`; post-freeze capture is the
> `CoaPostFreezeCaptureBehavior` (heuristic linkage). Alerts publish `CoaUnfreezeAlertEvent` → BackgroundService
> `CoaUnfreezeAlertConsumer`. The TEST/ADMIN `set-state` unfreeze branch is now blocked (governed seal = `/seal`).

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

---

## US-GL05-02 — Profit Centre Master & 2-level Hierarchy

**User story:** As a CFO, I maintain a profit-centre master for revenue segments
(Segment L1 → Sub-segment L2) linked to a responsible head, so PC-wise gross margin is reportable
each month. PC codes are unique **across all companies** (group segment reporting).

**Pre-condition (seed):** the `PROFITCENTRELEVEL` MiscType + its 2 level rows (`PCL1`/`PCL2`,
SortOrder 1/2) must exist in the QA clone; level ids are resolved at runtime via
`miscmaster/by-name?MiscTypeCode=PROFITCENTRELEVEL` (never hardcoded). The L1 (Segment) lifecycle is
self-contained (no cross-module FK). `CompanyId` comes from the token (owning company, stored for
audit only — uniqueness is global). `ResponsibleHeadId` is nullable/optional (resolved via `IUserLookup`).
Enforcement (current-FY-transaction deactivation guard, PC-mandatory tagging, PC P&L) is journal-engine
(Sprint 2) / FR-003 / FR-004 — those steps are 🚫.

Base route: `api/finance/ProfitCentre`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| Create → available in parent picker | Given a new L1 Segment is created, When GET `/by-name?level={L1}`, Then it is returned (offered to the L2 parent-segment picker, no code change). | ✅ implementable |
| Edit | Given an existing PC, When PUT with a new name, Then GET `/{id}` reflects it (code/level immutable). | ✅ implementable |
| Deactivate | Given an active PC, When PUT with `isActive=0`, Then it is excluded from `/by-name` (picker) but still present in GetAll (`IsDeleted=0`). | ✅ implementable |
| AC1 — segment links to responsible head | Given segments are created under a group, Then each links to a responsible head and is available for tagging. *(Head FK is optional/nullable until the FE wires the picker.)* | ⚠️ verify live |
| AC2 — code unique across companies | Given a PC code, When reused (in any company), Then it is rejected (400). | ✅ implementable |
| AC3 — hierarchy rolls up across families | Given an L2 Sub-segment under an L1 Segment, Then it shows `parentProfitCentreName` (rolls up to its segment). | ✅ implementable |
| AC4 — mid-year add note | Given a new PC added mid-year (with a justification), When created, Then an audit note records that prior transactions cannot be retro-tagged. | ✅ implementable (audit log) |
| AC5 — deactivation blocked by current-FY transactions | Given a PC with current-year transactions, When deactivation is attempted, Then 400 "blocked until year-end close". | 🚫 journal engine (Sprint 2) — `HasCurrentYearTransactionsAsync` stubbed to false |

> Single self-referencing `Finance.ProfitCentre` table; `LevelId` → `Finance.MiscMaster`
> (ordinal from the stable `SortOrder`, never the id). Code unique **globally** (`ProfitCentreCode`,
> across all companies); `CompanyId` from the JWT (audit only). PC codes allow hyphens
> (`^[A-Za-z0-9-]+$`) so `PC-SPIN-001` validates. `ResponsibleHeadId` is nullable/optional. See the
> **🌐 QA** suite `ProfitCentreQATests` for endpoint coverage.

---

## US-GL01-02 — Voucher Type Configuration (per-type series · allowed account types · FY reset)

**User story:** As a System Administrator, I configure voucher types each with their own dedicated
number series, allowed account types and number padding, so finance can introduce new document types
without a code change.

**Pre-condition (seed):** `Finance.AccountTypeMaster` is seeded (Asset/Liability/… for `CompanyId=1`)
and at least one UserManagement `FinancialYear` exists in the QA clone — both FK ids are resolved at
runtime (`accounttypemaster?CompanyId=1` first row, `/api/FinancialYear` first row), never hardcoded;
the create + series steps self-skip if neither resolves. The **Type-Lock on a saved voucher** (AC3)
depends on the voucher-entry transaction table (separate feature) and is `[Fact(Skip=…)]`. Approval
threshold / approver role are **out of scope** (removed by design).

Base route: `api/finance/VoucherTypeMaster`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC4 — add type, no deployment | Given the config screen, When a new type is created (POST), Then it appears in `/by-name` + GetAll immediately, with no code change/deployment. | ✅ implementable |
| AC1 — own dedicated series | Given a new type, When GET `/number-series?FinancialYearId=`, Then its row shows a next number formatted from its own `VoucherTypeCode`+padding starting at `…/0001`. | ✅ implementable |
| Edit | Given an existing type, When PUT (name/padding/account types), Then GET `/{id}` reflects it (code immutable). | ✅ implementable |
| AC2 — FY reset | Given a consumed series, When POST `/reset-series {voucherTypeId, financialYearId}`, Then the counter returns to `…/0001`. | ✅ implementable |
| Deactivate | Given an active type, When PUT `isActive=0`, Then it is excluded from `/by-name` (selectable list) but still present in GetAll (`IsDeleted=0`). | ✅ implementable |
| AC3 — saved-voucher type lock | Given a posted voucher of a type, When its type change is attempted, Then blocked. | 🚫 needs voucher-entry table |

> Standalone 3-table master (`VoucherTypeMaster` + `VoucherTypeAccountType` → `AccountTypeMaster` +
> `VoucherTypeNumberSeries` per `FinancialYearId`); does NOT touch `TransactionTypeMaster`/`DocumentSequence`.
> `VoucherTypeCode` doubles as the series prefix. See the **🌐 QA** suite `VoucherTypeMasterQATests` for
> endpoint coverage and `docs/VoucherType_Specification.md`.

---

## US-GL02-09 — Account Master Audit Trail & Version History

**User story:** As an auditor, I want an immutable per-account change history (who, when, field, old, new,
role, IP), viewable and exportable and retained 8 years, so every structural change to the chart of
accounts is traceable for statutory audit.

**Pre-condition (build):** the QA server must run the US-GL02-09 build (the `AccountAuditTrail` interceptor
+ role-in-JWT). The `role` field is populated only after a fresh login on that build. The trail is written
**only** by the SaveChanges interceptor when an `IAuditTrailed` master is changed — there is no write API.
The story exercises it via `AccountTypeMaster` (audited, and free of the COA-freeze trigger).

Base route: `api/finance/account-audit` (read-only: `GET {entityName}/{entityId}`, `GET /export`).

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — change captured with full context | Given an audited master field is edited, When saved, Then a row (account, field, old, new, user, **role**, timestamp, IP) is written in the same transaction. | ⚠️ verify live (role needs fresh login on the new build) |
| AC2 — DELETE/UPDATE rejected by DB | Given any app role incl. System Admin, When UPDATE/DELETE on the audit table is attempted, Then a DB constraint rejects it. | 🚫 no API surface — covered by integration (`AccountAuditTrailImmutabilityTests`) |
| AC3 — chronological field-level history | Given an auditor opens an account, When GET `/{entityName}/{entityId}`, Then all field-level changes list chronologically. | ✅ implementable |
| AC4 — export with checksum < 30s | Given a date range, When GET `/export?from=&to=&entityName=`, Then a payload with a record-count checksum (+ SHA-256) returns within 30s. | ✅ implementable |
| AC5 — 8-year retention | Given the retention policy, When records age, Then they remain (no purge job; immutability trigger prevents deletion). | 🚫 operational guarantee — not runtime-testable |

> New hardened table `Finance.AccountAuditTrail` (separate from the mutable `Finance.ActivityLog`).
> Capture = `AccountAuditTrailSaveChangesInterceptor` (Insert/Update/Delete, in-transaction). Immutability =
> `DENY DELETE,UPDATE TO public` + `trg_AccountAuditTrail_Immutable` (in the migration). Scope = COA
> structure + governance (GlAccountMaster, AccountGroup, AccountTypeMaster, CoaFreezeState,
> CoaChangeRequest, CoaUnfreezeRequest). See `docs/AccountAuditTrail_Specification.md`.

---

## US-GL03-01 — Fiscal Year & Period Calendar Setup

**User story:** As a System Administrator, I configure company-specific fiscal calendars that auto-generate
12 monthly periods plus Period 13 (adjustment), so each entity can close on its own cycle and the posting
engine has a calendar to validate every JE against.

**Pre-condition (seed):** `Finance.MiscMaster` rows for `MiscTypeCode='FYS'` (`OPEN`, `CLOSED`) and
`MiscTypeCode='FPS'` (`OPEN`, `SOFTCLOSED`, `HARDCLOSED`) must exist in the QA clone before the Create
endpoint will succeed. The handler throws a clear error if they're missing.

Base routes: `api/finance/FinancialYearMaster`, `api/finance/FinancialYearMaster/{companyId}/periods`,
`api/finance/FinancialYearMaster/period-for-date`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — independent calendars per company | Given two companies onboard with different fiscal-year starts, When each creates their year, Then each has its own 13-period set scoped by `CompanyId`. | 🚫 needs a second QA user bound to a real CompanyId (cross-company assert blocked) |
| AC2 — 13 periods auto-generated | Given a fiscal year `2100-01`, When POST `/api/finance/FinancialYearMaster` is OK, Then GET `/{id}` shows 12 monthly + Period 13 (`IsAdjustmentPeriod=true`) all `Status=OPEN`. | ✅ implementable |
| AC3 — Period 13 same dates as Period 12 | Given Period 13 (Indian accounting convention), When examined, Then its StartDate/EndDate equal Period 12 (March) — adjustment posts to the close month. | ✅ implementable |
| AC4 — duplicate `(CompanyId, FinancialYearCode)` rejected | Given a year already exists, When POSTed again with the same code, Then 400 "already exists". | ✅ implementable |
| AC4b — date-range overlap rejected | Given a year covers Apr-Mar, When a new year overlaps any day in that range, Then 400 "overlaps an existing fiscal year". | ✅ implementable |
| AC5 — auto-create next year before EndDate | Given a year ends within 3 months, When the Hangfire `AutoCreateNextFinancialYearMasterJob` runs, Then the next year + its 13 periods are created. | 🚫 background-job — cannot be triggered via the QA HTTP surface; verified by unit test |
| Posting engine — read APIs | Given a calendar exists, When GET `/{companyId}/periods` and GET `/period-for-date?date=`, Then both return the calendar correctly (date-resolver skips `IsAdjustmentPeriod` so March dates resolve to Period 12, not Period 13). | ✅ implementable |
| Update — date immutability | Given an existing year, When PUT updates `StartDate`/`EndDate`, Then those values are silently kept at create-time values (handler accepts only code + IsActive). | ⚠️ verify live |
| Delete — soft-delete cascades to periods | Given a year, When DELETE, Then `IsDeleted=1` propagates to all 13 periods (period FK is `Cascade` so the rows are tombstoned via the year). | ✅ implementable |

> Schema: `Finance.FinancialYearMaster` (header) + `Finance.FinancialPeriodMaster` (detail, `Cascade` on year delete).
> Status FK → `Finance.MiscMaster` keyed by `(MiscTypeCode, Code)`. See unit tests in
> `FinanceManagement.UnitTests/Application/FinancialYearMaster/Commands/CreateFinancialYearMasterCommandHandlerTests` for the
> 13-period generation contract.

---

## US-GL03-02 — Period Status & Posting Controls

**User story:** As a Finance Manager, I enforce three-state one-way period status (Open → SoftClosed → HardClosed)
at both the API and DB layers, so postings are controlled as the close progresses; reversals are gated behind
CFO + SysAdmin dual approval and an automatic second-approval auto-apply.

**Pre-condition (seed):** `Finance.MiscMaster` rows for `MiscTypeCode='PSO'` (`PENDING`, `FULLYAPPROVED`,
`APPLIED`, `REJECTED`) must exist (in addition to the FYS/FPS rows from US-GL03-01). The `testsales` QA user
must hold the `CFO` and `SysAdmin` roles in the QA clone for the dual-approval flow to land green; absent that,
approval steps Skip with a precise reason.

Base routes: `api/finance/FinancialPeriodStatus`, `api/finance/PeriodStatusOverride`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — HARDCLOSED blocks everyone | Given a hard-closed period, When any source posts a JE, Then it's rejected by both the API gate (`IPeriodPostingGate`) and the DB trigger on `Finance.JournalLine`. | 🚫 needs `Finance.JournalLine` table (GL-01-FR-009) — DB trigger is a no-op until JournalLine exists |
| AC2 — SOFTCLOSED restricts to Finance Manager+ | Given a soft-closed period, When a Finance Manager posts, Then 200; for any role below, Then 403/blocked. | 🚫 needs JournalLine + role-gated test users |
| AC3 — reversal needs CFO + SysAdmin | Given a period reversal request (`SOFTCLOSED → OPEN` or `HARDCLOSED → SOFTCLOSED`), When both CFO **and** SysAdmin approve (any order), Then the period status auto-flips immediately on the 2nd approval; single approval keeps it PENDING. | ⚠️ verify live (needs the dual-role test user) |
| AC4 — `PeriodStatusChangedDomainEvent` published on every flip | Given any successful status change, When committed, Then the event fires (`IsReversal=true` for the override path, `OverrideId` populated) and `GET /{periodId}/status` reflects the new status. | ✅ implementable |
| State machine — forward-only | Given current status, When a forward transition is attempted, Then `OPEN→SOFTCLOSED` and `SOFTCLOSED→HARDCLOSED` are 200, every other forward direction is 400 "Illegal status transition". | ✅ implementable |
| Override — single pending at a time | Given a PENDING override on a period, When a 2nd reversal is requested, Then 400 "already in progress". | ✅ implementable |
| Override — segregation of duties | Given an override Requested by user X, When user X attempts `/approve` or `/reject`, Then 400 "self-approve" / "self-reject". | ⚠️ verify live (needs a separate test user for approvers) |

> Schema: `Finance.PeriodStatusOverride` + `LastStatusChangedBy/At` columns on `Finance.FinancialPeriodMaster`.
> `IPeriodPostingGate` is the single security service consumed by GL-01-FR-009 posting middleware.
> Status / Override-status FKs → `Finance.MiscMaster` (`FPS`, `PSO`). State machine lives in
> `FinanceManagement.Application.Common.PeriodStatus.PeriodStatusStateMachine`. The auto-apply path
> (CFO + SysAdmin → period flip + override APPLIED) is atomic in one transaction
> (`PeriodStatusOverrideCommandRepository.ApplyPeriodStatusChangeAsync`) — proven in the integration
> tests under `FinanceManagement.IntegrationTests/Repositories/PeriodStatusOverride`.

---

## US-GL02-10 — Multi-Company COA (shared global template + entity-specific overrides)

**User story:** As a Group Finance Controller, I want a global COA shared across entities with
entity-specific overrides and company-restricted accounts, so group consistency is maintained while
each entity keeps its specific accounts.

**Pre-condition (seed):** `MultiCompanyCoa:TemplateCompanyId` configured to the group company, and
≥2 companies sharing one `Company.EntityId` on the QA clone. Deep propagation (AC3) and restricted-
posting (AC2) also need a coherent GL/journal seed, so they are exercised by the integration suite
(`GlAccountMasterMultiCompanyTests`), not live. Model: per-company copies of `Finance.GlAccountMaster`
linked by `GlobalAccountId`; `IsGlobal` / `IsCompanyRestricted` / `IsLocalOverride` flags.

Base route: `api/finance/glaccountmaster`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — new subsidiary inherits template | Given a global COA, When POST `/inherit-global/{companyId}`, Then the subsidiary gains a linked copy of each non-restricted global account (idempotent; entity-specific accounts may also be added directly). | ⚠️ verify live (idempotent no-op without template config) |
| AC2 — restricted account not postable cross-entity | Given a company-restricted account in entity A, When a user in entity B posts a journal line to it, Then 400 "restricted to another company". | 🚫 needs restricted account + cross-entity journal seed (integration-covered) |
| AC3 — global change propagates except overrides | Given a global account with subsidiary copies, When the template is edited, Then non-overridden copies update and `IsLocalOverride` copies are skipped. | 🚫 needs template config + ≥2 companies (integration-covered) |
| AC4 — consistency report flags single-entity accounts | Given the entity group, When GET `/consistency-report`, Then account codes present in only one company are returned, each with a flag (e.g. 'in Processing only'). | ✅ implementable (reachable; rows depend on data) |
| AC5 — mandatory, profile-scoped company selector | Given the account screen, When GET `/selectable-companies`, Then only the user's assigned companies are returned; create blocks server-side when no active company is in session. | ✅ implementable |

> **Implementation note:** Approach = global template + per-company copies (not shared rows), layered on
> the existing per-company `GlAccountMaster`. Entity group = companies sharing `Company.EntityId`.
> Propagation/inheritance via `IGlobalCoaPropagationService`. Config: `MultiCompanyCoa:TemplateCompanyId`
> (0 = feature dormant). See memory `project_multicompany_coa_10`.

---

## US-GL02-15 — COA Listing & Structure Reports (read-only + PDF export)

**User story:** As a Finance Controller / Auditor, I want a COA listing with hierarchy and attributes,
an account-usage report, an FS-mapping validation report, and PDF export, so I can review structure
and submit a clean COA to auditors.

**Pre-condition (seed):** Read-only over existing COA / journal / Schedule III data; company from
session. No configuration changes. PDF via QuestPDF (in-process). Posted = MiscMaster
JOURNAL_STATUS/POSTED; balance-sheet = Schedule III Section.StatementType code 'BS'; balance from
`Finance.LedgerBalance` (SUM(DrTotal-CrTotal)). AC3's BS-with-balance exclusion is data-specific and
proven by the integration suite.

Base route: `api/finance/coa-report`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — listing PDF with hierarchy + attributes | Given the COA, When GET `/listing/pdf`, Then a `%PDF` (application/pdf) renders (<10s target). | ✅ implementable (timing: ⚠️ verify live) |
| AC2 — usage report performance | Given ~2,000 accounts, When GET `/account-usage`, Then it completes (<30s target). | ✅ implementable (timing: ⚠️ verify live) |
| AC3 — exclude BS accounts with non-zero balance | Given a 'no posting in 12 months' run, When evaluated, Then balance-sheet accounts with a non-zero balance are NOT deactivation candidates. | 🚫 needs seeded stale BS+balance (integration-covered) |
| AC4 — FS-mapping shows zero/lists remaining | Given the validator, When GET `/fs-mapping-validation`, Then `isClean` iff `unmappedCount = 0`, else the unmapped leaf groups are listed. | ✅ implementable |
| AC5 — PDF suitable for auditor submission | Given the listing, When exported, Then the PDF carries company, generated-on, hierarchy indentation, attributes, posting counts and page numbers. | ✅ implementable |

> **Implementation note:** Read-only Dapper aggregates (`CoaReportQueryRepository`) mirroring the
> LedgerBalance read pattern; posting counts pre-aggregated on `JournalDetail.GlAccountId`. PDF =
> `CoaListingPdfBuilder` (QuestPDF Community). Endpoints: `/listing`, `/listing/pdf`, `/account-usage`,
> `/fs-mapping-validation`. No migration. See memory `project_coa_reports_15`.


---

## US-GL03-04 — Backdating Controls & Late-Posting Report

**User story:** As a Finance Controller, I need to know — and gate — every backdated journal entry,
so prior-month accruals can be booked when justified, the auditor can drill into who/when/why, and
the CFO sees a weekly summary instead of a thousand journal lines. Backdating cannot be hidden from
the report by a client-side flag flip because `IsBackdated` is computed by the DB.

**Pre-condition (seed):** `Finance.JournalHeader` already exists. The migration adds 4 columns
(`IsBackdated` persisted-computed bit, `BackdateReason` varchar(500), `BackdateAcknowledgedBy` int,
`BackdateAcknowledgedAt` datetimeoffset) + the composite index `IX_JournalHeader_IsBackdated`
on `(IsBackdated, IsDeleted, CompanyId)`. The weekly digest job needs `BackdateDigest:CfoRoleId`
+ `FcRoleId` configured in `appsettings.json` (binds via `BackdateDigestOptions`); if both are 0
the job logs and skips, so no functional failure mode.

Base routes:
- Report: `GET /api/finance/Journal/late-posting-report`
- Enforcement: invoked by the posting handler (GL-01 FR-009) via `IBackdateEnforcementService`
- Weekly digest: Hangfire recurring job `finance-weekly-backdated-journal-digest`
  (queue `finance-jobs`, cron `0 8 * * 1` = Mondays 08:00 UTC).

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — auto-detect backdating | Given any posted voucher, When `VoucherDate < CAST(PostedAt AS DATE)`, Then `IsBackdated = 1` regardless of API payload (DB-computed persisted column). | ✅ implementable (integration-covered) |
| AC2 — mandatory reason for SoftClosed | Given a posting whose `VoucherDate` falls inside a SOFTCLOSED period, When `BackdateReason` is null/whitespace, Then `IBackdateEnforcementService` returns `ReasonRequired()` and the posting handler must reject 400. | 🚫 needs posting handler wiring (GL-01 FR-009) |
| AC3 — late-posting report endpoint | Given backdated postings exist for the session company, When GET `/late-posting-report`, Then a paginated grid of every `IsBackdated = 1` voucher is returned (validator-enforced sort allow-list: PostedAt / VoucherDate / DaysBackdated). | ✅ implementable |
| AC4 — weekly CFO digest | Given the recurring job fires Mondays 08:00 UTC, When backdated postings exist for the last 7 days, Then a per-company digest email is sent to each CFO + FC recipient (no email when no rows). | ⚠️ verify live (needs SMTP + role grants) |
| Sort allow-list (SQLi defence) | Given an arbitrary string in `SortBy` / `SortDirection`, When the report is called, Then 400 — the Dapper repo only concatenates values that survive `GetLatePostingReportQueryValidator`. | ✅ implementable |
| Filtered-index strategy | Given the report's `WHERE IsBackdated = 1 AND IsDeleted = 0` plus company narrowing, Then `IX_JournalHeader_IsBackdated` seeks (composite — SQL Server forbids filtered indexes on computed columns, even persisted ones). | ✅ implementable |

> **Implementation note:** `IBackdateEnforcementService` is a pure read against the new
> `FinancialPeriodMaster` (3-state OPEN/SOFTCLOSED/HARDCLOSED, US-GL03-01). HARDCLOSED is left to
> `IPeriodPostingGate` (US-GL03-02) — we don't duplicate the rejection. The persisted computed column
> means a client cannot ship `isBackdated=false` in the payload to dodge the report. The Hangfire job
> lives in `BackgroundService.Infrastructure.Jobs.WeeklyBackdatedJournalDigestJob` and uses the
> existing `IRoleUserLookup` (US-GL02-08B pattern) + `SendEmailCommand` channel.

---

## US-GL02-16 — COA Read API (downstream modules)

**User story:** As a downstream-module developer (AP/AR/FA), I want a REST read API to query the
account master (get-by-code, search by type/group, validate-for-posting) with deactivation events,
so Phase-2 modules integrate reliably.

**Pre-condition (seed):** Read-only over GlAccountMaster; company from JWT. The deactivation event is
published via direct bus publish (sub-second) with the Finance transactional outbox as a durable
fallback. No write endpoints, no migration.

Base route: `api/finance/coa`.

| # | Acceptance Criterion (Given / When / Then) | Tag |
|---|---|---|
| AC1 — single lookup < 100ms | Given a code, When GET `/accounts/by-code/{code}`, Then it responds < 100ms (unique CompanyId+AccountCode index). | ✅ implementable (timing: ⚠️ verify live; <100ms proven in integration) |
| AC2 — validate-for-posting fails with reason | Given an inactive account or a currency mismatch (or missing mandatory cost centre), When GET `/accounts/validate-for-posting`, Then `isValid=false` with `reasons[]`. | ✅ implementable |
| AC3 — deactivation event within 1s | Given an account goes active→inactive, When saved, Then `GlAccountDeactivatedEvent` is published within 1s (direct bus + outbox fallback). | 🚫 async/bus — no HTTP surface (unit-covered) |
| AC4 — authenticated + logged | Given any call, Then it requires a token (401 without) and writes an audit log. | ✅ implementable |
| AC5 — search returns status | Given a type/group search, When GET `/accounts`, Then matching accounts return with `isActive`. | ✅ implementable |

> **Implementation note:** Read-only Dapper repo `CoaReadQueryRepository` (get-by-code on the unique
> index for <100ms); `ValidateForPostingQuery` assembles active + currency + cost-centre rules; the
> deactivation event is published from `UpdateGlAccountMasterCommandHandler` on the active→inactive
> transition via `IIntegrationEventPublisher` (direct `IPublishEndpoint`, Finance outbox fallback).
> Event contract: `Contracts.Events.Coa.GlAccountDeactivatedEvent`. See memory `project_coa_read_api_16`.
