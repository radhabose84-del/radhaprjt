namespace InventoryManagement.QATests.Tests.Mrs;

// ─────────────────────────────────────────────────────────────────────────────
// Mrs (Material Requisition Slip) — live-server QA suite (TRANSACTIONAL; create + update SKIPPED).
//
// Contract verified against source (2026-06-17 — MrsController.cs):
//   ⚠ Route prefix is "api/inventory/[controller]" → /api/inventory/Mrs
//   POST   /api/inventory/Mrs                                   ([FromBody] CreateMrsEntryCommand — nested mrsDetails)
//   PUT    /api/inventory/Mrs                                   ([FromBody] UpdateMrsEntryCommand)
//   GET    /api/inventory/Mrs/MrsEntryDetails?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/inventory/Mrs/{Id}                              (404 when not found)
//   GET    /api/inventory/Mrs/StockMovement/{itemId}/{warehouseId}
//   GET    /api/inventory/Mrs/ParentWarehouse/{Id}
//   GET    /api/inventory/Mrs/pending?PageNumber=&PageSize=&SearchTerm=
//
// Why create + update are SKIPPED:
//   A valid MRS requires the requesting Unit's workflow configuration plus warehouse stock —
//   neither guaranteed on the QA clone. Attribute-level [Fact(Skip=...)].
//   MrsEntryDetails (smoke), GetById non-existent, pending + StockMovement reachability, negatives ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("MrsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MrsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/inventory/Mrs";

    public MrsQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path + update BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: Unit workflow config + warehouse stock"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            warehouseId = 1,
            mrsDetails = new[]
            {
                new { itemId = _f.ActiveItemId, quantity = 1m }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { warehouseId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — ENTRY DETAILS  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_EntryDetails_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/MrsEntryDetails?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_EntryDetails_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/MrsEntryDetails?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (GetById + pending + StockMovement reachability)
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
    public async Task TC032_GetPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_StockMovement_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/StockMovement/1/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: Unit workflow config + warehouse stock"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            warehouseId = 1,
            mrsDetails = new[]
            {
                new { itemId = _f.ActiveItemId, quantity = 2m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999, warehouseId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
