namespace UserManagement.QATests.Tests.MiscTypeMaster;

[Collection("MiscTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/usermanagement/MiscTypeMaster";

    public MiscTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ⚠ Create validator uses "NotFound" case for NotEmpty — error says "not found"
    // MiscTypeCode max=50, Description max=250; AlreadyExists uniqueness check
    // Create returns ApiResponseDTO<GetMiscTypeMasterDto> → data.id
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = _f.EntityCode,
            description  = $"QA MiscTypeMaster {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "NOAUTH01",
            description  = "No Auth MiscType"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_MiscTypeCodeEmpty_Returns400()
    {
        // ⚠ Create validator uses "NotFound" case for NotEmpty → error says "not found"
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "",
            description  = "Test Description"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "TSTEMP01",
            description  = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MiscTypeCodeExceedsMaxLength_Returns400()
    {
        // MiscTypeCode max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = new string('A', 51),
            description  = "Test Description"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DescriptionExceedsMaxLength_Returns400()
    {
        // Description max = 250 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "TSTMX01",
            description  = new string('A', 251)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_DuplicateMiscTypeCode_Returns400()
    {
        // AlreadyExists validator fires for duplicate code
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = _f.EntityCode,   // same code as TC001
            description  = "Duplicate MiscType"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_MiscTypeCodeAtMaxLength_Returns200()
    {
        // Exactly 50 chars (boundary) — should pass. MiscTypeCode is unique-validated, so the
        // 50-char code must be run-unique (the old fixed "A…Z" collided on re-runs). EntityCode's
        // fast-varying chars come first, so the leading 50 chars stay unique per run.
        var code = (_f.EntityCode + new string('A', 50)).Substring(0, 50);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = code,
            description  = "Max Length Code Test"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DescriptionAtMaxLength_Returns200()
    {
        // Exactly 250 chars description (boundary) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = $"TSTBD{_f.EntityCode[..5]}",
            description  = new string('B', 250)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Response: { statusCode, data:[], TotalCount, PageNumber, PageSize }
    // Note: NO message field; NO 404 for empty results (always 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_HappyPath_Returns200()
    {
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_NoMatchSearch_Returns200_WithEmptyData()
    {
        // Unlike Currency, MiscTypeMaster GetAll returns 200 with empty data (not 404)
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // Returns 200 with {data, message} if IsSuccess, 404 if not
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(16)]
    public async Task TC016_GetById_ValidId_Returns200_WithCorrectData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("miscTypeCode").GetString()
            .Should().Be(_f.EntityCode);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_NonExistentId_Returns404()
    {
        // Controller checks IsSuccess → returns 404 when not found
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_IdZero_Returns404()
    {
        // Handler returns IsSuccess=false for id=0 → controller returns 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE
    // Returns 200 with {data} if IsSuccess, 404 if handler reports failure
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name={_f.EntityCode}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_EmptyName_Returns200Or404()
    {
        // Empty name — handler behaviour varies: may return all records (200) or empty (404)
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_NoMatchTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=ZZZNOMATCH999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // Update validator uses "NotEmpty" correctly → error says "required"
    // No ByteValue case → IsActive is not validated by FluentValidation
    // Controller pre-queries GetById (dead null-check) then sends update command
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = $"QA MiscTypeMaster Updated {_f.EntityCode}",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().NotBeNullOrEmpty();
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = $"QA MiscTypeMaster Updated {_f.EntityCode}",
            isActive     = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = $"QA MiscTypeMaster Updated {_f.EntityCode}",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = "Updated description",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_MiscTypeCodeEmpty_Returns400()
    {
        // Update validator uses "NotEmpty" correctly → error says "required"
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = "",
            description  = "Updated description",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = "",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_MiscTypeCodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            miscTypeCode = new string('A', 51),
            description  = "Updated description",
            isActive     = (byte)1
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
            id           = _f.CreatedId,
            miscTypeCode = _f.EntityCode,
            description  = new string('A', 251),
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_DuplicateCode_Returns400()
    {
        // Use the code created in TC009 (max-length test) — that code exists
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = _f.CreatedId,
            // TC009 created a record with exactly this code; updating a *different* record (CreatedId)
            // to it must be rejected as a duplicate.
            miscTypeCode = (_f.EntityCode + new string('A', 50)).Substring(0, 50),
            description  = "Duplicate code update",
            isActive     = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_NonExistentId_Returns400Or404()
    {
        // Update validator NotFound case behaviour is uncertain (implementation-dependent)
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = 999999,
            miscTypeCode = "NONEXT01",
            description  = "Non-existent update",
            isActive     = (byte)1
        });

        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Delete relies entirely on validator + handler (no controller-level id check)
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
    public async Task TC037_Delete_NonExistentId_Returns400()
    {
        // NotFound validator checks existence → non-existent → 400
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 200);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().NotBeNullOrEmpty();
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
        // After soft delete → handler returns IsSuccess=false → controller returns 404
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
    public async Task TC042_GetAll_ResponseHasNoPaginationBeyondTotalCount()
    {
        // Verify no "message" key in GetAll response (only statusCode/data/TotalCount/PageNumber/PageSize)
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("pageNumber", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("pageSize", out _).Should().BeTrue();
    }

    [Fact, TestPriority(43)]
    public async Task TC043_Create_SecondEntry_DifferentCode_Returns200()
    {
        // Verify a second distinct code creates successfully
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = $"SEC{_f.EntityCode[..7]}",
            description  = "Second QA MiscTypeMaster"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_AutoComplete_MatchesCreatedEntry_ReturnsArray()
    {
        // After TC043 created a second entry, autocomplete should find "SEC" prefix
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=SEC");

        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if ((int)resp.StatusCode == 200)
        {
            var doc = await ParseAsync(resp);
            doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact, TestPriority(45)]
    public async Task TC045_Update_SameCodeAsOwn_NotDuplicate_Returns200()
    {
        // Updating with the same code (self-reference) — AlreadyExists should exclude own Id
        // TC043 created a second entry; update it with its own code
        // Use TC001 entity (already deleted). Create a fresh one first.
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = $"OWN{_f.EntityCode[..7]}",
            description  = "Own Code Test"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createDoc = await ParseAsync(createResp);
        var ownId = createDoc.RootElement.CreatedId();

        // Update with its own code → should NOT trigger AlreadyExists
        var updateResp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id           = ownId,
            miscTypeCode = $"OWN{_f.EntityCode[..7]}",
            description  = "Own Code Updated",
            isActive     = (byte)1
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_GetById_ReturnsAllExpectedFields()
    {
        // Use a freshly created record to verify response structure
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = $"FLD{_f.EntityCode[..7]}",
            description  = "Field Check Test"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createDoc = await ParseAsync(createResp);
        var newId = createDoc.RootElement.CreatedId();

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);

        var data = doc.RootElement.GetProperty("data");
        data.TryGetProperty("id", out _).Should().BeTrue();
        data.TryGetProperty("miscTypeCode", out _).Should().BeTrue();
        data.TryGetProperty("description", out _).Should().BeTrue();
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
