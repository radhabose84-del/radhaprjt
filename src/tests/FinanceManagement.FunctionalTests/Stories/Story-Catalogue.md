# FinanceManagement — Functional Story Catalogue

Review gate: approve a story's AC table here **before** trusting its test code.
Tags: ✅ [implementable] · ⚠️ [verify live] · 🚫 [blocked — needs seeded data].

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
