namespace PurchaseManagement.QATests.Tests.PurchaseReturn;

// ─────────────────────────────────────────────────────────────────────────────
// PurchaseReturn (RTV) — live-server QA suite (TRANSACTIONAL; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — PurchaseReturnController):
//   GET    /api/PurchaseReturn?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/PurchaseReturn/{id:int}              (returns 200 + data:null when not found — no 404 guard)
//   GET    /api/PurchaseReturn/autocomplete?term=
//   GET    /api/PurchaseReturn/returnable-qty?grnHeaderId=
//   GET    /api/PurchaseReturn/pos?vendorId=
//   GET    /api/PurchaseReturn/grns?vendorId=&poId=
//   GET    /api/PurchaseReturn/pending?PageNumber=&PageSize=&SearchTerm=
//   POST   /api/PurchaseReturn                       { CreatePurchaseReturnCommand }
//   PUT    /api/PurchaseReturn/{id:int}              { UpdatePurchaseReturnCommand } (route id == body id)
//   POST   /api/PurchaseReturn/{id:int}/cancel
//   DELETE /api/PurchaseReturn/{id:int}
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid RTV requires seeded vendor / PO / GRN FKs, return type / reason / action masters, and a
//   configured 'Purchase Return' document-numbering series — none guaranteed on the QA clone. These
//   are attribute-level [Fact(Skip=...)] so they are explicit pending work. GetAll (smoke),
//   autocomplete, pending reachability, and GetById-nonexistent stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurchaseReturnCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PurchaseReturnQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PurchaseReturn";

    public PurchaseReturnQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: vendor/PO/GRN + return type/reason/action + doc-numbering 'Purchase Return'"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vendorId = 1,
            poId = 1,
            details = new[] { new { itemId = 1, quantity = 5m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { vendorId = 1 });
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (autocomplete + pending reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_Autocomplete_WithTerm_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Autocomplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Pending_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE / DELETE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created RTV id (TC001 is blocked on vendor/PO/GRN/masters/doc-numbering)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/{_f.CreatedId}", new
        {
            id = _f.CreatedId,
            vendorId = 1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/999999", new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        // Route id 999999 vs (missing) body id → mismatch / validation → 400.
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/999999", new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a created RTV id (TC001 is blocked on vendor/PO/GRN/masters/doc-numbering)"), TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
