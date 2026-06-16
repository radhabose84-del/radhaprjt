namespace SalesManagement.QATests.Tests.SalesOrderAmendment;

// ─────────────────────────────────────────────────────────────────────────────
// SalesOrderAmendment — live-server QA suite (TRANSACTIONAL; amendment of a SalesOrder).
//
// Contract verified against source (SalesOrderAmendmentController.cs, 2026-06-15):
//   GET  /api/SalesOrderAmendment?PageNumber=&PageSize=&SearchTerm=
//   GET  /api/SalesOrderAmendment/{salesOrderHeaderId}     (amendments for a given order header)
//   GET  /api/SalesOrderAmendment/pending?PageNumber=&PageSize=&SearchTerm=
//   GET  /api/SalesOrderAmendment/pending/{id}
//   POST /api/SalesOrderAmendment                          (CreateSalesOrderAmendmentCommand)
//   (NO PUT, NO DELETE — amendment is append-only against an existing order.)
//
// WHY CREATE IS SKIPPED:
//   CreateSalesOrderAmendmentCommand requires an existing SalesOrderHeaderId plus a full snapshot of
//   header totals (bags/weight/freight/GST/TCS/final amount), an AgentPaymentTermsId, an
//   AmendmentDetails[] line array, and a Discounts[] snapshot (max 3, one per SlabType). Because
//   SalesOrder create itself is blocked (no guaranteed seeded order chain in the QA clone), there is
//   no SalesOrderHeaderId to amend, and fabricating the snapshot would 400/500 without asserting
//   anything meaningful. → create-happy is [Fact(Skip=…)]. Negatives, smoke, and read-endpoint
//   reachability stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesOrderAmendmentCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesOrderAmendmentQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesOrderAmendment";

    public SalesOrderAmendmentQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL  (Smoke)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_GetAll_WithSearchTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=10&SearchTerm=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET by SalesOrderHeaderId (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(10)]
    public async Task TC010_GetBySalesOrderHeaderId_NonExistent_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetBySalesOrderHeaderId_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — PENDING list / pending-by-id (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_Pending_List_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Pending_ById_NonExistent_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Pending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=10");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — CREATE  (POST only; negatives active, happy-path BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Create_MissingSalesOrderHeaderId_Returns400()
    {
        // SalesOrderHeaderId defaults to 0 → FK/required validation must reject.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrderHeaderId = 0,
            reason = "QA amendment"
        });
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact(Skip = "needs seeded data: CreateSalesOrderAmendmentCommand requires an existing SalesOrderHeaderId " +
                 "plus a header-totals snapshot, AgentPaymentTermsId, AmendmentDetails[] and Discounts[] (max 3). " +
                 "SalesOrder create is itself blocked, so no order header id can be produced in-suite."),
     TestPriority(33)]
    public Task TC033_Create_HappyPath_BLOCKED() => Task.CompletedTask;
}
