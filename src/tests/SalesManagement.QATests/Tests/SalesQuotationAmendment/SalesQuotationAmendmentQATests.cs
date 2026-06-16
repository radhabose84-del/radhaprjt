namespace SalesManagement.QATests.Tests.SalesQuotationAmendment;

// ─────────────────────────────────────────────────────────────────────────────
// SalesQuotationAmendment — live-server QA suite (POST-only transactional entity).
//
// Contract verified against source (2026-06-15):
//   POST   /api/SalesQuotationAmendment            CreateSalesQuotationAmendmentCommand
//   GET    /api/SalesQuotationAmendment?PageNumber=&PageSize=&SearchTerm=     (GetAll, paged)
//   GET    /api/SalesQuotationAmendment/{salesQuotationHeaderId}              (by header id)
//   GET    /api/SalesQuotationAmendment/pending?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesQuotationAmendment/pending/{id}
//   (NO PUT / NO DELETE — controller exposes POST + GETs only.)
//
// Create payload:
//   salesQuotationHeaderId (the source quotation MUST be in Approved state),
//   reason?, freightCharges, otherCharges, totalBasicAmount, totalDiscount,
//   netTaxableAmount, totalTax, grandTotal,
//   amendmentDetails:[{ salesQuotationDetailId, newItemId?, newQuantity?, newExMillRate?,
//                       newDiscount?, newHSNId?, newTaxPercentage?, netRate, totalAmount, taxAmount }]
//
// BLOCKED: create-happy needs an Approved SalesQuotation header + its detail rows — the QA
//   clone does not guarantee an approved quotation, so it is [Fact(Skip=...)].
//   Negatives (empty body, no-auth), GetAll smoke and the by-header-id / pending reachability
//   checks stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesQuotationAmendmentCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesQuotationAmendmentQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesQuotationAmendment";

    public SalesQuotationAmendmentQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED — needs an Approved source quotation)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: an Approved SalesQuotation header with detail rows to amend"), TestPriority(1)]
    public Task TC001_Create_HappyPath_Returns200_And_CapturesId() => Task.CompletedTask;

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            salesQuotationHeaderId = 1,
            reason = "QA amendment",
            amendmentDetails = Array.Empty<object>()
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingRequired_Returns400()
    {
        // salesQuotationHeaderId default 0 → required/FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesQuotationHeaderId = 0,
            reason = "QA amendment",
            amendmentDetails = Array.Empty<object>()
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke)
    // ─────────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY HEADER ID  (reachability — 200/400/404 tolerant)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetByHeaderId_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetByHeaderId_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — GET PENDING  (reachability — 200/400/404 tolerant)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_GetPending_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_GetPendingById_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
