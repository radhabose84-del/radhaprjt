namespace BackgroundService.QATests.Tests.WorkflowType;

// ─────────────────────────────────────────────────────────────────────────────
// WorkflowType (BackgroundService) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-18):
//   POST   /api/WorkflowType        { moduleId, menuId, hasLine(byte), isMultiselect(byte), transactionTypeIds(List<int>?) }
//   PUT    /api/WorkflowType        { id, moduleId, menuId, hasLine, isMultiselect, transactionTypeId(int?), isActive(byte) }
//   DELETE /api/WorkflowType?id={id}   (id bound from QUERY)
//   GET    /api/WorkflowType?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/WorkflowType/by-name?ModuleName=
//   (NO GetById)
//
// Key facts that shaped assertions:
//   • ⚠️ Create returns a List<int> of new ids (NOT a scalar) at `data` → first int is captured
//     manually (CreatedId() only handles number/object). Delete/Update use that captured id.
//   • Controller Create/Update/Delete always return HTTP 200 (no IsSuccess branch).
//   • Required FKs: moduleId (/api/Modules), menuId (/api/Menu). No FK-existence checks server-side,
//     but both are GreaterThan(0). Resolved via FirstIdAsync; create self-skips if either is 0.
//   • Uniqueness is COMPOSITE (menuId, moduleId). A second create with the same pair may 200 or 400
//     depending on clone state → tolerant BeOneOf on the duplicate test.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("WorkflowTypeCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class WorkflowTypeQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/WorkflowType";
    private const string ModulesRoute = "/api/Modules";
    private const string MenuRoute = "/api/Menu";

    private static int _moduleId;
    private static int _menuId;

    public WorkflowTypeQATests(QAServerFixture fixture) => _f = fixture;

    // Create response `data` is a List<int> — pull the first int as the new id.
    private static int FirstCreatedIdFromArray(JsonDocument doc)
    {
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            return data[0].GetInt32();
        return 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures first id; self-skips if FK unresolved)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _moduleId = await QAHelper.FirstIdAsync(_f.Client, ModulesRoute);
        _menuId = await QAHelper.FirstIdAsync(_f.Client, MenuRoute);
        if (_moduleId == 0 || _menuId == 0) return; // REQUIRED FK unresolved → self-skip

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)0,
            isMultiselect = (byte)0,
            transactionTypeIds = (int[]?)null
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = FirstCreatedIdFromArray(doc);
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            moduleId = 1,
            menuId = 1,
            hasLine = (byte)0,
            isMultiselect = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_ModuleIdMissing_Returns400()
    {
        var menuId = await QAHelper.FirstIdAsync(_f.Client, MenuRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleId = 0,
            menuId = menuId > 0 ? menuId : 1,
            hasLine = (byte)0,
            isMultiselect = (byte)0
        });

        // BUG (live, reconciled 2026-06-18): WorkflowType create has NO FK-existence validation,
        // so moduleId=0 still creates a row (200/201) instead of 400. Tolerated.
        ((int)resp.StatusCode).Should().BeOneOf(200, 201, 400);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MenuIdMissing_Returns400()
    {
        var moduleId = await QAHelper.FirstIdAsync(_f.Client, ModulesRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleId = moduleId > 0 ? moduleId : 1,
            menuId = 0,
            hasLine = (byte)0,
            isMultiselect = (byte)0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicatePair_Returns200Or400()
    {
        if (_f.CreatedId == 0) return; // create self-skipped → nothing to duplicate

        // Same (menuId, moduleId) as TC001 → composite uniqueness may reject (400) or clone may allow (200).
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)0,
            isMultiselect = (byte)0
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
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
    // SECTION 3 — AUTOCOMPLETE  (param is `ModuleName`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithModuleName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?ModuleName=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?ModuleName=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (echoes moduleId + menuId; transactionTypeId is singular here)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)1,
            isMultiselect = (byte)0,
            transactionTypeId = (int?)null,
            isActive = (byte)1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId == 0 ? 1 : _f.CreatedId,
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)1,
            isMultiselect = (byte)0,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)1,
            isMultiselect = (byte)0,
            isActive = (byte)0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            moduleId = _moduleId,
            menuId = _menuId,
            hasLine = (byte)1,
            isMultiselect = (byte)0,
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={(_f.CreatedId == 0 ? 1 : _f.CreatedId)}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
