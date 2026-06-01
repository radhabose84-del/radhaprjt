namespace UserManagement.QATests.Tests.FinancialYear;

[Collection("FinancialYearCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class FinancialYearQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/FinancialYear";

    // ⚠ Indian FY validation:
    //   StartYear  : 4-digit year string
    //   StartDate  : must equal April 1 of StartYear
    //   EndDate    : must equal March 31 of (StartYear+1)  (StartDate + 1yr - 1day)
    // Use a far-future year to avoid colliding with real data.
    private const int    FyStartYear = 2090;
    private static string StartYearStr => FyStartYear.ToString();
    private static string StartDateStr => $"{FyStartYear}-04-01T00:00:00";
    private static string EndDateStr   => $"{FyStartYear + 1}-03-31T00:00:00";
    private static string FinYearName  => $"FY{FyStartYear}-{(FyStartYear + 1) % 100}";

    public FinancialYearQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // Returns FinancialYearDto in data → data.id
    // No uniqueness check → multiple FYs with same year allowed
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName
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
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_StartYearEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = "",
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Start Year");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_FinYearNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = ""
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_StartYearNotFourDigits_Returns400()
    {
        // StartYear must be a 4-digit year
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = "90",
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("4-digit");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_StartDateNotApril1_Returns400()
    {
        // StartDate must be April 1 of StartYear
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = $"{FyStartYear}-01-01T00:00:00",   // Jan 1, not April 1
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("April 1st");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EndDateNotMarch31_Returns400()
    {
        // EndDate must be March 31 of next year
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = $"{FyStartYear + 1}-12-31T00:00:00",   // Dec 31, not Mar 31
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("March 31st");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_StartYearNonNumeric_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = "ABCD",
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("4-digit");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_SecondYear_DifferentRange_Returns200()
    {
        // No uniqueness check; create FY for a different year
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = (FyStartYear + 5).ToString(),
            startDate   = $"{FyStartYear + 5}-04-01T00:00:00",
            endDate     = $"{FyStartYear + 6}-03-31T00:00:00",
            finYearName = $"FY{FyStartYear + 5}-{(FyStartYear + 6) % 100}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Returns 404 when no records match (not 200+empty)
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={FyStartYear}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_NoMatchSearch_Returns404()
    {
        // Returns 404 (not 200+empty) when no records match
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
    // SECTION 4 — AUTOCOMPLETE  (by-Year?year=...)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_AutoComplete_WithYear_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-Year?year={FyStartYear}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_AutoComplete_EmptyYear_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-Year");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-Year?year={FyStartYear}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // Controller pre-queries GetById → 404 for non-existent Id
    // Same date validation as Create; IsActive is byte (not validated)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(23)]
    public async Task TC023_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = $"{FinYearName} Updated",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("updated");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = $"{FinYearName} Updated",
            isActive    = (byte)0
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = $"{FinYearName} Updated",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_StartYearEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = "",
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Start Year");
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_FinYearNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = "",
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_StartDateNotApril1_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = $"{FyStartYear}-06-15T00:00:00",   // not April 1
            endDate     = EndDateStr,
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("April 1st");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_EndDateNotMarch31_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = $"{FyStartYear + 1}-06-30T00:00:00",   // not March 31
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("March 31st");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_StartYearNotFourDigits_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = _f.CreatedId,
            startYear   = "12",
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("4-digit");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id          = 999999,
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = EndDateStr,
            finYearName = FinYearName,
            isActive    = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // Controller pre-queries GetById → 404 for non-existent
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(34)]
    public async Task TC034_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_IdZero_Returns404Or400()
    {
        // GetById(0) → null → 404; or validator NotEmpty fires → 400
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("deleted");
    }

    [Fact, TestPriority(38)]
    public async Task TC038_Delete_AlreadyDeleted_Returns404()
    {
        // After soft delete, GetById returns null → controller returns 404
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(39)]
    public async Task TC039_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // GetById has no null check → soft-deleted FY returns 200+null
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
    public async Task TC041_Create_EndDateOneYearOff_Returns400()
    {
        // EndDate = March 31 but wrong year (same year as start) → validation fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = StartDateStr,
            endDate     = $"{FyStartYear}-03-31T00:00:00",   // same year, not next year
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("March 31st");
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Create_StartDateWrongYear_Returns400()
    {
        // StartDate April 1 but of a different year than StartYear
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            startDate   = $"{FyStartYear + 1}-04-01T00:00:00",   // April 1 of wrong year
            endDate     = EndDateStr,
            finYearName = FinYearName
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("April 1st");
    }

    [Fact, TestPriority(43)]
    public async Task TC043_AutoComplete_NoMatch_Returns200_WithEmptyArray()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-Year?year=1850");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_Create_LeapYearFinancialYear_Returns200()
    {
        // FY starting 2092 (a leap year) — EndDate still March 31 (2093-03-31)
        var leap = 2092;
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = leap.ToString(),
            startDate   = $"{leap}-04-01T00:00:00",
            endDate     = $"{leap + 1}-03-31T00:00:00",
            finYearName = $"FY{leap}-{(leap + 1) % 100}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_GetById_ReturnsExpectedFields()
    {
        // Create a fresh FY, verify response structure
        var year = 2095;
        var createResp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = year.ToString(),
            startDate   = $"{year}-04-01T00:00:00",
            endDate     = $"{year + 1}-03-31T00:00:00",
            finYearName = $"FY{year}-{(year + 1) % 100}"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var cDoc  = await ParseAsync(createResp);
        var newId = cDoc.RootElement.CreatedId();

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{newId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.TryGetProperty("id", out _).Should().BeTrue();
        data.TryGetProperty("startYear", out _).Should().BeTrue();
        data.TryGetProperty("finYearName", out _).Should().BeTrue();
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Create_MissingDates_Returns400()
    {
        // StartDate/EndDate default (DateTime.MinValue) → date validation fails
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            startYear   = StartYearStr,
            finYearName = FinYearName
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
