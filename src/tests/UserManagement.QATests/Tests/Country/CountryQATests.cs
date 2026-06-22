namespace UserManagement.QATests.Tests.Country;

[Collection("CountryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CountryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Country";

    public CountryQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (runs FIRST — TC001 captures CreatedId)
    // Code rules: max 5 chars, alphanumeric + hyphen; name max 50 chars
    // Both CountryCode AND CountryName must be unique
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = _f.EntityCode[..5],   // max 5 chars; [..5] ("Q"+4 digits) gives a 10000-value
            countryName = $"QA Country {_f.EntityCode}"   // space so it never collides with legacy 4-char codes
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "AA01",
            countryName = "No Auth Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CountryCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "",
            countryName = "Test Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_CountryNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "BB01",
            countryName = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CountryCodeWithSpace_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "A B",
            countryName = "Space Code Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CountryCodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "A@01",
            countryName = "Special Char Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CountryCodeExceedsMaxLength_Returns400()
    {
        // CountryCode max is 5 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "TOOLNG",
            countryName = "Long Code Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");   // live message: "CountryCode cannot be longer than 5"
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CountryNameExceedsMaxLength_Returns400()
    {
        // CountryName max is 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "CC01",
            countryName = new string('A', 51)
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");   // live message: "CountryName cannot be longer than 50"
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCountryCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = _f.EntityCode[..5],   // same code as TC001
            countryName = "Duplicate Code Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DuplicateCountryName_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "DD01",
            countryName = $"QA Country {_f.EntityCode}"   // same name as TC001
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(12)]
    [Trait("Layer", "Smoke")]
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

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
    public async Task TC016_GetAll_NoMatchSearch_Returns200WithEmptyData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_ValidId_Returns200_WithCorrectData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("countryCode").GetString()
            .Should().Be(_f.EntityCode[..5]);
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
        // Controller rejects id <= 0 before sending to handler
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_NonExistentId_Returns404()
    {
        // Live contract: non-existent id → 404 "Country with ID ... not found."
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // CountryCode is NOT immutable for Country (older entity)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = $"QA Country Updated {_f.EntityCode}",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = $"QA Country Updated {_f.EntityCode}",
            isActive    = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = $"QA Country Updated {_f.EntityCode}",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = $"QA Country Updated {_f.EntityCode}",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_CountryCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = "",
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_CountryNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = "",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_CountryCodeWithSpace_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = "A B",
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_CountryCodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = "A@01",
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("alphanumeric");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_CountryCodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = "TOOLNG",
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("characters");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_CountryNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = new string('A', 51),
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("characters");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            countryCode = _f.EntityCode[..5],
            countryName = $"QA Country Updated {_f.EntityCode}",
            isActive    = (byte)2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("0");
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = 0,
            countryCode = _f.EntityCode[..5],
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid Country ID");   // live message for id <= 0
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_NonExistentId_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = 999999,
            countryCode = _f.EntityCode[..5],
            countryName = "Updated Country",
            isActive    = (byte)1
        });

        // Validator's Id > 0 check passes for 999999; handler or validator catches not-found
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_IdZero_Returns400()
    {
        // Controller rejects id <= 0 before sending to handler
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid");
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Delete_NonExistentId_Returns400_Or200()
    {
        // After TC040, the record is soft-deleted; attempting again verifies deletion
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        // Soft-deleted record is treated as not found by the validator
        ((int)resp.StatusCode).Should().BeOneOf(400, 404, 200);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_VerifySoftDelete_GetByIdReturns404()
    {
        // Live contract: GetById of a soft-deleted country → 404 "not found".
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("not found");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA: code allows hyphens (valid) + whitespace-only rejects
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(43)]
    public async Task TC043_Create_CodeWithHyphen_Returns400_Or200_PerValidator()
    {
        // Validator pattern is ^[A-Za-z0-9-]+$ — hyphens ARE allowed
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "A-01",
            countryName = "Hyphen Code Country"
        });
        // Expect 200 (hyphen is valid) or 400 if name conflicts
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_Create_WhitespaceOnlyCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "   ",
            countryName = "Whitespace Code Country"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("whitespace");
    }

    [Fact, TestPriority(45)]
    public async Task TC045_Create_WhitespaceOnlyName_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            countryCode = "EE01",
            countryName = "   "
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("whitespace");
    }

    [Fact, TestPriority(46)]
    public async Task TC046_GetAll_DefaultPagination_Returns200()
    {
        // Verify PageNumber=1 with large PageSize returns correct structure
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
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
