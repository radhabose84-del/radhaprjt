namespace UserManagement.QATests.Tests.Geography;

// ─────────────────────────────────────────────────────────────────────────────
// Geography chain: Country → State → City (FK-chained — single file ensures
// sequential execution so captured IDs flow correctly between sections)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CountryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer",
                 "Shared.QAInfrastructure")]
public sealed class CountryStateCityQATests
{
    private readonly QAServerFixture _f;

    // Routes
    private const string Country = "/api/Country";
    private const string State   = "/api/State";
    private const string City    = "/api/City";

    // Shared state — captured during create tests
    private static int _countryId;
    private static int _stateId;
    private static int _cityId;

    // Unique codes per run
    private static readonly string CountryCode = $"C{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 9999:D4}";
    private static readonly string StateCode   = $"S{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 9999:D4}";
    private static readonly string CityCode    = $"Y{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 9999:D4}";

    public CountryStateCityQATests(QAServerFixture fixture) => _f = fixture;

    // =========================================================================
    // SECTION 1 — COUNTRY
    // =========================================================================

    [Fact, TestPriority(1)]
    public async Task TC001_Country_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = CountryCode,
            countryName = $"Test Country {CountryCode}"
        });

        await QAHelper.AssertOkAsync(resp);
        _countryId = await QAHelper.GetCreatedIdAsync(resp);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Country_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Country, new
        {
            countryCode = CountryCode,
            countryName = "No Auth Country"
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Country_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = "",
            countryName = "Test Country"
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Country_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = "TCODE",
            countryName = ""
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Country_Create_CodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = "ABCDEF",   // max is 5
            countryName = "Test Country"
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Country_Create_NameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = "TCODE",
            countryName = QAHelper.LongString(51)   // max is 50
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Country_Create_DuplicateCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Country, new
        {
            countryCode = CountryCode,   // same as TC001
            countryName = "Duplicate Country"
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "already exists");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Country_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Country}?PageNumber=1&PageSize=10");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Country_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Country}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Country_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync(
            $"{Country}?PageNumber=1&PageSize=10&SearchTerm={CountryCode}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Country_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Country}/{_countryId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_countryId);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_Country_GetById_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Country}/{_countryId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_Country_GetById_NonExistent_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{Country}/999999");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Country_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Country}/by-name?name={CountryCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Country_AutoComplete_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Country}/by-name?name={CountryCode}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_Country_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(Country, new
        {
            id          = _countryId,
            countryCode = CountryCode,
            countryName = $"Updated Country {CountryCode}",
            isActive    = 1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_Country_Update_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(Country, new
        {
            id          = _countryId,
            countryCode = CountryCode,
            countryName = "Updated Country",
            isActive    = 1
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_Country_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Country, new
        {
            id          = 0,
            countryCode = CountryCode,
            countryName = "Updated Country",
            isActive    = 1
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_Country_Update_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Country, new
        {
            id          = _countryId,
            countryCode = CountryCode,
            countryName = "",
            isActive    = 1
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(20)]
    public async Task TC020_Country_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(Country, new
        {
            id          = _countryId,
            countryCode = CountryCode,
            countryName = "Updated Country",
            isActive    = 2
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "0 or 1");
    }

    // =========================================================================
    // SECTION 2 — STATE  (requires _countryId from TC001)
    // =========================================================================

    [Fact, TestPriority(21)]
    public async Task TC021_State_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = StateCode,
            stateName = $"Test State {StateCode}",
            countryId = _countryId
        });
        await QAHelper.AssertOkAsync(resp);
        _stateId = await QAHelper.GetCreatedIdAsync(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_State_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(State, new
        {
            stateCode = StateCode,
            stateName = "No Auth State",
            countryId = _countryId
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_State_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = "",
            stateName = "Test State",
            countryId = _countryId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_State_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = "SCODE",
            stateName = "",
            countryId = _countryId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(25)]
    public async Task TC025_State_Create_CodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = "ABCDEF",   // max is 5
            stateName = "Test State",
            countryId = _countryId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(26)]
    public async Task TC026_State_Create_CountryIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = "SCODE",
            stateName = "Test State",
            countryId = 0
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_State_Create_NonExistentCountryId_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(State, new
        {
            stateCode = "SCODE",
            stateName = "Test State",
            countryId = 999999
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_State_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{State}?PageNumber=1&PageSize=10");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(29)]
    public async Task TC029_State_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{State}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_State_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync(
            $"{State}?PageNumber=1&PageSize=10&SearchTerm={StateCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_State_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{State}/{_stateId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_stateId);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_State_GetById_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{State}/{_stateId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_State_GetById_NonExistent_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{State}/999999");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_State_GetByCountry_ValidCountryId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{State}/by-country/{_countryId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_State_GetByCountry_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{State}/by-country/{_countryId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_State_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{State}/by-name?name={StateCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_State_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(State, new
        {
            id        = _stateId,
            stateCode = StateCode,
            stateName = $"Updated State {StateCode}",
            countryId = _countryId,
            isActive  = 1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_State_Update_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(State, new
        {
            id        = _stateId,
            stateCode = StateCode,
            stateName = "Updated State",
            countryId = _countryId,
            isActive  = 1
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_State_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(State, new
        {
            id        = 0,
            stateCode = StateCode,
            stateName = "Updated State",
            countryId = _countryId,
            isActive  = 1
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_State_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(State, new
        {
            id        = _stateId,
            stateCode = StateCode,
            stateName = "Updated State",
            countryId = _countryId,
            isActive  = 2
        });
        await QAHelper.Assert400Async(resp);
    }

    // =========================================================================
    // SECTION 3 — CITY  (requires _stateId from TC021)
    // =========================================================================

    [Fact, TestPriority(41)]
    public async Task TC041_City_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = CityCode,
            cityName = $"Test City {CityCode}",
            stateId  = _stateId
        });
        await QAHelper.AssertOkAsync(resp);
        _cityId = await QAHelper.GetCreatedIdAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_City_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(City, new
        {
            cityCode = CityCode,
            cityName = "No Auth City",
            stateId  = _stateId
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_City_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = "",
            cityName = "Test City",
            stateId  = _stateId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(44)]
    public async Task TC044_City_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = "CCODE",
            cityName = "",
            stateId  = _stateId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(45)]
    public async Task TC045_City_Create_CodeExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = "ABCDEF",   // max is 5
            cityName = "Test City",
            stateId  = _stateId
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(46)]
    public async Task TC046_City_Create_StateIdZero_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = "CCODE",
            cityName = "Test City",
            stateId  = 0
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(47)]
    public async Task TC047_City_Create_NonExistentStateId_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(City, new
        {
            cityCode = "CCODE",
            cityName = "Test City",
            stateId  = 999999
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(48)]
    public async Task TC048_City_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{City}?PageNumber=1&PageSize=10");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(49)]
    public async Task TC049_City_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{City}?PageNumber=1&PageSize=10");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(50)]
    public async Task TC050_City_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync(
            $"{City}?PageNumber=1&PageSize=10&SearchTerm={CityCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_City_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{City}/{_cityId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_cityId);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_City_GetById_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{City}/{_cityId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_City_GetById_NonExistent_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{City}/999999");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_City_GetByState_ValidStateId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{City}/by-state/{_stateId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_City_GetByState_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{City}/by-state/{_stateId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_City_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{City}/by-name?name={CityCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_City_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(City, new
        {
            id       = _cityId,
            cityCode = CityCode,
            cityName = $"Updated City {CityCode}",
            stateId  = _stateId,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(58)]
    public async Task TC058_City_Update_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(City, new
        {
            id       = _cityId,
            cityCode = CityCode,
            cityName = "Updated City",
            stateId  = _stateId,
            isActive = 1
        });
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(59)]
    public async Task TC059_City_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(City, new
        {
            id       = 0,
            cityCode = CityCode,
            cityName = "Updated City",
            stateId  = _stateId,
            isActive = 1
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(60)]
    public async Task TC060_City_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(City, new
        {
            id       = _cityId,
            cityCode = CityCode,
            cityName = "Updated City",
            stateId  = _stateId,
            isActive = 5
        });
        await QAHelper.Assert400Async(resp);
    }

    // =========================================================================
    // SECTION 4 — DELETE SEQUENCE (reverse FK: City → State → Country)
    // =========================================================================

    [Fact, TestPriority(61)]
    public async Task TC061_Country_Delete_BlockedWhenStateLinked_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{Country}/{_countryId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "linked");
    }

    [Fact, TestPriority(62)]
    public async Task TC062_State_Delete_BlockedWhenCityLinked_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{State}/{_stateId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "linked");
    }

    [Fact, TestPriority(63)]
    public async Task TC063_City_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{City}/{_cityId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(64)]
    public async Task TC064_City_VerifySoftDeleted_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{City}/{_cityId}");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(65)]
    public async Task TC065_State_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{State}/{_stateId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(66)]
    public async Task TC066_State_VerifySoftDeleted_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{State}/{_stateId}");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(67)]
    public async Task TC067_Country_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{Country}/{_countryId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(68)]
    public async Task TC068_Country_VerifySoftDeleted_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{Country}/{_countryId}");
        await QAHelper.Assert404Async(resp);
    }

    [Fact, TestPriority(69)]
    public async Task TC069_Country_Delete_NonExistentId_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{Country}/999999");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(70)]
    public async Task TC070_Country_Delete_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{Country}/999999");
        await QAHelper.Assert401Async(resp);
    }
}
