namespace PurchaseManagement.QATests.Tests.QuotationCompare;

// ─────────────────────────────────────────────────────────────────────────────
// QuotationCompare — live-server QA suite (TRANSACTIONAL; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — QuotationCompareController):
//   GET    /api/QuotationCompare/comparison/{rfqId}   (returns 404 when no comparison for the RFQ)
//   POST   /api/QuotationCompare                        { createQuoteComparsion } — body deserializes to
//                                                         CreateQuoteComparsionCommand (top-level command)
//   GET    /api/QuotationCompare/pending?pageNumber=&pageSize=&searchTerm=
//   GET    /api/QuotationCompare/{id:int}
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid comparison requires a seeded RFQ plus the quotations to compare against it — neither
//   guaranteed on the QA clone. These are attribute-level [Fact(Skip=...)] so they are explicit
//   pending work. The pending list (smoke), no-auth, empty-body POST, and comparison/{rfqId} +
//   GET /{id} reachability stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QuotationCompareCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class QuotationCompareQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/QuotationCompare";

    public QuotationCompareQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: RFQ + quotations to compare"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            createQuoteComparsion = new
            {
                rfqId = 1,
                quotationIds = new[] { 1, 2 }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            createQuoteComparsion = new { rfqId = 1 }
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — PENDING LIST  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPending_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetPending_Page2_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=2&pageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (comparison/{rfqId} + GET /{id} reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Comparison_ByRfqId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/comparison/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Comparison_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/comparison/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
