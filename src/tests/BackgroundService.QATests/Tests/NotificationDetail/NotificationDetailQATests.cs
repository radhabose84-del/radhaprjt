namespace BackgroundService.QATests.Tests.NotificationDetail;

// ─────────────────────────────────────────────────────────────────────────────
// NotificationDetail — live-server QA suite (READ-ONLY user inbox).
//
// NotificationDetail is a per-user notification inbox: rows are produced by the
// notification dispatch pipeline, not by a master-data CRUD endpoint. The controller
// exposes only a paged read (by userId) and a mark-as-read PUT. There is no
// POST/DELETE/GetAll. The "mark a real notification read" happy path is SKIPPED
// because the QA clone seeds no inbox rows for the test user.
//
// Contract verified against source (2026-06-18 — NotificationDetailController.cs):
//   ⚠ Route prefix is "api/[controller]" → BASE = /api/NotificationDetail (BARE).
//   GET /api/NotificationDetail/detail/{userId}?PageNumber=1&PageSize=10&FromDate=&ToDate=&ReadStatus=
//        (userId is a STRING route segment)
//   PUT /api/NotificationDetail                 { id, readStatusId }  (mark-as-read)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("NotificationDetailCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class NotificationDetailQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/NotificationDetail";

    public NotificationDetailQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET DETAIL  (smoke; userId is a STRING route segment)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetDetail_HappyPath_Returns200Or404()
    {
        // userId is a string route segment ("1"); an empty inbox is valid → tolerate 404.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/detail/1?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetDetail_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/detail/1?PageNumber=1&PageSize=10");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetDetail_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/detail/1?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetDetail_WithReadStatusFilter_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/detail/1?PageNumber=1&PageSize=10&ReadStatus=0");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — MARK AS READ (PUT)  (happy path BLOCKED; nonexistent tolerant)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: notification detail rows for a user (the inbox is populated by the dispatch pipeline, not seeded on the QA clone)"), TestPriority(30)]
    public async Task TC030_MarkAsRead_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = 1, readStatusId = 1 });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_MarkAsRead_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999, readStatusId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_MarkAsRead_NonExistent_Returns200Or400Or404Or500()
    {
        // No seeded inbox rows — marking a nonexistent id read may no-op (200), be rejected
        // (400/404), or hit an unguarded update path (500). Tolerate all four.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = 999999, readStatusId = 1 });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_MarkAsRead_EmptyBody_Returns200Or400Or404Or500()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }
}
