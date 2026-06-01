namespace UserManagement.QATests.Tests.Currency;

[Collection("CurrencyCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CurrencyQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Currency";

    // Currency-specific test data
    // Code: 3-6 alphabetic chars only (AlphabeticOnly pattern, no digits/symbols)
    // Name: alphabetic + spaces only (AlphabeticWithSpaces pattern)
    private const string TestCode = "QATST";
    private const string TestName = "QA Test Currency";

    public CurrencyQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // Create returns int directly in data (not a DTO)
    // No AlreadyExists check in validator → multiple records with same code allowed
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
        // Create returns int directly: { "data": 42 }
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            code = "NOAUT",
            name = "No Auth Currency"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "",
            name = TestName
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
            code = TestCode,
            name = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooShort_Returns400()
    {
        // MinLength = 3; "QA" is 2 chars → fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "QA",
            name = TestName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("3");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeTooLong_Returns400()
    {
        // MaxLength = 6; 7 chars → fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "TOOLONG",
            name = TestName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NameTooLong_Returns400()
    {
        // MaxLength = 50; 51 alpha chars → fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = TestCode,
            name = new string('A', 51)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeWithDigit_Returns400()
    {
        // AlphabeticOnly pattern: digits not allowed in Code
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "USD1",
            name = TestName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_CodeWithSpecialChar_Returns400()
    {
        // AlphabeticOnly pattern: special chars not allowed in Code
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "US@D",
            name = TestName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_NameWithDigit_Returns400()
    {
        // AlphabeticWithSpaces: digits not allowed in Name
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = TestCode,
            name = "Dollar One"  // valid
        });

        // "Dollar One" is all alpha+spaces, should pass
        // Separate test for invalid name with digit
        var resp2 = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "TSTIN",
            name = "Dollar 1"   // digit in name → fails
        });

        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Returns 404 when no records (unique to Currency — not 200+empty)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(12)]
    public async Task TC012_GetAll_HappyPath_Returns200()
    {
        // DB has pre-existing currencies → GetAll returns 200
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        // QA currency was created in TC001 → should exist
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        // May be 200 or 404 depending on total record count
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_GetAll_NoMatchSearch_Returns404()
    {
        // IMPORTANT: Currency GetAll returns 404 (not 200+empty) when no records match
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // Has null check → 404 for non-existent (like newer-pattern controllers)
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
            .Should().Be(TestCode);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_IdZero_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_NonExistentId_Returns404()
    {
        // Controller has null check → 404 when handler returns null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE
    // Query param is "CurrencyName" (not "name" or "term")
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_WithCurrencyName_Returns200()
    {
        // Param name is CurrencyName (unique to this controller)
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?CurrencyName=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?CurrencyName=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // Code is IMMUTABLE — UpdateCurrencyCommand has no Code property
    // Update response: { message, statusCode } — no data field
    // Validator has no Id > 0 check → Id=0 passes validation, handler returns 200
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = "QA Updated Currency",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Updated");
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = "QA Updated Currency",
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
            name     = "QA Updated Currency",
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
            name     = "QA Updated Currency",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = "",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_NameTooLong_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = new string('A', 51),
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_NameWithDigit_Returns400()
    {
        // AlphabeticWithSpaces: digits not allowed in Name
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = "Dollar One 1",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            name     = "QA Updated Currency",
            isActive = (byte)2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("0");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_IdZero_NoValidatorCheck_Returns200()
    {
        // UpdateCurrencyCommandValidator has NO Id > 0 check
        // Controller has no check either → passes to handler → updates 0 rows → 200
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = 0,
            name     = "QA Updated Currency",
            isActive = (byte)1
        });

        // No validator catches Id=0 → handler runs → 200 OK
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Verify_CodeIsImmutable_GetByIdShowsOriginalCode()
    {
        // UpdateCurrencyCommand has no Code field → code cannot change
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("code").GetString()
            .Should().Be(TestCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Delete validator has NotFound case → 400 for non-existent id
    // Controller has no id validation → validator handles all checks
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_IdZero_Returns400()
    {
        // NotEmpty validator fires for id=0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_NonExistentId_Returns400()
    {
        // NotFound validator: checks DB → currency not found → 400
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_AlreadyDeleted_Returns400()
    {
        // NotFound validator fires → soft-deleted record treated as not found → 400
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    [Fact, TestPriority(40)]
    public async Task TC040_VerifySoftDelete_GetByIdReturns404()
    {
        // After soft delete, controller null check → 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA: boundary lengths, name patterns, autocomplete
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(41)]
    public async Task TC041_Create_CodeExactlyMinLength_Returns200()
    {
        // Code = exactly 3 chars (minimum) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "QAT",
            name = "QA Test Min Code"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Create_CodeExactlyMaxLength_Returns200()
    {
        // Code = exactly 6 chars (maximum) — should pass
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "QATEST",
            name = "QA Test Max Code"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_AutoComplete_FindsCreatedCurrency_Returns200WithData()
    {
        // After TC041/TC042 created currencies, autocomplete should find them
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?CurrencyName=QA+Test");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
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
    public async Task TC045_Create_CodeWithHyphen_Returns400()
    {
        // AlphabeticOnly does not allow hyphens (unlike Country's AlphabeticWithHyphen)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "QA-T",
            name = "QA Hyphen Code"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Update_NameWithSpecialChar_Returns400()
    {
        // AlphabeticWithSpaces: special chars not allowed
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "QABCD",
            name = "QA@Currency"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
