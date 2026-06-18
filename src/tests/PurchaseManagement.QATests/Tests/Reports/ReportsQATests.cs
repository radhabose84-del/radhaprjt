namespace PurchaseManagement.QATests.Tests.Reports;

// ─────────────────────────────────────────────────────────────────────────────
// Reports (Purchase) — live-server QA suite (READ-ONLY stock reports; reachability).
//
// Contract verified against source (2026-06-17 — ReportsController.cs):
//   Route prefix: [Route("api/purchase/[controller]")] → /api/purchase/Reports
//   GET    /api/purchase/Reports/CurrentStock?itemId=&warehouseId=&storageTypeId=&targetId=
//   GET    /api/purchase/Reports/SubStoresCurrentStock?itemId=&departmentId=&warehouseId=&storageTypeId=&targetId=
//
// Why reachability only:
//   Both are read-only aggregated stock reports; an empty result legitimately yields 404. We
//   assert reachability tolerantly (200/400/404), not payload.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurReportsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ReportsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/Reports";

    public ReportsQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_CurrentStock_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/CurrentStock");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_CurrentStock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/CurrentStock");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_SubStoresCurrentStock_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/SubStoresCurrentStock");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
