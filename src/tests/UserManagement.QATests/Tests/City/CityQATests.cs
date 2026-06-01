namespace UserManagement.QATests.Tests.City;

[Collection("CityCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CityQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute    = "/api/City";
    private const string CountryRoute = "/api/Country";
    private const string StateRoute   = "/api/State";

    public CityQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — SETUP
    //   TC001 → create Country → SecondaryId = countryId (temp)
    //   TC002 → create State   → SecondaryId = stateId  (overwrite; used for all City tests)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateCountry_CapturesCountryId()
    {
        var resp = await _f.Client.PostAsJsonAsync(CountryRoute, new
        {
            countryCode = _f.EntityCode[..4],
            countryName = $"QA City Test Country {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.SecondaryId = id;   // temp: countryId
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Setup_CreateState_CapturesStateId()
    {
        var resp = await _f.Client.PostAsJsonAsync(StateRoute, new
        {
            stateCode = _f.EntityCode[..4],
            stateName = $"QA City Test State {_f.EntityCode}",
            countryId = _f.SecondaryId    // countryId from TC001
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.SecondaryId = id;   // overwrite: now holds stateId
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC003 captures CreatedId)
    // Code max=5, Name max=50; no duplicate / FK validation in Create validator
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(3)]
    public async Task TC003_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode  = _f.EntityCode[..4],
            cityName  = $"QA City {_f.EntityCode}",
            stateId   = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "AA01",
            cityName = "No Auth City",
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CityCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "",
            cityName = "Test City",
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "BB01",
            cityName = "",
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CityCodeExceedsMaxLength_Returns400()
    {
        // CityCode max = 5 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "TOOLNG",
            cityName = "Long Code City",
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CityNameExceedsMaxLength_Returns400()
    {
        // CityName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "CC01",
            cityName = new string('A', 51),
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_SecondCity_SameState_Returns200()
    {
        // Multiple cities may belong to the same state; no duplicate-code check in validator
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            cityCode = "ZZ99",
            cityName = $"QA City B {_f.EntityCode}",
            stateId  = _f.SecondaryId
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
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
    public async Task TC015_GetAll_NoMatchSearch_Returns200WithEmptyData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // Controller has NO null check → non-existent returns 200 with null data
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(16)]
    public async Task TC016_GetById_ValidId_Returns200_WithCorrectData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
        doc.RootElement.GetProperty("data").GetProperty("cityCode").GetString()
            .Should().Be(_f.EntityCode[..4]);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_IdZero_Returns400()
    {
        // Controller rejects id <= 0 before handler
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_NonExistentId_Returns200_WithNullData()
    {
        // Controller has NO null check — handler returns null, controller wraps as data:null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — GET BY STATE  (extra endpoint unique to City)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_GetByState_ValidStateId_Returns200_WithCities()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-state/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByState_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-state/{_f.SecondaryId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetByState_StateIdZero_Returns400()
    {
        // Controller rejects stateid <= 0
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-state/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetByState_NonExistentStateId_Returns404()
    {
        // Controller has null check → 404 when no cities found for that state
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-state/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — AUTOCOMPLETE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE
    // Controller checks StateId <= 0 → 400 "Invalid StateID"
    // Update response uses legacy "City" key
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(27)]
    public async Task TC027_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = $"QA City Updated {_f.EntityCode}",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Updated");
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = $"QA City Updated {_f.EntityCode}",
            stateId  = _f.SecondaryId,
            isActive = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = $"QA City Updated {_f.EntityCode}",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = $"QA City Updated {_f.EntityCode}",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_CityCodeEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = "",
            cityName = "Updated City",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_CityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = "",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_CityCodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = "TOOLNG",
            cityName = "Updated City",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_CityNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = new string('A', 51),
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = $"QA City Updated {_f.EntityCode}",
            stateId  = _f.SecondaryId,
            isActive = (byte)2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("0");
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_IdZero_Returns400()
    {
        // Validator: Id must be greater than 0
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = 0,
            cityCode = _f.EntityCode[..4],
            cityName = "Updated City",
            stateId  = _f.SecondaryId,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Update_StateIdZero_Returns400()
    {
        // Controller-level check: StateId <= 0 → 400 "Invalid StateID"
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id       = _f.CreatedId,
            cityCode = _f.EntityCode[..4],
            cityName = "Updated City",
            stateId  = 0,
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid");
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(39)]
    public async Task TC039_Delete_IdZero_Returns400()
    {
        // Controller rejects id <= 0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid");
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetString()
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Delete_AlreadyDeleted_Returns400Or200()
    {
        // After TC041, re-deleting soft-deleted city — behaviour depends on handler
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        // City controller has no null check → 200 with null data after soft delete
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_GetByState_AfterMainCityDeleted_StillReturnsSecondCity()
    {
        // TC010 created a second city (ZZ99) that is still active
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-state/{_f.SecondaryId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        // At least the second city created in TC010 should still be visible
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_GetAll_LargePageSize_Returns200_WithCorrectStructure()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(100);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_AutoComplete_MatchesCityName_Returns200_WithArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA+City");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
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
