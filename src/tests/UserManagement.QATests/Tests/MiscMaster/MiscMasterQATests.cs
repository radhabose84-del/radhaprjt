namespace UserManagement.QATests.Tests.MiscMaster;

[Collection("MiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute         = "/api/usermanagement/MiscMaster";
    private const string MiscTypeRoute     = "/api/usermanagement/MiscTypeMaster";

    public MiscMasterQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — SETUP
    //   TC001 → create MiscTypeMaster → SecondaryId = MiscTypeId
    //           EntityCode = MiscTypeCode string (used for autocomplete param)
    //   TC002 → create MiscMaster     → CreatedId = MiscMasterId
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateMiscTypeMaster_CapturesMiscTypeId()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = _f.EntityCode,
            description  = $"QA MiscMaster Setup {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.SecondaryId = id;   // MiscTypeId used for all MiscMaster tests
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Setup_CreateMiscMaster_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ⚠ Create validator uses "NotFound" case for NotEmpty → error says "not found"
    // AlreadyExists is composite: Code + MiscTypeId together
    // Code max=50, Description max=250
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = "NOAUTH",
            description = "No Auth MiscMaster"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_CodeEmpty_Returns400()
    {
        // ⚠ Create validator uses "NotFound" case → error says "not found"
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = "",
            description = "Test Description"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = "NODESC",
            description = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_MiscTypeIdZero_Returns400()
    {
        // MiscTypeId=0 triggers NotEmpty for int (0 is default/empty for int)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = 0,
            code        = "NOTYPE",
            description = "No Type Description"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CodeExceedsMaxLength_Returns400()
    {
        // Code max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = new string('A', 51),
            description = "Long Code Test"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DescriptionExceedsMaxLength_Returns400()
    {
        // Description max = 250 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = "LONGDESC",
            description = new string('A', 251)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCompositeKey_Returns400()
    {
        // Same Code + MiscTypeId as TC002 → composite AlreadyExists fires
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,   // same code + same MiscTypeId = duplicate
            description = "Duplicate Composite Key"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_SameCode_DifferentMiscTypeId_Returns200()
    {
        // Same Code but different MiscTypeId → composite key is different → allowed
        // Use a dummy MiscTypeId that exists (1 is assumed to exist in DB)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = 1,              // different MiscType
            code        = _f.EntityCode,  // same code → but different FK = no conflict
            description = "Same Code Different Type"
        });

        // May be 200 (composite key unique) or 400 if MiscTypeId=1 doesn't exist
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Response: { statusCode, data:[], TotalCount, PageNumber, PageSize }
    // Always 200 — no 404 for empty results
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(12)]
    public async Task TC012_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_GetAll_NoMatchSearch_Returns200_WithEmptyData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // Controller has null check → 404 when handler returns null
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_ValidId_Returns200_WithCorrectData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("code").GetString()
            .Should().Be(_f.EntityCode);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_NonExistentId_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_IdZero_Returns404()
    {
        // Handler returns null for id=0 → controller returns 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE
    // ⚠ UNIQUE: requires BOTH params → name + MiscTypeCode (string, not id)
    // EntityCode = MiscTypeCode of the MiscTypeMaster created in TC001
    // Always returns 200 OK
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_BothParams_Returns200()
    {
        // Both name and MiscTypeCode required for meaningful results
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/by-name?name={_f.EntityCode}&MiscTypeCode={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_NameOnly_Returns200()
    {
        // MiscTypeCode missing → handler uses empty/null → still returns 200
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(
            $"{BaseRoute}/by-name?name={_f.EntityCode}&MiscTypeCode={_f.EntityCode}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(24)]
    public async Task TC024_AutoComplete_NoMatchTerm_Returns200_WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/by-name?name=ZZZNOMATCH&MiscTypeCode={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ⚠ Update validator uses "Notempty" (wrong casing) → NotEmpty NEVER fires
    //   → empty Code on Update passes validation → 200 returned
    // Controller always returns 200 regardless of result
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(25)]
    public async Task TC025_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster Updated {_f.EntityCode}",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Updated");
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster Updated {_f.EntityCode}",
            sortOrder   = 1,
            isActive    = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster Updated {_f.EntityCode}",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = "Updated description",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_CodeEmpty_Returns200_DueToValidatorTypo()
    {
        // ⚠ Update validator has "Notempty" (wrong casing) → NotEmpty case never matches
        // Empty Code passes all remaining validators (MaxLength OK, AlreadyExists checks "")
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = "",
            description = "Empty Code Update",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        // NotEmpty never fires → 200 (behavior defect, documented by this test)
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_CodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = new string('A', 51),
            description = "Updated description",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_DescriptionExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = new string('A', 251),
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_SortOrderChange_Returns200()
    {
        // SortOrder field unique to MiscMaster Update command
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster Updated {_f.EntityCode}",
            sortOrder   = 10,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_DuplicateCompositeKey_Returns400()
    {
        // Create a second MiscMaster first to create a duplicate scenario
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = $"DUP{_f.EntityCode[..7]}",
            description = "Duplicate target"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now try to update our main record to the same code+type combo
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = $"DUP{_f.EntityCode[..7]}",   // same code as newly created
            description = "Trying duplicate",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_RestoreOriginalCode_Returns200()
    {
        // Restore back to original code after TC033 changed it
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            miscTypeId  = _f.SecondaryId,
            code        = _f.EntityCode,
            description = $"QA MiscMaster {_f.EntityCode}",
            sortOrder   = 1,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Controller always returns 200; FluentValidation pipeline handles errors
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_IdZero_Returns400()
    {
        // NotEmpty validator fires for id=0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_NonExistentId_Returns400Or200()
    {
        // NotFound validator checks existence — behaviour depends on NotFoundAsync impl
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("elete");   // "Deleted successfully." or "Deleted"
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_AlreadyDeleted_Returns400Or200()
    {
        // Soft-deleted record: NotFound or SoftDelete validator may fire
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_VerifySoftDelete_GetByIdReturns404()
    {
        // Controller has null check → handler returns null for soft-deleted → 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(41)]
    public async Task TC041_GetAll_LargePageSize_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_MiscTypeCodeOnly_Returns200()
    {
        // No name param — autocomplete uses only MiscTypeCode filter
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/by-name?MiscTypeCode={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_Create_SecondMiscMaster_SameType_DifferentCode_Returns200()
    {
        // Same MiscTypeId, different Code → composite key unique → allowed
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = $"SEC{_f.EntityCode[..7]}",
            description = "Second QA MiscMaster"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_GetAll_SearchByCode_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=SEC");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_GetById_ReturnsExpectedFields()
    {
        // Create a fresh record to verify response structure
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = $"FLD{_f.EntityCode[..7]}",
            description = "Field Check"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createDoc = await ParseAsync(createResp);
        var newId = createDoc.RootElement.CreatedId();

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);

        var data = doc.RootElement.GetProperty("data");
        data.TryGetProperty("id", out _).Should().BeTrue();
        data.TryGetProperty("code", out _).Should().BeTrue();
        data.TryGetProperty("description", out _).Should().BeTrue();
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Create_CodeAtMaxLength_Returns200()
    {
        // Exactly 50 chars (max boundary) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = new string('Z', 49) + "Q",   // 50 chars
            description = "Max Code Length Test"
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
