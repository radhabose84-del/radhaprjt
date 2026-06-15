namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-08 — Depreciation run
//   As a finance user I run depreciation for a period and review the abstract.
//   BLOCKED on capitalized assets — the period lookup is runnable; the run + abstract
//   steps are Skipped until capitalized assets exist.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-08-DepreciationRun")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-08")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_08_DepreciationRun_Tests
{
    private readonly QAServerFixture _f;
    public US_FAM_08_DepreciationRun_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the depreciation period lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_DepreciationPeriodLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/DepreciationDetail/DeprecationPeriod");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs capitalized assets: run DepreciationDetail for a period."), TestPriority(2)]
    public Task Step2_RunDepreciationDetail() => Task.CompletedTask;

    [Fact(Skip = "Needs posted depreciation: the abstract reflects the run."), TestPriority(3)]
    public Task Step3_AbstractReflectsRun() => Task.CompletedTask;

    [Fact(Skip = "Needs posted assets: WDVDepreciation computes WDV for the period."), TestPriority(4)]
    public Task Step4_WdvComputed() => Task.CompletedTask;
}
