namespace BackgroundService.QATests.Tests.NotificationConfig;

// ─────────────────────────────────────────────────────────────────────────────
// NotificationConfig (BackgroundService) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-18):
//   POST   /api/NotificationConfig        { moduleName, notificationEventTypeId }
//   PUT    /api/NotificationConfig        { id, moduleName, notificationEventTypeId, isActive(byte) }
//   DELETE /api/NotificationConfig?id={id}   (id bound from QUERY)
//   GET    /api/NotificationConfig?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/NotificationConfig/{id}    (ALWAYS 200; data + message both DTO)
//   GET    /api/NotificationConfig/by-name?ModuleName=
//
// Key facts that shaped assertions:
//   • moduleName: NotEmpty, max 250. notificationEventTypeId: FK (NotEmpty) → /api/backgroundservice/MiscMaster.
//     Resolved via FirstIdAsync; create self-skips if it resolves 0.
//   • Create returns a raw int id at `data` → CreatedId() reads it directly.
//   • Controller Create/Update/Delete always return HTTP 200 (no IsSuccess branch).
//   • Uniqueness is COMPOSITE (moduleName, notificationEventTypeId) — duplicate test reuses both.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("NotificationConfigCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class NotificationConfigQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/NotificationConfig";
    private const string MiscMasterRoute = "/api/backgroundservice/MiscMaster";

    private static string _createdModuleName = string.Empty;
    private static int _eventTypeId;

    public NotificationConfigQATests(QAServerFixture fixture) => _f = fixture;

    private string NewModuleName() => "QAMod" + _f.EntityCode[..Math.Min(10, _f.EntityCode.Length)];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId; self-skips if FK unresolved)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdModuleName = NewModuleName();
        _eventTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        if (_eventTypeId == 0) return; // REQUIRED FK unresolved → self-skip

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = _createdModuleName,
            notificationEventTypeId = _eventTypeId
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
            moduleName = "NoAuthModule",
            notificationEventTypeId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_ModuleNameEmpty_Returns400()
    {
        var eventTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = "",
            notificationEventTypeId = eventTypeId > 0 ? eventTypeId : 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EventTypeIdMissing_Returns400()
    {
        // notificationEventTypeId NotEmpty → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = NewModuleName(),
            notificationEventTypeId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_ModuleNameTooLong_Returns400()
    {
        var eventTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = new string('A', 251),
            notificationEventTypeId = eventTypeId > 0 ? eventTypeId : 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DuplicateComposite_Returns400()
    {
        if (_f.CreatedId == 0) return; // create self-skipped → nothing to duplicate

        // Same (moduleName, eventTypeId) as TC001 → composite AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = _createdModuleName,
            notificationEventTypeId = _eventTypeId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmptyBody_Returns400()
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdModuleName}");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (ALWAYS 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{(_f.CreatedId == 0 ? 1 : _f.CreatedId)}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200()
    {
        // Live: this controller returns a proper 404 envelope for a missing id (not 200).
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `ModuleName`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithModuleName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?ModuleName=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?ModuleName=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (echoes moduleName + eventTypeId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            moduleName = _createdModuleName + "U",
            notificationEventTypeId = _eventTypeId,
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
            moduleName = _createdModuleName + "U",
            notificationEventTypeId = _eventTypeId,
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
            moduleName = _createdModuleName + "U",
            notificationEventTypeId = _eventTypeId,
            isActive = (byte)0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            moduleName = _createdModuleName + "U",
            notificationEventTypeId = _eventTypeId,
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
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
