namespace UserManagement.QATests.Tests.Division;

[Collection("DivisionCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DivisionQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Division";

    // CompanyId=1 assumed to exist in QA DB (cross-module FK, no DB constraint).
    // NOTE: testsales JWT company = 0, so company-scoped reads can't see these rows — this
    // collection is blocked by the test-user/company mismatch (needs a real-company test user).
    private const int QACompanyId = 1;

    public DivisionQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // Create returns DivisionDTO in data → data.id gives new Id
    // CompanyId validated via "MinLength" case: must be >= 1
    // Name max=100, ShortName max=50 (much larger than Department)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = _f.EntityCode[..6],
            name      = $"QA Division {_f.EntityCode}",
            companyId = QACompanyId
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
            shortName = "NOAU",
            name      = "No Auth Division",
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "TSTD",
            name      = "",
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_ShortNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "",
            name      = "Test Division",
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_NameExceedsMaxLength_Returns400()
    {
        // Name max = 100 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "TSTD",
            name      = new string('A', 101),
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_ShortNameExceedsMaxLength_Returns400()
    {
        // ShortName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = new string('A', 51),
            name      = "Test Division",
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CompanyIdZero_Returns400()
    {
        // "MinLength" case: CompanyId must be >= 1; 0 fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "NOCMP",
            name      = "No Company Division",
            companyId = 0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("CompanyId");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NameAtMaxLength_Returns200()
    {
        // Exactly 100 chars Name — boundary test should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "BDRY",
            name      = new string('B', 100),
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_ShortNameAtMaxLength_Returns200()
    {
        // Exactly 50 chars ShortName — boundary test should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = new string('S', 50),
            name      = $"QA Division Max Short {_f.EntityCode}",
            companyId = QACompanyId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ⚠ Always returns 200 — NO empty/null check (unlike Department/DeptGroup)
    // Empty result → 200 with empty data array (not 404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(11)]
    [Trait("Layer", "Smoke")]
    public async Task TC011_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
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
        // The division list is scoped to the user's assigned divisions in their company
        // (UserDivision + CompanyId). A QA-created division isn't assigned to the test user,
        // so it legitimately may not appear — assert the response shape, not a match.
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
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
        // ⚠ Division GetAll always 200 — no 404 for empty results
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
        doc.RootElement.GetProperty("data").GetProperty("name").GetString()
            .Should().Contain("QA Division");
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
        // No null check in GetById action → 200 with data:null
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
    // Uses Companies JWT claim for filtering; always returns 200
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
    // SECTION 5 — UNITS BY DIVISION  (extra endpoint)
    // Uses CompanyId from JWT claim; always returns 200
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(23)]
    public async Task TC023_UnitsByDivision_ValidDivisionId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/units-by-division?divisionId={_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(24)]
    public async Task TC024_UnitsByDivision_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/units-by-division?divisionId={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_UnitsByDivision_DivisionIdZero_Returns200_WithEmptyArray()
    {
        // No validation on divisionId — always 200; no division id=0 exists → empty array
        var resp = await _f.Client.GetAsync($"{BaseRoute}/units-by-division?divisionId=0");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE
    // Controller pre-queries GetById → 404 for non-existent Id
    // IsActive is byte (not Status enum)
    // CompanyId validated: must be >= 1
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(26)]
    public async Task TC026_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = $"QA Division Updated {_f.EntityCode}",
            companyId = QACompanyId,
            isActive  = (byte)1
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
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = $"QA Division Updated {_f.EntityCode}",
            companyId = QACompanyId,
            isActive  = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = $"QA Division Updated {_f.EntityCode}",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = "Updated Division",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = "",
            companyId = QACompanyId,
            isActive  = (byte)1
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
            id        = _f.CreatedId,
            shortName = "",
            name      = "Updated Division",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_NameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = new string('A', 101),
            companyId = QACompanyId,
            isActive  = (byte)1
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
            id        = _f.CreatedId,
            shortName = new string('A', 51),
            name      = "Updated Division",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_CompanyIdZero_Returns400()
    {
        // MinLength case: CompanyId must be >= 1
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = "Updated Division",
            companyId = 0,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("CompanyId");
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = 999999,
            shortName = "NONEX",
            name      = "Non-existent Division",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // ⚠ NO pre-query check — controller just sends command → always 200
    // (Unlike Department which pre-queries and may return 404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_IdZero_Returns400()
    {
        // NotEmpty validator fires for id=0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_NonExistentId_Returns200()
    {
        // ⚠ No pre-query — controller ignores result → always 200
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
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
    public async Task TC041_Delete_AlreadyDeleted_Returns200Or400()
    {
        // SoftDelete validator may fire; controller always returns 200 anyway
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // GetById has no null check → soft-deleted division returns 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 8 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(43)]
    public async Task TC043_Update_IdZero_Returns404()
    {
        // Controller: GetById(0) → null → 404 (pre-query check fires)
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = 0,
            shortName = "ZERO",
            name      = "Zero Id Division",
            companyId = QACompanyId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_GetAll_LargePageSize_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_AutoComplete_NoMatch_Returns200_WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Create_SecondDivision_SameCompany_Returns200()
    {
        // Multiple divisions per company — no duplicate check
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            shortName = "SEC",
            name      = $"QA Second Division {_f.EntityCode}",
            companyId = QACompanyId
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
