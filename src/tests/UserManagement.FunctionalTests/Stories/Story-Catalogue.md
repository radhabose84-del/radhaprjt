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

## US-UM-03 — Navigation & RBAC scaffolding

> **As** an application administrator
> **I want** to register a Module, add a Menu under it, and wire role entitlements
> **so that** the navigation tree and role-based access reflect the app structure.

**Workflow under test:** create a Module → create a Menu referencing that module → read the menu
back; then exercise the role-entitlement / role-allocation surface.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 03.1 | A **Module** is created → returns a new id | ✅ [intended] | `POST /api/Modules` `{moduleName}` |
| 03.2 | A **Menu** is created referencing `moduleId` (parentId=0 root) → new id | ✅ [intended] | `POST /api/Menu` `{menuName,moduleId,menuUrl,parentId,sortOrder}` |
| 03.3 | The menu is reachable via `by-name?name=&moduleId=` | ⚠️ [verify] | autocomplete filter semantics |
| 03.4 | `POST /by-module` / `/by-parent` return the navigation tree for the module | ⚠️ [verify] | `List<int>` body lookups |
| 03.5 | **RoleEntitlements** create wires modules/menus/privileges to a role | 🚫 [blocked] | nested RBAC DTO needs seeded role/module/menu ids |
| 03.6 | **UserRoleAllocation** assigns roles to a user (bulk roleIds) | 🚫 [blocked] | needs a seeded user + role ids |
| 03.7 | Teardown leaf-first (menu → module) | ✅ [intended] | DELETE `/{id}` route binding |

---

## US-UM-04 — Reference master setup

> **As** an administrator
> **I want** to set up location/station/icon reference masters
> **so that** other modules can reference them.

**Workflow under test:** create Location, Station, IconMaster; verify read-back, deactivate-excludes-from-autocomplete, and that TimeZones (read-only) is reachable.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 04.1 | A **Location** is created (immutable code) → new id | ✅ [intended] | `POST /api/usermanagement/Location` |
| 04.2 | A **Station** is created (immutable code) → new id | ✅ [intended] | `POST /api/Station` |
| 04.3 | An **IconMaster** is created (keyword immutable) → new id | ✅ [intended] | `POST /api/IconMaster` |
| 04.4 | Each is readable by id | ✅ [intended] | non-company-scoped reads |
| 04.5 | Deactivating the Location/Station excludes it from `by-name` autocomplete, keeps it in GetAll | ⚠️ [verify] | autocomplete `IsActive=1` filter |
| 04.6 | **TimeZones** (read-only) list is reachable | ✅ [intended] | `GET /api/TimeZones` |
| 04.7 | Teardown removes the created masters | ✅ [intended] | DELETE `/{id}` |

---

## US-UM-05 — Security policy configuration

> **As** a security administrator
> **I want** to define password-complexity, admin-security and company-settings policies
> **so that** authentication is governed by the configured rules.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 05.1 | A **PasswordComplexityRule** can be created and read back | ✅ [intended] | `POST /api/PasswordComplexityRule` |
| 05.2 | An **AdminSecuritySettings** record can be created (all int/byte fields) | ⚠️ [verify] | create returns 201 with no id body → singleton-ish; verify behaviour |
| 05.3 | **CompanySettings** can be created for a company (currency/language/timezone/financialYear FKs) | 🚫 [blocked] | needs a valid FK combo for `testsales` |
| 05.4 | The current CompanySettings singleton is readable | ✅ [intended] | `GET /api/CompanySettings` |
| 05.5 | Teardown removes the created policy rows where supported | ⚠️ [verify] | some settings have no delete |

---

## US-UM-06 — User onboarding & access  *(BLOCKED — needs seeded FK chain)*

> **As** an administrator
> **I want** to create an Entity, onboard a User under it, allocate roles, and drive the password flow
> **so that** the user can log in with the right access.

| # | Acceptance criterion | Tag | Notes / source |
|---|---|---|---|
| 06.1 | An **Entity** is created (entityCode auto-generated) → new id | ✅ [intended] | `POST /api/Entity` |
| 06.2 | A **User** is created under the entity (5 nested arrays + group/dept FKs) | 🚫 [blocked] | needs seeded userGroup/department/role/unit/division ids |
| 06.3 | Roles are allocated to the user | 🚫 [blocked] | depends on 06.2 |
| 06.4 | First-time password set / reset-request / reset flow works | 🚫 [blocked] | depends on a real created user; reset endpoints are `[AllowAnonymous]` (reachability only) |
| 06.5 | The Entity is readable by id and lists its companies | ✅ [intended] | `GET /api/Entity/{id}`, `/{id}/companies` |

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
