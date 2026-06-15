# Functional Tests — process & conventions

Functional tests verify **end-to-end business workflows** through the live `BSOFT.Api`
(multi-step journeys), reusing `Shared.QAInfrastructure` (live server, `testsales` auth,
isolated `BannariERP_QATest` clone). They are **selective** (test-pyramid top): a handful of
high-value journeys per module — they do **not** re-test field-level CRUD (that's `*.QATests`).

This `UserManagement.FunctionalTests` project is the **template** every other module copies.

---

## One project per module, one class per story

```
src/tests/{Module}.FunctionalTests/
├── Stories/Story-Catalogue.md      # the stories (review gate before any test code)
├── Fixtures/FunctionalCollections.cs
├── Workflows/
│   ├── US_<MOD>_01_<Name>_Tests.cs # ONE test class per story
│   └── US_<MOD>_02_<Name>_Tests.cs
└── README.md
```

- **One test class per story** → two developers on different stories edit different files →
  **no merge conflicts**.
- Steps within a story run in order via `[TestPriority(n)]` on a single shared
  `QAServerFixture` (one login, one run-unique `EntityCode`).

## Story IDs & traits (traceability + CI routing)

Every workflow class carries three traits and a story-ID class name:

```csharp
[Collection("US-UM-01-OrgSetup")]
[Trait("Module", "UserManagement")]
[Trait("Story",  "US-UM-01")]
[Trait("Owner",  "radha")]          // developer who owns the story/ticket
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_UM_01_OrgSetup_Tests { ... }
```

Run a slice by any dimension:

```bash
dotnet test --filter "Story=US-UM-01"
dotnet test --filter "Module=UserManagement"
dotnet test --filter "Owner=radha"
```

CI groups failures by `Module` / `Story` / `Owner` to route the failed-TC report to the
right developer.

## Story-ID convention

`US-<MODULE>-<NN>` — e.g. `US-UM-01`, `US-PUR-03`. The same ID appears in the **catalogue
row**, the **class name**, and the **`Story` trait** → one ticket ↔ one class ↔ one CI row.

---

## Workflow for a NEW module (when real stories exist)

Legacy UserManagement had no stories, so they were reverse-engineered and reviewed. For
modules that **do** have stories (Purchase, etc.), the flow is:

1. **PM/BA** assigns a story (Jira/backlog) → a developer. Acceptance criteria are already
   signed off in the ticket.
2. The **assigned developer** adds `US_<MOD>_NN_<Name>_Tests.cs` to their module's
   `*.FunctionalTests` project, tagged with their `Owner` trait.
3. PR review checks **test ↔ acceptance-criteria** match (not reverse-engineered intent —
   the ticket is the source of truth).
4. CI runs all functional tests post-deploy; failures route to the `Owner`.

Different stories = different files in the same project → parallel developers never collide.

---

## Running locally

Same prerequisites as `UserManagement.QATests` (see memory `qa-run-setup-and-status`):

1. Start `BSOFT.Api` against the isolated clone: `./run-qa.ps1` (repo root).
2. Reset the clone before a run: `src/tests/qa-clone-reset.sql` (SQLCMD Mode OFF).
3. `dotnet test src/tests/UserManagement.FunctionalTests`
   - report: add `--logger "html;LogFileName=functional.html" --results-directory qa-reports`

`appsettings.QA.json` points at `http://localhost:5239` locally; CI overrides `QAServer:BaseUrl`
to the deployed QA URL (Gitea phase).
