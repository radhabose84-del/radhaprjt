namespace PurchaseManagement.QATests.Tests.QuotationEntry;

// ─────────────────────────────────────────────────────────────────────────────
// QuotationEntry — live-server QA suite (TRANSACTIONAL; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — QuotationEntryController):
//   GET    /api/QuotationEntry/GetAll?pageNumber=&pageSize=&searchTerm=
//   GET    /api/QuotationEntry/id?id=               ⚠️ LITERAL 'id' PATH + ?id= QUERY (route is [HttpGet("id")],
//                                                      action param `int id` binds from the ?id= query string)
//   GET    /api/QuotationEntry/autocomplete?search=  (param is 'search', NOT 'term')
//   POST   /api/QuotationEntry                       { CreateQuotationCommand } (returns 200 envelope)
//   PUT    /api/QuotationEntry/id                    { UpdateQuotationCommand } (route is [HttpPut("id")])
//   GET    /api/QuotationEntry/change-logs/{id:int}
//   POST   /api/QuotationEntry/upload-logo           (multipart)
//   DELETE /api/QuotationEntry/delete-logo
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid quotation requires a seeded RFQ + supplier + items, with a manually-supplied
//   quotationNumber — none guaranteed on the QA clone. These are attribute-level [Fact(Skip=...)] so
//   they are explicit pending work. GetAll (smoke), autocomplete reachability, no-auth and
//   empty-body POST stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QuotationEntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class QuotationEntryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/QuotationEntry";

    public QuotationEntryQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: RFQ + supplier + items (manual quotationNumber)"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            quotationNumber = _f.EntityCode[..10],
            rfqId = 1,
            supplierId = 1,
            details = new[] { new { itemId = 1, rate = 100m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { rfqId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAll?pageNumber=1&pageSize=15");
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetAll?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAll?pageNumber=2&pageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (autocomplete + GetById via ?id= reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Autocomplete_WithSearch_Returns200Or400Or404()
    {
        // ⚠️ param is 'search', not 'term'.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?search=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Autocomplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?search=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        // ⚠️ literal 'id' path segment + ?id= query for the action's int id param.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/id?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created quotation id (TC001 is blocked on RFQ/supplier/items)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/id", new { id = _f.CreatedId, rfqId = 1 });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/id", new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/id", new { });
        await QAHelper.Assert400Async(resp);
    }
}
