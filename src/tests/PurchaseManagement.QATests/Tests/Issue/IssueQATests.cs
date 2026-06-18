namespace PurchaseManagement.QATests.Tests.Issue;

// ─────────────────────────────────────────────────────────────────────────────
// Issue (Purchase) — live-server QA suite (TRANSACTIONAL; create-happy SKIPPED).
//
// Contract verified against source (2026-06-17 — Issue/IssueController.cs,
// [Route("api/purchase/[controller]")] => /api/purchase/Issue):
//   POST   /api/purchase/Issue                                    CreateIssueEntryCommand
//   GET    /api/purchase/Issue/{mrsId}                            (pending issue details for an MRS)
//   GET    /api/purchase/Issue/IssueEntryPendingHeaders?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/purchase/Issue/by-name?mrsNo=                     (approved-MRS lookup)
//
// Why create-happy is SKIPPED:
//   A valid Issue needs an approved MRS + available stock + document-numbering — none
//   guaranteed on the QA clone. Primary smoke list is IssueEntryPendingHeaders; negatives /
//   reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurIssueCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class IssueQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/Issue";

    public IssueQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: approved MRS + stock + doc-numbering"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            mrsId = 1,
            issueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            issueDetails = new[] { new { itemId = 1, quantity = 5m } }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { mrsId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — IssueEntryPendingHeaders (smoke; tolerant 200/404) ──────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPendingHeaders_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/IssueEntryPendingHeaders?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetPendingHeaders_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/IssueEntryPendingHeaders?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 3 — EXTRA READS (reachability; tolerant) ───────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_ByName_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?mrsNo=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?mrsNo=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetByMrsId_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
