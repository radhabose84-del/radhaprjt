namespace PurchaseManagement.QATests.Tests.ActivityLogs;

// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogs (Purchase) — live-server QA suite (READ-ONLY log report; reachability).
//
// Contract verified against source (2026-06-17 — ActivityLogsController.cs):
//   Route prefix: [Route("api/purchase/logs")] → /api/purchase/logs
//   GET    /api/purchase/logs/{entityName}/{entityId:int}?pageNumber=&pageSize=
//   GET    /api/purchase/logs/{id:long}                      (404 when not found)
//
// Why reachability only:
//   Read-only activity-log feed. The clone has no guaranteed log rows, so we assert reachability
//   tolerantly (200/404), not payload. Note both routes share the prefix; the {entityName}/{id}
//   form takes a string segment, the {id:long} form a single numeric segment.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurActivityLogsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ActivityLogsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/logs";

    public ActivityLogsQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetByEntity_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/PurchaseIndent/1?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByEntity_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/PurchaseIndent/1?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
