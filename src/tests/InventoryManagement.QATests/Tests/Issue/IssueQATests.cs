namespace InventoryManagement.QATests.Tests.Issue;

// ─────────────────────────────────────────────────────────────────────────────
// Issue (material issue entry) — live-server QA suite (TRANSACTIONAL; create SKIPPED).
//
// Contract verified against source (2026-06-17 — IssueController.cs):
//   ⚠ Route prefix is "api/inventory/[controller]" → /api/inventory/Issue
//   POST   /api/inventory/Issue                                  ([FromBody] CreateIssueEntryCommand — nested issueDetails)
//   GET    /api/inventory/Issue/{mrsId}                          (pending issues for an MRS; 404 when none)
//   GET    /api/inventory/Issue/IssueEntryPendingHeaders?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/inventory/Issue/by-name?mrsNo=                   (approved MRS lookup)
//   NO update / delete.
//
// Why create is SKIPPED:
//   A valid issue entry requires an approved MRS header plus on-hand warehouse stock — neither
//   guaranteed on the QA clone. Attribute-level [Fact(Skip=...)].
//   IssueEntryPendingHeaders (smoke), by-name + pending-by-mrsId reachability, and negatives ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("IssueEntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class IssueQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/inventory/Issue";

    public IssueQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: approved MRS header + warehouse stock"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            mrsId = 1,
            issueDetails = new[]
            {
                new { itemId = _f.ActiveItemId, quantity = 1m, warehouseId = 1 }
            }
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — PENDING HEADERS  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_PendingHeaders_HappyPath_Returns200()
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
    public async Task TC021_PendingHeaders_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/IssueEntryPendingHeaders?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (by-name + pending-by-mrsId reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_ByName_WithMrsNo_Reachable_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?mrsNo=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_ByName_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?mrsNo=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetPendingByMrsId_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
