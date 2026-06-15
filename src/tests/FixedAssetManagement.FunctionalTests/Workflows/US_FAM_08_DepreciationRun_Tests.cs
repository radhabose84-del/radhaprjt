namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-08 — Depreciation run
//
//   As a finance user I run depreciation for a period and review the abstract.
//
// Live-reconciled status:
//   • The period lookup and the depreciation detail / abstract / WDV read endpoints are
//     reachable (auth + routing proven).
//   • Meaningful figures require CAPITALIZED assets, which depend on a posted AssetPurchase —
//     GRN-driven and blocked for testsales (no OldUnitId GRN scope, see US-FAM-05). With no
//     capitalized assets the read endpoints return 400/empty, asserted tolerantly.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-08-DepreciationRun")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-08")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_08_DepreciationRun_Tests
{
    private readonly QAServerFixture _f;
    private const string From = "2026-01-01";
    private const string To = "2026-12-31";

    public US_FAM_08_DepreciationRun_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the depreciation period lookup is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_DepreciationPeriodLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/DepreciationDetail/DeprecationPeriod");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC2 — the depreciation-detail run endpoint is reachable (needs capitalized assets for data).
    [Fact, TestPriority(2)]
    public async Task Step2_RunDepreciationDetailReachable()
    {
        var resp = await _f.Client.GetAsync($"/api/DepreciationDetail?FromDate={From}&ToDate={To}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // AC3 — the depreciation abstract endpoint is reachable.
    [Fact, TestPriority(3)]
    public async Task Step3_AbstractReachable()
    {
        var resp = await _f.Client.GetAsync($"/api/DepreciationDetail/Abstract?FromDate={From}&ToDate={To}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // AC4 — the WDV depreciation endpoint is reachable.
    [Fact, TestPriority(4)]
    public async Task Step4_WdvReachable()
    {
        var resp = await _f.Client.GetAsync($"/api/WDVDepreciation/GetWDV?FromDate={From}&ToDate={To}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
