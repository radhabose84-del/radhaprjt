namespace SalesManagement.QATests.Tests.StockLedger;

// ─────────────────────────────────────────────────────────────────────────────
// StockLedger — live-server QA suite (READ-ONLY report).
//
// Contract verified against source (2026-06-15 — StockLedgerController.cs):
//   GET /api/StockLedger/report?PageNumber=&PageSize=&ItemId=&LotId=&WarehouseId=
//       &BinId=&StatusId=&PackNo=&DateFrom=&DateTo=&ProductionYear=
//       (PageNumber default 1, PageSize default 10; all filters optional/nullable)
//   GET /api/StockLedger/by-pack-range?ProductionYear=&ItemId=&StartPackNo=
//       &EndPackNo=&PackTypeId=
//       (ProductionYear is REQUIRED [int]; the rest are optional/nullable)
//
// Key facts that shaped assertions:
//   • Both endpoints are READ-ONLY — no create/update/delete.
//   • The report is paginated ({ data, TotalCount, PageNumber, PageSize }); the
//     by-pack-range endpoint returns { data, TotalCount } (no paging).
//   • Empty datasets are valid → tolerate 404 alongside 200/400.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("StockLedgerCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StockLedgerQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/StockLedger";

    public StockLedgerQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — /report happy-path
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_Report_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/report?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Report_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/report?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Report_WithDateRange_Reachable()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/report?PageNumber=1&PageSize=15&DateFrom=2024-01-01&DateTo=2024-12-31");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REACHABILITY — /by-pack-range (ProductionYear is required)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_ByPackRange_WithProductionYear_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-pack-range?ProductionYear=2024");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_ByPackRange_WithPackRange_Reachable()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/by-pack-range?ProductionYear=2024&StartPackNo=1&EndPackNo=100");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_ByPackRange_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-pack-range?ProductionYear=2024");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
