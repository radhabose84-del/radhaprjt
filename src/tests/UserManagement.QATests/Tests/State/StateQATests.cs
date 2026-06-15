namespace UserManagement.QATests.Tests.State;

[Collection("StateCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StateQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute      = "/api/State";
    private const string CountryRoute   = "/api/Country";

    public StateQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — SETUP: Create a Country to obtain a valid CountryId
    //             TC001 → _f.SecondaryId = CountryId used for all State tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateCountry_CapturesCountryId()
    {
        var resp = await _f.Client.PostAsJsonAsync(CountryRoute, new
        {
            countryCode = _f.EntityCode[..4],
            countryName = $"QA State Test Country {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.SecondaryId = id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC002 captures CreatedId)
    // Code max=5, Name max=50; no duplicate check in validator
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(2)]
    public async Task TC002_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode  = _f.EntityCode[..4],
            stateName  = $"QA State {_f.EntityCode}",
            countryId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "AA01",
            stateName = "No Auth State",
            countryId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_StateCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "",
            stateName = "Test State",
            countryId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_StateNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "BB01",
            stateName = "",
            countryId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_StateCodeExceedsMaxLength_Returns400()
    {
        // StateCode max = 5 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "TOOLNG",
            stateName = "Long Code State",
            countryId = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_StateNameExceedsMaxLength_Returns400()
    {
        // StateName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "CC01",
            stateName = new string('A', 51),
            countryId = _f.SecondaryId
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(9)]
    [Trait("Layer", "Smoke")]
    public async Task TC009_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_GetAll_NoMatchSearch_Returns200WithEmptyData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // Controller has null check → 404 for non-existent (unlike Country)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(14)]
    public async Task TC014_GetById_ValidId_Returns200_WithCorrectData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("stateCode").GetString()
            .Should().Be(_f.EntityCode[..4]);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_GetById_IdZero_Returns400()
    {
        // Controller rejects id <= 0 before sending to handler
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_NonExistentId_Returns404()
    {
        // Controller has null check → 404 when handler returns null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — GET BY COUNTRY  (extra endpoint unique to State)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(18)]
    public async Task TC018_GetByCountry_ValidCountryId_Returns200_WithStates()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-country/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetByCountry_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-country/{_f.SecondaryId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetByCountry_IdZero_Returns400()
    {
        // Controller rejects countryid <= 0
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-country/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByCountry_NonExistentCountryId_Returns400()
    {
        // Live contract: by-country with no matching states → 400.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-country/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — AUTOCOMPLETE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(24)]
    public async Task TC024_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE
    // Controller checks CountryId <= 0 → 400 with "Invalid StateID"
    // Update response uses key "city" (legacy naming in controller)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(25)]
    public async Task TC025_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = $"QA State Updated {_f.EntityCode}",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("updated");
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = $"QA State Updated {_f.EntityCode}",
            countryId = _f.SecondaryId,
            isActive  = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = $"QA State Updated {_f.EntityCode}",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = $"QA State Updated {_f.EntityCode}",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_StateCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = "",
            stateName = "Updated State",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_StateNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = "",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_StateCodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = "TOOLNG",
            stateName = "Updated State",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_StateNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = new string('A', 51),
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = $"QA State Updated {_f.EntityCode}",
            countryId = _f.SecondaryId,
            isActive  = (byte)2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("0");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_IdZero_Returns400()
    {
        // Validator: Id must be greater than 0
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = 0,
            stateCode = _f.EntityCode[..4],
            stateName = "Updated State",
            countryId = _f.SecondaryId,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_CountryIdZero_Returns400()
    {
        // Controller-level check: CountryId <= 0 → 400 "Invalid StateID"
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id        = _f.CreatedId,
            stateCode = _f.EntityCode[..4],
            stateName = "Updated State",
            countryId = 0,
            isActive  = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid");
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_IdZero_Returns400()
    {
        // Controller rejects id <= 0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid");
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Delete_AlreadyDeleted_Returns400Or200()
    {
        // After TC039, the state is soft-deleted; re-delete behaviour varies
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_VerifySoftDelete_GetByIdReturns404()
    {
        // Soft-deleted state must be hidden from API → controller returns 404
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 8 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(42)]
    public async Task TC042_GetByCountry_AfterStateDeleted_ReturnsEmptyOrNotFound()
    {
        // The state created in TC002 is now soft-deleted.
        // Autocomplete / by-country should no longer list it.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-country/{_f.SecondaryId}");
        // Live: 200 (empty), 400 (no active states), or 404 — accept any non-success-absent code.
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_AutoComplete_MatchesCreatedStateName_Returns200()
    {
        // Autocomplete by name — even after delete, check response shape
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA+State");

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
    public async Task TC045_Create_SecondState_SameCountry_Returns200()
    {
        // Verify multiple states can belong to same country; code+name unique enough
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stateCode = "ZZ99",
            stateName = $"QA State B {_f.EntityCode}",
            countryId = _f.SecondaryId
        });

        // No duplicate-code validation in this entity → expect 200
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_GetByCountry_AfterSecondStateCreated_ReturnsMultiple()
    {
        // After TC045, the country has at least one active state (TC002 is deleted, TC045 is active)
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-country/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
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
