namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-11 — Fixed-asset dashboard & reporting (read-only)
//
//   As a fixed-asset manager I open the dashboard summaries and reports to monitor
//   assets, expiries, transfers and audits.
//
// READ-ONLY story: exercises the Dashboard (3) and Report (3) endpoints with safe params.
// Data-dependent reports return 404 on an empty dataset, asserted tolerantly.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-11-DashboardReporting")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-11")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_11_DashboardReporting_Tests
{
    private readonly QAServerFixture _f;

    private const string DashboardRoute = "/api/fam/Dashboard";
    private const string ReportRoute = "/api/fam/Report";
    private const string From = "2026-01-01";
    private const string To = "2026-12-31";

    public US_FAM_11_DashboardReporting_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the dashboard summaries (card / asset / expiry) are reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_DashboardsReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/card-dashboard?fromDate={From}&toDate={To}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/Asset-summary?fromDate={From}&toDate={To}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/AssetExpiry-summary?fromDate={From}&toDate={To}"));
    }

    // AC2 — the asset report (always 200, empty list when no data) is reachable.
    [Fact, TestPriority(2)]
    public async Task Step2_AssetReportReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/AssetReport?fromDate={From}&toDate={To}"));
    }

    // AC3 — the data-dependent reports are reachable (404 on empty dataset).
    [Fact, TestPriority(3)]
    public async Task Step3_DataDependentReportsReachable()
    {
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/AssetTransferReport?FromDate={From}&ToDate={To}")).StatusCode)
            .Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/AssetAuditReport?auditTypeId=1")).StatusCode)
            .Should().BeOneOf(200, 404);
    }
}
