# MaintenanceManagement.FunctionalTests

Workflow / acceptance tests for the MaintenanceManagement module. They sit **above** the
per-entity CRUD QA tests (`MaintenanceManagement.QATests`) and verify multi-step business
workflows against the live BSOFT.Api (`testsales` auth, isolated QA clone).

## Conventions (multi-developer model)
- **One test class per story**, named `US_MNT_<NN>_<Slug>_Tests`, in `Workflows/`.
- Each story carries `[Trait("Story","US-MNT-NN")]`, `[Trait("Module","MaintenanceManagement")]`,
  `[Trait("Owner","<dev>")]` so CI can route failures and you can run a slice:
  `dotnet test --filter "Story=US-MNT-01"`.
- Steps within a story are ordered with `[TestPriority(n)]` + the shared `PriorityOrderer`
  over a single `QAServerFixture`; cross-step ids are held in `private static` fields.
- Every story is described first in `Stories/Story-Catalogue.md` (the review gate).

## Running (needs the live API; the user runs it)
```
dotnet test src/tests/MaintenanceManagement.FunctionalTests
dotnet test src/tests/MaintenanceManagement.FunctionalTests --filter "Story=US-MNT-01"
```
