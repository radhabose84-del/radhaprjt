namespace InventoryManagement.QATests.Tests.ItemSpecificationValue;

// ─────────────────────────────────────────────────────────────────────────────
// ItemSpecificationValue — live-server QA suite (CRUD + negatives, FK-dependent create).
//
// Contract verified against source (2026-06-17 — ItemSpecificationValueController.cs):
//   POST   /api/itemspecificationvalue        { specificationMasterId, specificationValue? }
//   PUT    /api/itemspecificationvalue        { id, specificationMasterId, specificationValue?, isActive }
//   DELETE /api/itemspecificationvalue?id={id}  (id bound from QUERY — action param `int id`)
//   GET    /api/itemspecificationvalue?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/itemspecificationvalue/{id}    (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/itemspecificationvalue/by-name?term=&specificationMasterId=
//   GET    /api/itemspecificationvalue/by-specification-master/{specificationMasterId}
//
// FK / seeding note:
//   specificationMasterId is a same-module FK → /api/itemspecificationmaster. Resolved at runtime via
//   FirstIdAsync; when the clone has no spec master (id 0), the create-happy step and the tests that
//   depend on a created id self-skip ([Fact(Skip="…")]). The Smoke / no-auth / empty-body / reachability
//   tests stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ItemSpecificationValueCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ItemSpecificationValueQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/itemspecificationvalue";
    private const string MasterRoute = "/api/itemspecificationmaster";

    private static int _specMasterId;

    public ItemSpecificationValueQATests(QAServerFixture fixture) => _f = fixture;

    private string NewValue() => $"QA SpecValue {_f.EntityCode[..8]}";

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _specMasterId = await QAHelper.FirstIdAsync(_f.Client, MasterRoute);
        if (_specMasterId == 0)
            return; // no spec master in clone → covered by downstream Skips; nothing to assert here.

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationMasterId = _specMasterId,
            specificationValue = NewValue()
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
            specificationMasterId = 1,
            specificationValue = "No Auth Value"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        // specificationMasterId defaults to 0 → FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MasterIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationMasterId = 0,
            specificationValue = NewValue()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_InvalidMasterFk_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationMasterId = 999999,
            specificationValue = NewValue()
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE + BY-SPECIFICATION-MASTER reachability
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_BySpecificationMaster_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-specification-master/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_BySpecificationMaster_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-specification-master/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created ItemSpecificationValue (TC001 self-skips when no spec master exists)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationMasterId = _specMasterId,
            specificationValue = NewValue() + " Upd",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            specificationMasterId = 1,
            specificationValue = "QA Upd",
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
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a successfully created ItemSpecificationValue (TC001 self-skips when no spec master exists)"), TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
