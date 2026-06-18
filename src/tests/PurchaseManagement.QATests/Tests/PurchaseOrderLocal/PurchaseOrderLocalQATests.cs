namespace PurchaseManagement.QATests.Tests.PurchaseOrderLocal;

// ─────────────────────────────────────────────────────────────────────────────
// PurchaseOrderLocal — live-server QA suite (TRANSACTIONAL; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — PurchaseOrderLocalController):
//   POST   /api/PurchaseOrderLocal                       { PurchaseOrderCreateDto }
//   PUT    /api/PurchaseOrderLocal                       { PurchaseOrderUpdateDto }
//   POST   /api/PurchaseOrderLocal/amendment             { PurchaseOrderUpdateDto }
//   GET    /api/PurchaseOrderLocal?pageNumber=&pageSize=&searchTerm=&poMethodId=&statusId=&budgetGroupId=
//   GET    /api/PurchaseOrderLocal/analysis?...
//   GET    /api/PurchaseOrderLocal/{id:int}              (returns 200 + StatusCode:404 in body when not found)
//   GET    /api/PurchaseOrderLocal/{id:int}/detail
//   GET    /api/PurchaseOrderLocal/autocomplete?term=&poMethodId=&budgetGroupId=
//   GET    /api/PurchaseOrderLocal/pending-po?poId=&PoMethodId=&pageNumber=&pageSize=&searchTerm=
//   GET    /api/PurchaseOrderLocal/total-purchase-value?date=  (date REQUIRED → 400 without it)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid local PO requires seeded vendor / item / paymentTerm / incoterm FKs, an active budget,
//   and a configured 'Purchase Order' document-numbering series — none guaranteed on the QA clone.
//   These are attribute-level [Fact(Skip=...)] so they are explicit pending work, not silent gaps.
//   Reads (GetAll smoke, autocomplete, pending-po, GetById-nonexistent) and negatives stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurchaseOrderLocalCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PurchaseOrderLocalQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PurchaseOrderLocal";

    public PurchaseOrderLocalQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: vendor/item/paymentTerm/incoterm + budget + doc-numbering 'Purchase Order'"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vendorId = 1,
            poMethodId = 1,
            details = new[] { new { itemId = 1, quantity = 10m, rate = 100m } }
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_FilteredByPoMethodAndStatus_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=10&poMethodId=1&statusId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (autocomplete + pending-po reachability)
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
    public async Task TC032_PendingPo_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending-po?pageNumber=1&pageSize=15");
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
    // SECTION 4 — UPDATE / AMENDMENT  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created local PO id (TC001 is blocked on vendor/item/budget/doc-numbering)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            vendorId = 1,
            isActive = 1,
            details = new[] { new { itemId = 1, quantity = 12m, rate = 110m } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999, isActive = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }
}
