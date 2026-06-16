namespace SalesManagement.QATests.Tests.StoReceipt;

// ─────────────────────────────────────────────────────────────────────────────
// StoReceipt — live-server QA suite (TRANSACTIONAL; CREATE + READS ONLY — no update/delete).
//
// Contract verified against source (2026-06-15):
//   POST   /api/StoReceipt              { stoReceiptDate(DateOnly), deliveryChallanHeaderId, receivingPlantId,
//                                          receivingStorageLocationId, binId?, vehicleNumber?, remarks?,
//                                          details:[CreateStoReceiptDetailDto] }
//   GET    /api/StoReceipt?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/StoReceipt/{id}          (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/StoReceipt/by-name?term=
//   GET    /api/StoReceipt/dc-open-qty?dcDetailId={id}
//
// StoReceiptController exposes NO PUT and NO DELETE — only Create + 4 reads. This suite
// authors sections for those verbs only (per "author only sections that exist").
//
// Why create-happy is SKIPPED:
//   A valid StoReceipt requires a seeded deliveryChallanHeaderId (an inbound delivery challan with
//   open quantity) plus receiving plant/storage ids and nested receipt details mapped to DC detail
//   lines — none guaranteed in the QA clone. Attribute-level [Fact(Skip=...)] keeps it explicit.
//   Negatives (empty body / missing required / no-auth), smoke GetAll, AutoComplete, GetById and
//   dc-open-qty reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("StoReceiptCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StoReceiptQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/StoReceipt";

    public StoReceiptQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a delivery-challan header with open qty (deliveryChallanHeaderId) + receiving plant/storage ids and mapped receipt details"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            deliveryChallanHeaderId = 1,
            receivingPlantId = 1,
            receivingStorageLocationId = 1,
            vehicleNumber = "QA-1234",
            remarks = "Created by QA suite",
            details = new[]
            {
                new { itemId = 1, quantity = 10m, uomId = 1 }
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
            stoReceiptDate = "2026-01-01",
            deliveryChallanHeaderId = 1,
            receivingPlantId = 1,
            receivingStorageLocationId = 1
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
    public async Task TC004_Create_MissingRequiredFields_Returns400()
    {
        // Only a date supplied — deliveryChallanHeaderId, plant/storage and details missing.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoReceiptDate = "2026-01-01"
        });

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
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  +  DC-OPEN-QTY reachability
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_DcOpenQty_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/dc-open-qty?dcDetailId=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_DcOpenQty_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/dc-open-qty?dcDetailId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
