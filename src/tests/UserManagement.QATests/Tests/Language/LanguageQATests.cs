namespace UserManagement.QATests.Tests.Language;

[Collection("LanguageCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LanguageQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Language";

    // ⚠ Delete is exposed as PUT api/Language/{id} (NOT HttpDelete)

    public LanguageQATests(QAServerFixture fixture) => _f = fixture;

    // Code max = 10, Name max = 50; simple NotEmpty + MaxLength only
    private string TestCode => _f.EntityCode[..6];   // <= 10 chars
    private string TestName => $"QA Lang {_f.EntityCode}";

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // Returns LanguageDTO in data → data.id; no uniqueness check
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = TestCode,
            name = TestName
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
            code = "NOAU",
            name = "No Auth Language"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "",
            name = "Test Language"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "TSTL",
            name = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeExceedsMaxLength_Returns400()
    {
        // Code max = 10 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "ELEVENCHAR1",   // 11 chars
            name = "Long Code Language"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NameExceedsMaxLength_Returns400()
    {
        // Name max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "TSTL",
            name = new string('A', 51)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeAtMaxLength_Returns200()
    {
        // Exactly 10 chars (max boundary) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "TENCHARSTR",   // exactly 10
            name = $"QA Lang Max Code {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NameAtMaxLength_Returns200()
    {
        // Exactly 50 chars Name (max boundary) — should pass. Must be run-unique (Language Name
        // has a uniqueness check and the QA clone isn't reset between runs), so seed it with the
        // run-unique EntityCode and pad to exactly 50 chars.
        var name50 = (_f.EntityCode + new string('B', 50))[..50];
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "BDRY",
            name = name50
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_SecondLanguage_Returns200()
    {
        // No uniqueness check — duplicate-ish codes allowed
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "SEC",
            name = $"QA Second Lang {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Always returns 200 (no 404 for empty)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(11)]
    [Trait("Layer", "Smoke")]
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

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
        // Language GetAll always 200 — empty result is 200+empty (not 404)
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ⚠ NO null check → always 200, even for non-existent (data = null)
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
        // No null check → 200 with data:null
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

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_NoMatch_Returns200_WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // Controller pre-queries GetById → 404 for non-existent Id
    // IsActive is byte (not validated)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = $"QA Lang Updated {_f.EntityCode}",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("updated");
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = $"QA Lang Updated {_f.EntityCode}",
            isActive = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = $"QA Lang Updated {_f.EntityCode}",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = "Updated Language",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = "",
            name     = "Updated Language",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = "",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_CodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = "ELEVENCHAR1",   // 11 chars
            name     = "Updated Language",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_NameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            code     = TestCode,
            name     = new string('A', 51),
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = 999999,
            code     = "NONEX",
            name     = "Non-existent Language",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_EmptyBody_Returns400()
    {
        // Empty body has no Id (0), so the update controller's existence pre-check resolves it to
        // "not found" (404) before field validation (400) can run. Either rejection code is fine.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST)
    // ⚠ Delete is exposed as PUT api/Language/{id}  (NOT HttpDelete)
    // No pre-query → always 200
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(34)]
    public async Task TC034_Delete_NoAuthToken_Returns401()
    {
        // Delete is a PUT to /{id}
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/{_f.CreatedId}", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_UsingHttpDelete_Returns405()
    {
        // There is NO HttpDelete handler — using DELETE verb should not match
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(404, 405);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_NonExistentId_Returns200()
    {
        // PUT /{id} delete — no pre-query check → 200 even for non-existent
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/999999", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/{_f.CreatedId}", new { });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("deleted");
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_AlreadyDeleted_Returns200()
    {
        // No pre-query → always 200 even if already soft-deleted
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/{_f.CreatedId}", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(39)]
    public async Task TC039_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // GetById has no null check → soft-deleted language returns 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_GetAll_LargePageSize_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_GetAll_AfterDelete_StillFindsSecondLang()
    {
        // TC010 created a second language still active
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA+Second");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Create_VerifyResponseFields()
    {
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "FLD",
            name = $"QA Field Check {_f.EntityCode}"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createDoc = await ParseAsync(createResp);
        var newId = createDoc.RootElement.CreatedId();

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.TryGetProperty("code", out _).Should().BeTrue();
        data.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact, TestPriority(43)]
    public async Task TC043_Update_VerifyChange_GetByIdReflectsUpdate()
    {
        // Create fresh, update name, verify via GetById
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "UPD",
            name = $"QA Pre-Update Lang {_f.EntityCode}"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cDoc  = await ParseAsync(createResp);
        var newId = cDoc.RootElement.CreatedId();

        var updResp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = newId,
            code     = "UPD",
            name     = $"QA Post-Update Lang {_f.EntityCode}",
            isActive = (byte)1
        });
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(getResp);
        doc.RootElement.GetProperty("data").GetProperty("name").GetString()
            .Should().Contain("Post-Update");
    }

    [Fact, TestPriority(44)]
    public async Task TC044_AutoComplete_FindsCreatedLanguage_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA+Lang");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_Create_NameWithSpaces_Returns200()
    {
        // Spaces in name allowed (no format restriction)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "SPC",
            name = $"QA Lang With Spaces {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_GetAll_DefaultPagination_StructureValid()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=10");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
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
