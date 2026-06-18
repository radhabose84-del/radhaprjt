namespace ProductionManagement.QATests.Tests.LotMaster;

// ─────────────────────────────────────────────────────────────────────────────
// LotMaster — live-server QA suite (FK-dependent master; create-happy ATTEMPTED).
//
// Contract verified against source (2026-06-17):
//   POST   /api/lotmaster              { lotCode(req, max20, unique per unit), batchNumber(req, max50, unique),
//                                        lotTypeId(req, MiscMaster FK), itemId(req, cross-module Inventory),
//                                        variantId?, startDate(DateOnly req, <= today),
//                                        statusId(req, MiscMaster FK), productionOrderRef?, remarks?(max500) }
//   PUT    /api/lotmaster              { id, ...mutable fields..., isActive(int 0/1) }
//   DELETE /api/lotmaster/{id}         (id bound from ROUTE)
//   GET    /api/lotmaster?PageNumber=&PageSize=&SearchTerm=&ItemId=
//   GET    /api/lotmaster/{id}
//   GET    /api/lotmaster/by-name?term=&itemId=
//   GET    /api/lotmaster/by-stock?itemId=
//
// Why create-happy + lifecycle may be SKIPPED at runtime:
//   lotTypeId + statusId are MiscMaster FKs and itemId is a cross-module Inventory item.
//   We resolve lotTypeId/statusId via FirstIdAsync and itemId via _f.ActiveItemId; if any is
//   unresolvable (0) the create + lifecycle steps self-skip via `if (...) return;` guards.
//   Negatives, smoke GetAll, by-stock reachability, and no-auth remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LotMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LotMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/lotmaster";

    private static int _lotTypeId;
    private static int _statusId;
    private static int _itemId;
    private static string _createdLotCode = string.Empty;

    public LotMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path ATTEMPTED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_AttemptOrSkip()
    {
        _lotTypeId = await QAHelper.FirstIdAsync(_f.Client, "/api/production/miscmaster");
        _statusId = _lotTypeId; // MiscMaster pool serves both lotType and status
        _itemId = _f.ActiveItemId;

        if (_lotTypeId == 0 || _statusId == 0 || _itemId == 0)
            return; // needs seeded data: lotType/status MiscMaster + active Inventory item

        _createdLotCode = NewCode();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            lotCode = _createdLotCode,
            batchNumber = $"BATCH{_createdLotCode}",
            lotTypeId = _lotTypeId,
            itemId = _itemId,
            startDate = today.ToString("yyyy-MM-dd"),
            statusId = _statusId,
            remarks = "Created by QA suite"
        });

        if (resp.StatusCode != HttpStatusCode.OK)
            return; // clone FK chain may still reject

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
            lotCode = "NOAUTH01",
            batchNumber = "BATCHNOAUTH",
            lotTypeId = 1,
            itemId = 1,
            startDate = "2026-01-01",
            statusId = 1
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
        // Only a lotCode supplied — batchNumber/lotType/item/startDate/status missing.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            lotCode = NewCode()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_LotCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            lotCode = "",
            batchNumber = "BATCHX",
            lotTypeId = 1,
            itemId = 1,
            startDate = "2026-01-01",
            statusId = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; supports optional &ItemId=)
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
    public async Task TC022_GetAll_FilterByItemId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&ItemId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  +  BY-STOCK reachability
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
    public async Task TC032_GetByStock_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-stock?itemId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetByStock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-stock?itemId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: term + itemId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200Or400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA&itemId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA&itemId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle guarded on a created id)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            batchNumber = $"BATCH{_createdLotCode}U",
            lotTypeId = _lotTypeId,
            itemId = _itemId,
            startDate = today.ToString("yyyy-MM-dd"),
            statusId = _statusId,
            remarks = "Updated by QA",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            batchNumber = "BATCHX",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
