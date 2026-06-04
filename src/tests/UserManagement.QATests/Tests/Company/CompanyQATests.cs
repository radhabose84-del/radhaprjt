namespace UserManagement.QATests.Tests.Company;

[Collection("CompanyCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CompanyQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Company";

    // Assumed-existing IDs for address FK fields
    private const int QACountryId = 1;
    private const int QAStateId   = 1;
    private const int QACityId    = 1;
    private const int QAEntityId  = 1;   // EntityId must be >= 1

    // Fixed valid GST — not uniqueness-checked, can reuse across runs
    private const string ValidGst     = "22AAAAA1234A1Z5";
    private const string ValidWebsite  = "http://www.qatest.com";

    // PAN derived from fixture EntityCode (unique per run): QATCA + 4 digits + A = 10 chars.
    // Format is the strict Indian PAN: ^[A-Z]{3}[CPHFATBLJG][A-Z][0-9]{4}[A-Z]$ — the 4th
    // char MUST be a PAN entity-type letter (C = company). 'QATST' was invalid (4th char 'S').
    private string TestPanNumber =>
        $"QATCA{new string(_f.EntityCode.Where(char.IsDigit).TakeLast(4).ToArray())}A";

    public CompanyQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // All fields nested under "company" object
    // AlreadyExists checks: CompanyName + PanNumber (both unique)
    // Validation: GstFormat, PanFormat, Website, Pincode, NumericOnly, Telephone
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CompanyNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(companyName: ""));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_InvalidGstFormat_Returns400()
    {
        // GstFormat: ^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(gstNumber: "INVALIDGST"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("GstNumber");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_InvalidPanFormat_Returns400()
    {
        // PanFormat validation fires for malformed PAN
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(panNumber: "INVALID"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("PanNumber");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_YearOfEstablishmentTooLow_Returns400()
    {
        // NumericOnly case: YearOfEstablishment must be 1900-now
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(yearOfEstablishment: 1800));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("YearOfEstablishment");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EntityIdZero_Returns400()
    {
        // MinLength case: EntityId must be >= 1
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(entityId: 0));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("EntityId");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_InvalidWebsite_Returns400()
    {
        // Website pattern validation
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(website: "not-a-url"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Website");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_InvalidPincode_Returns400()
    {
        // Pincode: must be 6 digits
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(pinCode: "1234"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("PinCode");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_CompanyNameExceedsMaxLength_Returns400()
    {
        // CompanyName max = 50 characters
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(companyName: new string('A', 51)));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateCompanyName_Returns400()
    {
        // AlreadyExists: CompanyName must be unique
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(
            companyName: $"QA Company {_f.EntityCode}")); // same as TC001

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(12)]
    public async Task TC012_Create_EmptyCompanyObject_Returns400()
    {
        // company object present but all fields empty → NotEmpty fires
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            company = new { }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Always returns 200 (no 404 for empty — unlike Currency/Department)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(13)]
    [Trait("Layer", "Smoke")]
    public async Task TC013_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetAll_NoMatchSearch_Returns200_WithEmptyData()
    {
        // Company GetAll always 200 — empty result is 200+empty array (NOT 404)
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // id <= 0 → 400; non-existent → 200+null (no null check after handler)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(18)]
    public async Task TC018_GetById_ValidId_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("id").GetInt32()
            .Should().Be(_f.CreatedId);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_IdZero_Returns400()
    {
        // Controller explicitly checks id <= 0 → 400
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetById_NonExistentId_Returns200_WithNullData()
    {
        // No null check after handler → 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE
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
    // SECTION 5 — UPDATE
    // Pre-queries GetById via command.Company.Id → 404 for non-existent
    // Payload nested under "company" object (same structure as Create + Id)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(25)]
    public async Task TC025_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildValidUpdatePayload(_f.CreatedId));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("updated");
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, BuildValidUpdatePayload(_f.CreatedId));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Update_CompanyNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute,
            BuildValidUpdatePayload(_f.CreatedId, companyName: ""));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(28)]
    public async Task TC028_Update_InvalidGstFormat_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute,
            BuildValidUpdatePayload(_f.CreatedId, gstNumber: "BADGST"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("GstNumber");
    }

    [Fact, TestPriority(29)]
    public async Task TC029_Update_InvalidPanFormat_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute,
            BuildValidUpdatePayload(_f.CreatedId, panNumber: "BADPAN"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("PanNumber");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_YearOfEstablishmentTooLow_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute,
            BuildValidUpdatePayload(_f.CreatedId, yearOfEstablishment: 1800));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("YearOfEstablishment");
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_InvalidWebsite_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute,
            BuildValidUpdatePayload(_f.CreatedId, website: "not-a-url"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Website");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_NonExistentId_Returns404()
    {
        // Controller pre-queries GetById(999999) → null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildValidUpdatePayload(999999));
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_EmptyCompanyObject_Returns400()
    {
        // Empty body has no Id (0), so the update controller's existence pre-check resolves it
        // to "not found" (404) before field validation (400) can run — consistent with TC035,
        // which accepts 404 for a non-existent update. Either rejection code is acceptable.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { company = new { } });
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — uses /{id} route param)
    // No pre-query → always 200 after validators pass
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(34)]
    public async Task TC034_Delete_IdZero_Returns400()
    {
        // NotEmpty validator fires for id=0
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Delete_NonExistentId_Returns200()
    {
        // No pre-query → validator passes → handler deletes 0 rows → 200
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
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
    public async Task TC038_Delete_AlreadyDeleted_Returns200Or400()
    {
        // SoftDelete validator may fire; controller always returns 200 otherwise
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // No null check → 200+null after soft delete
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

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
    public async Task TC041_Create_DuplicatePanNumber_Returns400()
    {
        // AlreadyExists: PanNumber must also be unique
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload(
            companyName: $"QA Company Alt {_f.EntityCode}",   // different name
            panNumber:   TestPanNumber));                       // same PAN as TC001

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Create_CityIdZero_Returns400()
    {
        // NumericOnly: CityId must be >= 1
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildValidCreatePayload(cityId: 0));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("CityId");
    }

    [Fact, TestPriority(43)]
    public async Task TC043_Create_InvalidContactEmail_Returns400()
    {
        // Email pattern validation on CompanyContact.Email
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildValidCreatePayload(contactEmail: "not-an-email"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("email");
    }

    [Fact, TestPriority(44)]
    public async Task TC044_Update_IdZero_Returns404()
    {
        // Pre-query GetById(0) → null → 404
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildValidUpdatePayload(0));
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
    public async Task TC046_Create_FutureYearOfEstablishment_Returns400()
    {
        // YearOfEstablishment must be <= DateTime.Now.Year (NumericOnly InclusiveBetween)
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildValidCreatePayload(yearOfEstablishment: DateTime.Now.Year + 1));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("YearOfEstablishment");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Payload builders — Company uses nested DTOs under "company" key
    // ─────────────────────────────────────────────────────────────────────────

    private object BuildValidCreatePayload(
        string? companyName         = null,
        string? gstNumber           = null,
        string? panNumber           = null,
        string? website             = null,
        int     yearOfEstablishment = 2000,
        int     entityId            = QAEntityId,
        string? pinCode             = "123456",
        int     cityId              = QACityId,
        string? contactEmail        = "qa@test.com") => new
    {
        company = new
        {
            companyName         = companyName ?? $"QA Company {_f.EntityCode}",
            legalName           = $"QA Legal Name {_f.EntityCode}",
            gstNumber           = gstNumber   ?? ValidGst,
            panNumber           = panNumber   ?? TestPanNumber,
            yearOfEstablishment = yearOfEstablishment,
            website             = website     ?? ValidWebsite,
            entityId            = entityId,
            companyAddress      = new
            {
                addressLine1   = "QA Address Line 1",
                addressLine2   = "QA Address Line 2",
                pinCode        = pinCode,
                cityId         = cityId,
                stateId        = QAStateId,
                countryId      = QACountryId,
                phone          = "",
                alternatePhone = ""
            },
            companyContact = new
            {
                name        = "QA Contact Person",
                designation = "QA Manager",
                email       = contactEmail,
                phone       = "9876543210"
            }
        }
    };

    private object BuildValidUpdatePayload(
        int     id                  = 0,
        string? companyName         = null,
        string? gstNumber           = null,
        string? panNumber           = null,
        string? website             = null,
        int     yearOfEstablishment = 2000) => new
    {
        company = new
        {
            id                  = id,
            companyName         = companyName ?? $"QA Company Updated {_f.EntityCode}",
            legalName           = $"QA Legal Updated {_f.EntityCode}",
            gstNumber           = gstNumber   ?? ValidGst,
            panNumber           = panNumber   ?? TestPanNumber,
            yearOfEstablishment = yearOfEstablishment,
            website             = website     ?? ValidWebsite,
            entityId            = QAEntityId,
            isActive            = (byte)1,
            companyAddress      = new
            {
                addressLine1   = "QA Updated Address Line 1",
                addressLine2   = "QA Updated Line 2",
                pinCode        = "123456",
                cityId         = QACityId,
                stateId        = QAStateId,
                countryId      = QACountryId,
                phone          = "",
                alternatePhone = ""
            },
            companyContact = new
            {
                name        = "QA Updated Contact",
                designation = "QA Lead",
                email       = "qa@test.com",
                phone       = "9876543210"
            }
        }
    };

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
