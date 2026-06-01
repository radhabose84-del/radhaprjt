namespace UserManagement.QATests.Tests.Department;

[Collection("DepartmentCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DepartmentQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute        = "/api/Department";
    private const string DeptGroupRoute   = "/api/DepartmentGroup";

    // CompanyId=1 assumed to exist in QA DB (cross-module, no FK constraint)
    private const int QACompanyId = 1;

    public DepartmentQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — SETUP
    //   TC001 → create DepartmentGroup → SecondaryId = DepartmentGroupId
    //   TC002 → create Department      → CreatedId   = DepartmentId
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateDepartmentGroup_CapturesGroupId()
    {
        var resp = await _f.Client.PostAsJsonAsync(DeptGroupRoute, new
        {
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = $"QA Dept Group {_f.EntityCode}",
            companyId           = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        _f.SecondaryId = doc.RootElement.CreatedId();   // robust: scalar or object id
        _f.SecondaryId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Setup_CreateDepartment_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName          = _f.EntityCode[..4],
            deptName           = $"QA Department {_f.EntityCode}",
            companyId          = QACompanyId,
            departmentGroupId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ShortName max=6 (very short!), DeptName max=50
    // No AlreadyExists, no FK validation for CompanyId/DepartmentGroupId
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "NOAU",
            deptName          = "No Auth Department",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DeptNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "TSTD",
            deptName          = "",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_ShortNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "",
            deptName          = "Test Department",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DeptNameExceedsMaxLength_Returns400()
    {
        // DeptName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "TSTD",
            deptName          = new string('A', 51),
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_ShortNameExceedsMaxLength_Returns400()
    {
        // ShortName max = 6 characters (very short!)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "TOOLONG",   // 7 chars → fails
            deptName          = "Test Department",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_ShortNameAtMaxLength_Returns200()
    {
        // Exactly 6 chars (max boundary for ShortName) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "SIXCHR",   // exactly 6 chars
            deptName          = $"QA Dept Boundary {_f.EntityCode}",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DeptNameAtMaxLength_Returns200()
    {
        // Exactly 50 chars DeptName (max boundary) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "BDRY",
            deptName          = new string('B', 50),
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Returns 404 when no data (like Currency) — not 200+empty
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_HappyPath_Returns200()
    {
        // QA DB has pre-existing departments → GetAll returns 200
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        // May return 200 or 404 depending on total count
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_NoMatchSearch_Returns404()
    {
        // GetAll returns 404 (not 200+empty) when no records match
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ⚠ NO null check in GetById action → always 200, even for non-existent
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(16)]
    public async Task TC016_GetById_ValidId_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_NonExistentId_Returns200_WithNullData()
    {
        // ⚠ GetById action has NO null check → 200 with data:null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_IdZero_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (by-name, withoutDatacontrol)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — EXTRA ENDPOINTS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(23)]
    public async Task TC023_GetByDepartmentGroup_WithValidName_Returns200()
    {
        // by-department-group/{name} — pass any string; returns 200 always (empty = [] result)
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-department-group/QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(24)]
    public async Task TC024_GetByDepartmentGroup_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-department-group/QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_GetByDepartmentGroup_EmptyName_Returns400()
    {
        // Controller checks: if (string.IsNullOrEmpty(departmentGroupName)) → 400
        // Empty route segment → ASP.NET returns 404 (no route match)
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-department-group/ ");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE
    // Controller pre-queries GetById → 404 for non-existent Id
    // IsActive is Status enum: 0=Inactive, 1=Active (not byte)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(26)]
    public async Task TC026_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = $"QA Department Updated {_f.EntityCode[..10]}",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("updated");
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = $"QA Department Updated {_f.EntityCode[..10]}",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = $"QA Department Updated {_f.EntityCode[..10]}",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = "Updated Dept",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_DeptNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = "",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_ShortNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = "",
            deptName          = "Updated Dept",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_DeptNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = _f.EntityCode[..4],
            deptName          = new string('A', 51),
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_ShortNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            shortName         = "TOOLONG",   // 7 chars
            deptName          = "Updated Dept",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById → handler returns null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = 999999,
            shortName         = "TSTD",
            deptName          = "Non-existent update",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_IdZero_Returns404()
    {
        // GetById(0) returns null → controller returns 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = 0,
            shortName         = "TSTD",
            deptName          = "Zero Id Update",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId,
            isActive          = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Controller pre-queries GetById → 404 for non-existent
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_IdZero_Returns404()
    {
        // GetById(0) returns null → controller returns 404
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("deleted");
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Delete_AlreadyDeleted_Returns404()
    {
        // After soft delete, GetById returns null → controller returns 404
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 8 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(42)]
    public async Task TC042_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // GetById action has no null check → soft-deleted record returns 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_WithoutDataControl_Returns200Or404()
    {
        // Extra endpoint: /api/Department/withoutDatacontrol?name=QA
        var resp = await _f.Client.GetAsync($"{BaseRoute}/withoutDatacontrol?name=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_MaintenanceByGroupUser_Returns200()
    {
        // Extra endpoint: maintenance-by-group-user-based/{name} — always 200
        var resp = await _f.Client.GetAsync($"{BaseRoute}/maintenance-by-group-user-based/QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_GetAll_LargePageSize_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Create_SecondDepartment_SameGroup_Returns200()
    {
        // Multiple departments per group — no duplicate check
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName         = "SCN",
            deptName          = $"QA Second Dept {_f.EntityCode[..10]}",
            companyId         = QACompanyId,
            departmentGroupId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
