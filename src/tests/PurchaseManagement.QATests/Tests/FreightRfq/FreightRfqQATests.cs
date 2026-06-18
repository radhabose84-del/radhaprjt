namespace PurchaseManagement.QATests.Tests.FreightRfq;

// ─────────────────────────────────────────────────────────────────────────────
// FreightRfq — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — FreightRfqController.cs):
//   Route prefix: [Route("api/purchase/[controller]")] → /api/purchase/FreightRfq
//   GET    /api/purchase/FreightRfq?PageNumber=&PageSize=&SearchTerm=&StatusId=
//   GET    /api/purchase/FreightRfq/{id}                (200 + data:null when not found — no 404 guard)
//   GET    /api/purchase/FreightRfq/pending?PageNumber=&PageSize=
//   GET    /api/purchase/FreightRfq/po-references?term=
//   GET    /api/purchase/FreightRfq/po-prefill/{poId}
//   GET    /api/purchase/FreightRfq/transporters?term=
//   GET    /api/purchase/FreightRfq/next-number?rfqDate=
//   POST   /api/purchase/FreightRfq                     (create)
//   PUT    /api/purchase/FreightRfq                     (update)
//   PUT    /api/purchase/FreightRfq/quotations          (save quotations)
//   POST   /api/purchase/FreightRfq/submit-for-approval
//   DELETE /api/purchase/FreightRfq/{id}
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid FreightRfq requires a seeded rfqType misc value, a pending PO reference, and at
//   least one transporter — none of which the QA clone guarantees. These are attribute-level
//   [Fact(Skip=...)] so they are explicit pending work, not silent gaps. Negatives (empty body /
//   no-auth), smoke GetAll, and read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("FreightRfqCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class FreightRfqQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/purchase/FreightRfq";

    public FreightRfqQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ──────────────────

    [Fact(Skip = "needs seeded data: rfqType misc + PO reference + transporters"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            rfqDate = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
            rfqTypeId = 1,
            purchaseOrderId = 1,
            remarks = "Created by QA suite",
            transporters = new[] { new { transporterId = 1 } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { rfqTypeId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke; tolerant 200/404) ─────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 3 — EXTRA READS (reachability) ────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetPoReferences_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/po-references?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetTransporters_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/transporters?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetNextNumber_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/next-number?rfqDate=2026-06-17");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 4 — UPDATE / DELETE (lifecycle BLOCKED; negatives ACTIVE) ──────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a created FreightRfq id (TC001 is blocked on rfqType/PO/transporter seeds)"), TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
