# UserManagement — Functional Story Catalogue

**Status:** 🔴 DRAFT — awaiting review. **No test code is written until this is approved.**
**Module:** UserManagement (legacy — no original user stories existed)
**Source:** Reverse-engineered from controllers/handlers/validators + the `UserManagement.QATests` suite.

---

## How to read this

These stories are **reconstructed from how the code behaves today**, not from a written spec.
Each acceptance criterion (AC) is tagged so the reviewer can separate *real intent* from
*legacy accidents* before we lock any behaviour into a test:

| Tag | Meaning | Action on review |
|---|---|---|
| ✅ **[intended]** | Clearly the designed business behaviour | Keep — becomes a test assertion |
| ⚠️ **[verify]** | Works today but may be a legacy quirk/bug, not intent | **You decide:** assert-as-is, assert-the-correct-behaviour, or drop |
| 🚫 **[blocked]** | Cannot be asserted with the current test setup | Note the blocker; test skipped/deferred |

> **Scope reminder (test pyramid):** these are **workflow** tests — multi-step business
> journeys. Field-level CRUD/validation is already covered by `UserManagement.QATests`
> and is **not** repeated here.

---

## ⚙️ Shared constraint that shapes both stories — `testsales` company = 0

The QA test user `testsales` logs in with **CompanyId claim = 0**. Many UserManagement reads
are **company-scoped** to the JWT company claim (`GetAll`, autocomplete, `units-by-division`,
division/unit lists filtered by `UserDivision`/`UserUnit`). Rows created against
`companyId = 1` are therefore **invisible** to those company-scoped reads under `testsales`.

**Consequence for functional tests:** a workflow must be verified by **chaining the
create-response IDs** and reading back through **non-company-scoped endpoints** (e.g.
`GET /api/Division/{id}` — confirmed *not* company-scoped after the recent `GetByIdAsync`
fix). Any assertion that depends on a **company-scoped list** is marked 🚫 **[blocked]**
until a **real-company test user** exists.

> **Reviewer decision needed:** do we (a) accept the chained-ID verification for now, or
> (b) provision a `testsales`-style user bound to a real CompanyId so company-scoped reads
> can also be asserted? (b) unlocks several ⚠️/🚫 ACs below.

---

## US-UM-01 — Organisation hierarchy setup

> **As** a company administrator
> **I want** to build the organisation hierarchy (Company → Division → Unit → Department)
> **so that** transactions and users can later be scoped to the right org node.

**Workflow under test:** create a Company, then a Division under it, then a Unit, then a
Department — each referencing its parent — and prove the parent/child links and the
"can't remove a parent that has children" rule.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 01.1 | A **Company** is created with a unique PAN/code → returns a new id | ✅ [intended] | `POST /api/Company`; PAN must match strict Indian PAN regex (4th char = entity type) |
| 01.2 | A **Division** is created referencing `companyId` → returns a new id | ✅ [intended] | `POST /api/Division` `{shortName,name,companyId}` |
| 01.3 | A **Unit** is created referencing the company/division | ✅ [intended] | `POST /api/Unit` (nested address/contact DTOs default to `new()`) |
| 01.4 | A **Department** is created under the org | ✅ [intended] | `POST /api/Department` |
| 01.5 | The created Division is readable by id and echoes the `companyId` link | ✅ [intended] | `GET /api/Division/{id}` — **not** company-scoped (verified) |
| 01.6 | A Company/Division **list** shows the new rows | 🚫 [blocked] | company-scoped read + `testsales` company=0 → invisible. Needs real-company user |
| 01.7 | **Inactivating** a Division that has child Units is **rejected** with "linked with other records" | ⚠️ [verify] | dependent-block rule exists (Rule 25); confirm it actually fires for Division→Unit and that the message/`400` is the intended contract |
| 01.8 | **Deleting** a parent that has live children is **blocked** (soft-delete validation) | ⚠️ [verify] | confirm which parent/child pairs enforce this vs. silently allow (Division delete currently has **no** pre-query → always 200; that may be a gap) |
| 01.9 | `GET /api/Division/{id}` on a **soft-deleted** division returns `200` with `data: null` | ⚠️ [verify] | current behaviour (no null-guard in GetById). Intended, or should it be `404`? |

**Open questions for reviewer (US-UM-01):**
- 01.7/01.8: Is the dependent-block rule meant to apply to **every** parent→child pair in the
  org chain, or only some? The code enforces it unevenly (Division delete has no guard).
- 01.9: `200 + null` for a deleted/non-existent id — accept as the contract, or is `404` the intent?

---

## US-UM-02 — Access control setup

> **As** a security administrator
> **I want** to define a Role, attach access policy / item-group mappings, and assign it to a user
> **so that** the user gets exactly the permissions the role grants.

**Workflow under test:** create a Role, create/attach an AccessPolicy + RoleItemGroupMapping,
and prove the "role linked to mappings/users can't be silently removed" rule.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 02.1 | A **Role** (UserRole) is created → returns a new id | ✅ [intended] | `POST /api/UserRole` |
| 02.2 | An **AccessPolicy** is created/attached for the role | ✅ [intended] | `POST /api/AccessPolicy` (see `AccessPolicyQATests` for shape) |
| 02.3 | A **RoleItemGroupMapping** links the role to item groups | ✅ [intended] | mappings are soft-deleted with the role on delete (handler confirmed) |
| 02.4 | **Deleting the Role** soft-deletes the role **and** its RoleItemGroupMappings together | ✅ [intended] | `DeleteRoleCommandHandler` cascades mapping soft-delete |
| 02.5 | Deleting a **non-existent / already-deleted** role is an idempotent no-op (`<=0`, controller `200`, no audit) | ✅ [intended] | confirmed via recent idempotent-delete fix |
| 02.6 | Assigning the role to a user changes that **user's effective permissions** | 🚫 [blocked] | true permission-effect needs a **second login as the affected user** to read their effective rights — not assertable as `testsales`. Deferred |

**Open questions for reviewer (US-UM-02):**
- 02.6: Do we want real permission-effect coverage (requires a disposable user we can log in
  as), or is verifying the **setup chain + cascade-delete rule** (02.1–02.5) sufficient for now?
- Confirm the exact `AccessPolicy` create payload to chain (I'll lift it from `AccessPolicyQATests`).

---

## Proposed test surface once approved (preview — not yet written)

| Story | Test class | Approx. steps | Verifies |
|---|---|---|---|
| US-UM-01 | `Workflows/US_UM_01_OrgSetup_Tests.cs` | ~6–8 ordered | 01.1–01.5 (chained IDs) + 01.7 dependent-block; 01.6 skipped [blocked] |
| US-UM-02 | `Workflows/US_UM_02_AccessControl_Tests.cs` | ~5 ordered | 02.1–02.5 (setup + cascade delete); 02.6 skipped [blocked] |

Every class is tagged `[Trait("Story", "US-UM-0x")]`, `[Trait("Module","UserManagement")]`,
`[Trait("Owner","<dev>")]` and uses `[TestPriority]` so steps run in order on one fixture.

---

## ✋ Review gate

Please mark each AC as **keep / change / drop**, and answer the open questions (esp. the
**company=0** decision and **02.6** permission-effect scope). I will only write the two test
classes after this sign-off, so no legacy quirk gets frozen into a test by accident.
