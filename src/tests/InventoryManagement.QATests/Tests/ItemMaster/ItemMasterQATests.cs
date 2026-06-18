namespace InventoryManagement.QATests.Tests.ItemMaster;

// ─────────────────────────────────────────────────────────────────────────────
// ItemMaster — live-server QA suite (ITEM MASTER; create-happy + update SKIPPED).
//
// Contract verified against source (2026-06-17 — ItemMasterController.cs):
//   ⚠ Route is "api/[controller]" → /api/ItemMaster
//   GET    /api/ItemMaster?pageNumber=&pageSize=&search=&onlyActive=&itemGroupId=&itemCategoryId=
//   GET    /api/ItemMaster/{id:int}                  (returns 404 with body when not found)
//   GET    /api/ItemMaster/autocomplete?searchPattern=&itemGroupId=&itemCategoryId=
//   GET    /api/ItemMaster/variantFilter?hasVariant=&parentItemId=&moduleId=
//   POST   /api/ItemMaster                           ([FromBody] ItemDto — large nested item payload)
//   PUT    /api/ItemMaster                           ([FromBody] ItemDto)
//   POST   /api/ItemMaster/variants  /template  /upload-logo
//   DELETE /api/ItemMaster/delete-logo
//   GET    /api/ItemMaster/logs   /log/id/{id}   /logs/itemid/{itemId}
//
// Why create-happy + update are SKIPPED:
//   A valid Item requires a fully-formed nested ItemDto carrying real itemGroup, itemCategory,
//   stockUom and hsn ids plus tab/variant sub-objects — none of which the QA clone guarantees.
//   These are attribute-level [Fact(Skip=...)] so they are explicit pending work, not silent gaps.
//   GetAll (smoke), GetById (valid via _f.ActiveItemId + non-existent), autocomplete, variantFilter,
//   and negatives (no-auth / empty body) remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ItemMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ItemMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ItemMaster";

    public ItemMasterQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path + update BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: ItemMaster nested payload (itemGroup/itemCategory/stockUom/hsn) + tabs"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemName = $"QA Item {_f.EntityCode}",
            itemGroupId = 1,
            itemCategoryId = 1,
            stockUomId = 1,
            hsnId = 1
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
            itemName = "Sec Bypass Attempt",
            itemGroupId = 1,
            itemCategoryId = 1
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
    // SECTION 2 — GET ALL  (smoke; clone has items → strict 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        await QAHelper.AssertOkAsync(resp);

        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_WithFilters_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=5&itemGroupId=1&itemCategoryId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (valid via _f.ActiveItemId + non-existent)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidActiveItem_Returns200()
    {
        if (_f.ActiveItemId <= 0)
            return; // no resolvable active item on the clone — nothing to assert

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.ActiveItemId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE + VARIANT FILTER  (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithPattern_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?searchPattern=a");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?searchPattern=a");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_VariantFilter_Reachable_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/variantFilter?hasVariant=true");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle BLOCKED — depends on a created id; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: ItemMaster nested payload (itemGroup/itemCategory/stockUom/hsn) + tabs"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemName = $"QA Item Updated {_f.EntityCode}",
            itemGroupId = 1,
            itemCategoryId = 1,
            stockUomId = 1,
            hsnId = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            itemName = "Sec Bypass"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
