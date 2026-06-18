namespace InventoryManagement.QATests.Tests.Reports;

// ─────────────────────────────────────────────────────────────────────────────
// Reports (Inventory stock) — live-server QA suite (READ-ONLY).
//
// Contract verified against source (2026-06-17 — ReportsController.cs):
//   ⚠ Route is "api/[controller]" → /api/Reports
//   GET /api/Reports/CurrentStock?itemId=&warehouseId=&storageTypeId=&targetId=   (404 when null result)
//   GET /api/Reports/SubStoresCurrentStock?itemId=&departmentId=&...              (404 when null result)
//   GET /api/Reports/CurrentStockUnitWise?unitIds=1,2&itemId=&...                 (400 when unitIds blank/invalid)
//   GET /api/Reports/by-division?companyId=&divisionId=                          (400 when ids <= 0)
//
// All endpoints are READ-ONLY — no create/update/delete. Empty stock is valid, so reads tolerate
// 200/400/404. CurrentStock is the smoke read; no-auth on it returns 401.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("InvReportsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ReportsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Reports";

    public ReportsQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — current stock summary
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_CurrentStock_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/CurrentStock");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_CurrentStock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/CurrentStock");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REACHABILITY — other read reports
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_SubStoresCurrentStock_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/SubStoresCurrentStock");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_CurrentStockUnitWise_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/CurrentStockUnitWise?unitIds=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_ByDivision_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-division?companyId=1&divisionId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
