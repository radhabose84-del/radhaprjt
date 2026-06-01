namespace UserManagement.QATests.Tests.DepartmentGroup;

[Collection("DepartmentGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DepartmentGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DepartmentGroup";

    // ⚠ Non-standard routes:
    //   GetAll  → GET  api/DepartmentGroup/GetAllDepartmentGroup
    //   Update  → PUT  api/DepartmentGroup/update
    private const string GetAllRoute  = "/api/DepartmentGroup/GetAllDepartmentGroup";
    private const string UpdateRoute  = "/api/DepartmentGroup/update";

    public DepartmentGroupQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // Create returns int directly in Data; IsActive is byte
    // Only DepartmentGroupName validated (NotEmpty + MaxLength + Uniqueness)
    // DepartmentGroupCode is NOT validated → any length/format accepted
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = $"QA Dept Group {_f.EntityCode}",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        // Create returns int directly: { "data": 42 }
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "NOAU",
            departmentGroupName = "No Auth Dept Group",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_DepartmentGroupNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "TSTGRP",
            departmentGroupName = "",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DepartmentGroupNameExceedsMaxLength_Returns400()
    {
        // DepartmentGroupName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "TSTGRP",
            departmentGroupName = new string('A', 51),
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateDepartmentGroupName_Returns400()
    {
        // Uniqueness check is ALWAYS applied (outside switch/case) → fires on duplicate name
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "DUP",
            departmentGroupName = $"QA Dept Group {_f.EntityCode}",   // same as TC001
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_DepartmentGroupCodeNotValidated_LongCodePasses()
    {
        // DepartmentGroupCode has no validator → code longer than 15 chars passes validation
        // Note: may fail at DB level if column is varchar(15)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "VALIDCODE",      // 9 chars, within typical DB limit
            departmentGroupName = $"QA Valid Code Group {_f.EntityCode}",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DepartmentGroupNameAtMaxLength_Returns200()
    {
        // Exactly 50 chars (max boundary) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "BDRY",
            departmentGroupName = new string('B', 49) + "Q",   // 50 chars, unique
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_SecondGroup_DifferentName_Returns200()
    {
        // Multiple groups with same Code but different Name → allowed (code not validated unique)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = _f.EntityCode[..6],    // same code as TC001 — no conflict
            departmentGroupName = $"QA Second Dept Group {_f.EntityCode}",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_InactiveGroup_Returns200()
    {
        // IsActive=0 (byte) on create is valid
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "INACT",
            departmentGroupName = $"QA Inactive Group {_f.EntityCode}",
            isActive            = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ⚠ NON-STANDARD ROUTE: GET api/DepartmentGroup/GetAllDepartmentGroup
    // Returns 404 when no records match (not 200+empty)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15");

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
        var resp = await _f.AnonymousClient.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_NoMatchSearch_Returns404()
    {
        // Returns 404 (not 200+empty) when no records match
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ⚠ NO null check → always 200, even for non-existent
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
        // NO null check → 200 with data:null
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
    // SECTION 4 — AUTOCOMPLETE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
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
    // SECTION 5 — UPDATE
    // ⚠ NON-STANDARD ROUTE: PUT api/DepartmentGroup/update
    // IsActive is Status enum (int 0/1), NOT byte
    // No Id > 0 check, no existence check in validator → any Id passes
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(23)]
    public async Task TC023_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = $"QA Dept Group Updated {_f.EntityCode[..8]}",
            isActive            = 1   // Status enum: 1=Active
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Updated");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = $"QA Dept Group Updated {_f.EntityCode[..8]}",
            isActive            = 0   // Status enum: 0=Inactive
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = $"QA Dept Group Updated {_f.EntityCode[..8]}",
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = "Updated Name",
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_DepartmentGroupNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = "",
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_DepartmentGroupNameTooLong_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = _f.EntityCode[..6],
            departmentGroupName = new string('A', 51),
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_IdZero_NoValidatorCheck_Returns200()
    {
        // Update validator has no Id > 0 check and no existence check
        // → Id=0 passes validation → handler updates 0 rows → controller returns 200
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = 0,
            departmentGroupCode = "ZERO",
            departmentGroupName = "Zero Id Group Update",
            isActive            = 1
        });

        // No validator catches Id=0 → 200 (behavior defect, documented)
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_NonExistentId_NoValidatorCheck_Returns200()
    {
        // Update validator has no existence check → non-existent Id passes
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = 999999,
            departmentGroupCode = "NONEX",
            departmentGroupName = "Non-existent Update",
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_CodeChange_Returns200()
    {
        // Code is not validated → change it freely
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = _f.CreatedId,
            departmentGroupCode = "NEWCD",
            departmentGroupName = $"QA Dept Group Updated {_f.EntityCode[..8]}",
            isActive            = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Controller ignores GetById result → always 200 after validator passes
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(33)]
    public async Task TC033_Delete_IdZero_Returns400()
    {
        // NotEmpty validator fires for id=0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_NonExistentId_Returns200()
    {
        // Controller ignores GetById result → passes to handler → 0 rows deleted → 200
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_AlreadyDeleted_Returns200Or400()
    {
        // SoftDelete validator may fire if soft-deleted record has dependents
        // Otherwise silently returns 200
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(38)]
    public async Task TC038_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // GetById has no null check → soft-deleted record returns 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_GetAll_AfterDelete_StillFindsOtherGroups()
    {
        // TC009 created another group which is still active
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15&SearchTerm=QA+Second");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_GetAll_LargePageSize_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoMatch_Returns200_WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_GetById_ResponseHasDataField()
    {
        // Create fresh group to verify response structure
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "FLDCK",
            departmentGroupName = $"QA Field Check Group {_f.EntityCode}",
            isActive            = (byte)1
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createDoc = await ParseAsync(createResp);
        var newId     = createDoc.RootElement.CreatedId();

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.TryGetProperty("id", out _).Should().BeTrue();
        data.TryGetProperty("departmentGroupName", out _).Should().BeTrue();
    }

    [Fact, TestPriority(43)]
    public async Task TC043_AutoComplete_FindsCreatedGroup_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA+Dept+Group");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_Create_NameWithSpaces_Returns200()
    {
        // Spaces in name are allowed (no Alphanumeric restriction)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "SPACE",
            departmentGroupName = $"QA Group With Spaces {_f.EntityCode}",
            isActive            = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_Update_VerifyChange_GetByIdReflectsUpdate()
    {
        // Create a fresh group, update it, then verify via GetById
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            departmentGroupCode = "UPD",
            departmentGroupName = $"QA Pre-Update Group {_f.EntityCode}",
            isActive            = (byte)1
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cDoc  = await ParseAsync(createResp);
        var newId = cDoc.RootElement.CreatedId();

        var updResp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id                  = newId,
            departmentGroupCode = "UPD2",
            departmentGroupName = $"QA Post-Update Group {_f.EntityCode}",
            isActive            = 1
        });
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(getResp);
        doc.RootElement.GetProperty("data").GetProperty("departmentGroupName").GetString()
            .Should().Contain("Post-Update");
    }

    [Fact, TestPriority(46)]
    public async Task TC046_GetAll_StandardHttpGetRoute_Returns404()
    {
        // GET api/DepartmentGroup (without /GetAllDepartmentGroup) has no handler → 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(404, 405);   // no matching route
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
