# FixedAssetManagement.FunctionalTests

Workflow / acceptance tests for the FixedAssetManagement module. They sit **above** the
per-entity CRUD QA tests (`FixedAssetManagement.QATests`) in the pyramid and verify
multi-step business workflows against the live BSOFT.Api (`testsales` auth, isolated QA clone).

## Conventions (multi-developer model)
- **One test class per story**, named `US_FAM_<NN>_<Slug>_Tests`, in `Workflows/`.
- Each story carries `[Trait("Story","US-FAM-NN")]`, `[Trait("Module","FixedAssetManagement")]`,
  `[Trait("Owner","<dev>")]` so CI can route failures to the owner and you can run a slice:
  `dotnet test --filter "Story=US-FAM-01"`.
- Steps within a story are ordered with `[TestPriority(n)]` + the shared `PriorityOrderer`,
  over a single `QAServerFixture`. xUnit makes a new instance per test, so cross-step ids are
  held in `private static` fields.
- Every story is described first in `Stories/Story-Catalogue.md` (the review gate); ACs are
  tagged ✅/⚠️/🚫 before the test assertions are trusted.

## Running (needs the live API; the user runs it)
```
dotnet test src/tests/FixedAssetManagement.FunctionalTests
dotnet test src/tests/FixedAssetManagement.FunctionalTests --filter "Story=US-FAM-01"
```
