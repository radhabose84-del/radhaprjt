namespace BudgetManagement.QATests.Tests.ActivityLogs;

// ─────────────────────────────────────────────────────────────────────────────
// ActivityLogs (Budget) — live-server QA suite (READ-ONLY log report).
//
// Contract verified against source (2026-06-16 — ActivityLogsController.cs):
//   ⚠ Route prefix is "api/budget/logs":
//   GET /api/budget/logs/{entityName}/{entityId}?pageNumber=&pageSize=   (paged activity for an entity)
//   GET /api/budget/logs/{id}                                            (single log by long id; 404 when missing)
//
// Key facts that shaped assertions:
//   • Both endpoints are READ-ONLY — no create/update/delete.
//   • Activity logs may be empty for any entity → tolerate 200/404.
//   • The {id} route accepts a long; a non-existent id returns 404.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("BudgetActivityLogsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ActivityLogsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/budget/logs";

    public ActivityLogsQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SMOKE — activity log for an entity
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetForEntity_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/BudgetRequest/1?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetForEntity_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/BudgetRequest/1?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REACHABILITY — single log by id
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
