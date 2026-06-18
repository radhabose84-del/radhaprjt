namespace PurchaseManagement.QATests.Tests.SES;

// ─────────────────────────────────────────────────────────────────────────────
// SES (Service Entry Sheet) — live-server QA suite (TRANSACTIONAL; create SKIPPED).
//
// Contract verified against source (2026-06-17 — SESController.cs):
//   Route prefix: [Route("api/[controller]")] → /api/SES
//   GET    /api/SES/ses/list?pageNumber=&pageSize=&searchTerm=
//   GET    /api/SES/ses/{id}                            (404 when not found)
//   GET    /api/SES/ses/approval?fromDate=&toDate=&vendorId=
//   GET    /api/SES/approved-list
//   GET    /api/SES/{poId}/service-header               (404 when not found)
//   GET    /api/SES/{poId}/lines
//   POST   /api/SES/ses                                 (create)
//   PUT    /api/SES/service-entry-sheets                (update)
//   GET    /api/SES/create-source?purchaseOrderId=&scheduleNo=&serviceItemId=
//   GET    /api/SES/ses/details?sesId=
//
// Why create is SKIPPED:
//   A valid SES requires an approved service PO, a service schedule, and a service config —
//   none guaranteed on the QA clone. Attribute-level [Fact(Skip=...)]. Negatives, smoke list,
//   and read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SESCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SESQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SES";

    public SESQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ──────────────────

    [Fact(Skip = "needs seeded data: approved service PO + schedule + service config"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/ses", new
        {
            purchaseOrderId = 1,
            scheduleNo = 1,
            serviceItemId = 1,
            sesDate = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
            lines = new[] { new { serviceItemId = 1, quantity = 1m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/ses", new { purchaseOrderId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/ses", new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — SES LIST (smoke; tolerant 200/404) ────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetList_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/ses/list?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetList_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/ses/list?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 3 — EXTRA READS (reachability) ────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_ApprovedList_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/approved-list");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_SesApproval_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/ses/approval");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetSesById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/ses/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetSesById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/ses/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
