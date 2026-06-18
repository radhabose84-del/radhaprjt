namespace BackgroundService.QATests.Tests.NotificationWhatsAppGroup;

// ─────────────────────────────────────────────────────────────────────────────
// NotificationWhatsAppGroup (BackgroundService) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-18):
//   POST   /api/NotificationWhatsAppGroup        { departmentId, groupName, apiKey }
//   PUT    /api/NotificationWhatsAppGroup        { id, departmentId, groupName, apiKey, isActive(INT 0/1) }
//   DELETE /api/NotificationWhatsAppGroup/{id}   (id bound from ROUTE)
//   GET    /api/NotificationWhatsAppGroup?pageNumber=&pageSize=&searchTerm=&departmentId=  (camelCase)
//   GET    /api/NotificationWhatsAppGroup/{id:int}        (ALWAYS 200)
//   GET    /api/NotificationWhatsAppGroup/autocomplete?searchTerm=   (suffix is `autocomplete`, NOT by-name)
//   GET    /api/NotificationWhatsAppGroup/by-department/{departmentId:int}?searchTerm=
//
// Key facts that shaped assertions:
//   • departmentId: FK (>0) → /api/Department. Resolved via FirstIdAsync; create self-skips if 0.
//   • groupName: NotEmpty, max 250, regex ^[a-zA-Z0-9 ]+$ (alphanumeric + SPACES, NO hyphens/specials).
//     → run-unique name derived WITHOUT hyphens.
//   • apiKey: NotEmpty, max 500.
//   • isActive on Update is INT (0/1) — NOT byte (the rest of the module uses byte).
//   • Create returns a raw int id at `data` (+IsSuccess:true) → CreatedId() reads it directly.
//   • Controller always returns HTTP 200 on Create/Update/Delete. NO uniqueness/NotFound validators.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("NotificationWhatsAppGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class NotificationWhatsAppGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/NotificationWhatsAppGroup";
    private const string DepartmentRoute = "/api/Department";
    private const string TestApiKey = "QA-TEST-APIKEY-0123456789";

    private static int _departmentId;
    private static string _createdName = string.Empty;

    public NotificationWhatsAppGroupQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique name; groupName regex allows letters/digits/spaces only → no hyphens.
    private string NewName() => "QAWA " + _f.EntityCode[..Math.Min(10, _f.EntityCode.Length)];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId; self-skips if FK unresolved)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdName = NewName();
        _departmentId = await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        if (_departmentId == 0) return; // REQUIRED FK unresolved → self-skip

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentId = _departmentId,
            groupName = _createdName,
            apiKey = TestApiKey
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
            departmentId = 1,
            groupName = "NoAuth Group",
            apiKey = TestApiKey
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_GroupNameEmpty_Returns400()
    {
        var deptId = await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentId = deptId > 0 ? deptId : 1,
            groupName = "",
            apiKey = TestApiKey
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_ApiKeyEmpty_Returns400()
    {
        var deptId = await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentId = deptId > 0 ? deptId : 1,
            groupName = NewName(),
            apiKey = ""
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DepartmentIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentId = 0,
            groupName = NewName(),
            apiKey = TestApiKey
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_GroupNameInvalidChars_Returns400()
    {
        // Regex ^[a-zA-Z0-9 ]+$ rejects special chars (@, hyphen, etc.).
        var deptId = await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentId = deptId > 0 ? deptId : 1,
            groupName = "QA@Invalid#Name",
            apiKey = TestApiKey
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
    // SECTION 2 — GET ALL  (camelCase params)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_FilterByDepartmentId_Returns200()
    {
        var deptId = _departmentId > 0 ? _departmentId : await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15&departmentId={deptId}");
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (suffix is `autocomplete`) + BY-DEPARTMENT
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?searchTerm=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?searchTerm=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_ByDepartment_Returns200()
    {
        var deptId = _departmentId > 0 ? _departmentId : await QAHelper.FirstIdAsync(_f.Client, DepartmentRoute);
        if (deptId == 0) return;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-department/{deptId}?searchTerm=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (isActive is INT here, not byte)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            departmentId = _departmentId,
            groupName = _createdName + " U",
            apiKey = TestApiKey,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId == 0 ? 1 : _f.CreatedId,
            departmentId = _departmentId,
            groupName = _createdName + " U",
            apiKey = TestApiKey,
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

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            departmentId = _departmentId,
            groupName = _createdName + " U",
            apiKey = TestApiKey,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            departmentId = _departmentId,
            groupName = _createdName + " U",
            apiKey = TestApiKey,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE: /{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{(_f.CreatedId == 0 ? 1 : _f.CreatedId)}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
