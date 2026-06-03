namespace UserManagement.QATests.Tests.Unit;

[Collection("UnitCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class UnitQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute      = "/api/Unit";
    private const string UpdateRoute    = "/api/Unit/update";
    private const string MiscTypeRoute  = "/api/usermanagement/MiscTypeMaster";
    private const string MiscMastRoute  = "/api/usermanagement/MiscMaster";

    // Assumed-existing IDs in QA DB (no FK constraints enforced for cross-module).
    // NOTE: testsales JWT company = 0, so company-scoped reads can't see these rows — this
    // collection is blocked by the test-user/company mismatch (needs a real-company test user).
    private const int QACompanyId  = 1;
    private const int QADivisionId = 1;
    private const int QACountryId  = 1;
    private const int QAStateId    = 1;
    private const int QACityId     = 1;

    public UnitQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — SETUP
    //   TC001 → create MiscTypeMaster → SecondaryId = MiscTypeMasterId (temp)
    //   TC002 → create MiscMaster     → SecondaryId = MiscMasterId = UnitTypeId
    //   TC003 → create Unit (full nested payload) → CreatedId = UnitId
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Setup_CreateMiscTypeMaster_CapturesMiscTypeId()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = $"U{_f.EntityCode[..9]}",
            description  = $"QA Unit Test MiscType {_f.EntityCode}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        _f.SecondaryId = doc.RootElement.CreatedId();
        _f.SecondaryId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Setup_CreateMiscMaster_CapturesUnitTypeId()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscMastRoute, new
        {
            miscTypeId  = _f.SecondaryId,
            code        = $"U{_f.EntityCode[..9]}",
            description = "QA Unit Type"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        _f.SecondaryId = doc.RootElement.CreatedId();
        _f.SecondaryId.Should().BeGreaterThan(0);   // SecondaryId now = UnitTypeId
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        // Create returns int directly: { "data": 42 }
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE VALIDATION
    // Many required fields; MobileNumber (10 digits), PinCode (6 digits), Email
    // UnitTypeId FK-checked against MiscMaster (FKColumnDelete)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_UnitNameEmpty_Returns400()
    {
        var payload = BuildValidCreatePayload(unitName: "");
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_ShortNameEmpty_Returns400()
    {
        var payload = BuildValidCreatePayload(shortName: "");
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_UnitTypeIdZero_Returns400()
    {
        // UnitTypeId=0 fails NotEmpty (GreaterThan(0)) and skips FKColumnDelete (.When)
        var payload = BuildValidCreatePayload(unitTypeId: 0);
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("UnitTypeId");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_InvalidUnitTypeId_Returns400()
    {
        // Valid int but non-existent → FKColumnDelete fires → 400
        var payload = BuildValidCreatePayload(unitTypeId: 999999);
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("inactive or deleted");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_InvalidPhone_Returns400()
    {
        // MobileNumber: must be exactly 10 digits
        var payload = BuildValidCreatePayload(contactNumber: "12345");   // only 5 digits
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("ContactNumber");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_InvalidEmail_Returns400()
    {
        var payload = BuildValidCreatePayload(email: "not-an-email");
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Email");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_InvalidPinCode_Returns400()
    {
        // PinCode: must be 6 digits (regex ^[0-9]{6}$)
        var payload = BuildValidCreatePayload(pinCode: 1234);   // 4 digits → fails
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("PinCode");
    }

    [Fact, TestPriority(12)]
    public async Task TC012_Create_AddressLine1Empty_Returns400()
    {
        var payload = BuildValidCreatePayload(addressLine1: "");
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(13)]
    public async Task TC013_Create_UnitNameExceedsMaxLength_Returns400()
    {
        // UnitName max = 50 characters
        var payload = BuildValidCreatePayload(unitName: new string('A', 51));
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Create_ShortNameExceedsMaxLength_Returns400()
    {
        // ShortName max = 10 characters (very short!)
        var payload = BuildValidCreatePayload(shortName: "ELEVENCHAR!");   // 11 chars
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // Returns 404 when empty; otherwise 200 with pagination
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(16)]
    public async Task TC016_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        // May return 200 or 404 depending on whether units exist pre-test
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if ((int)resp.StatusCode == 200)
        {
            var doc = await ParseAsync(resp);
            doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact, TestPriority(17)]
    public async Task TC017_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_GetAll_SearchByTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_GetAll_NoMatchSearch_Returns404()
    {
        // GetAll returns 404 when no records match (like Currency)
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // id <= 0 → 400 (controller check); non-existent → 200+null (no null check)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId()
            .Should().Be(_f.CreatedId);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetById_IdZero_Returns400()
    {
        // Controller explicitly checks id <= 0 → 400
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetById_NonExistentId_Returns200_WithNullData()
    {
        // No null check after handler → 200 with data:null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (by-name?unitname=...&CompanyId=...)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(24)]
    public async Task TC024_AutoComplete_WithBothParams_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?unitname=QA&CompanyId={QACompanyId}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_AutoComplete_NoParams_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(26)]
    public async Task TC026_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?unitname=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — BY USERID  (extra endpoint)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(27)]
    public async Task TC027_GetUnitByUserId_ValidUserId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-userid?CompanyId={QACompanyId}&UserId=1");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_GetUnitByUserId_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-userid?CompanyId=1&UserId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE  (PUT api/Unit/update — non-standard suffix)
    // Payload nested under updateUnitDto; no pre-query check → always 200
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(29)]
    public async Task TC029_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, BuildValidUpdatePayload(_f.CreatedId));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString()
            .Should().Contain("Updated");
    }

    [Fact, TestPriority(30)]
    public async Task TC030_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(UpdateRoute, BuildValidUpdatePayload(_f.CreatedId));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Update_UnitNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, BuildValidUpdatePayload(_f.CreatedId, unitName: ""));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("required");
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Update_ShortNameExceedsMaxLength_Returns400()
    {
        // ShortName max = 10 in validator
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute,
            BuildValidUpdatePayload(_f.CreatedId, shortName: "ELEVENCHAR!"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("longer than");
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Update_InvalidPhone_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute,
            BuildValidUpdatePayload(_f.CreatedId, contactNumber: "12345"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("ContactNumber");
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Update_InvalidEmail_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute,
            BuildValidUpdatePayload(_f.CreatedId, email: "not-an-email"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Email");
    }

    [Fact, TestPriority(35)]
    public async Task TC035_Update_InvalidPinCode_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute,
            BuildValidUpdatePayload(_f.CreatedId, pinCode: 1234));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("PinCode");
    }

    [Fact, TestPriority(36)]
    public async Task TC036_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (ALWAYS LAST — uses /{id} param; field name = UnitId)
    // No pre-query → always 200 after validator passes
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(37)]
    public async Task TC037_Delete_UnitIdZero_Returns400()
    {
        // NotEmpty validator fires for UnitId=0
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
    public async Task TC039_Delete_NonExistentUnitId_Returns200()
    {
        // No pre-query check → validator passes (SoftDelete returns false for missing) → 200
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
            .Should().Contain("Deleted");
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Delete_AlreadyDeleted_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_VerifyDelete_GetByIdReturns200_WithNullData()
    {
        // No null check → soft-deleted unit returns 200+null
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 8 — EXTRA COVERAGE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(43)]
    public async Task TC043_GetAll_LargePageSize_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=100");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_AutoComplete_CompanyIdZero_Returns200_EmptyOrFiltered()
    {
        // CompanyId=0 → handler uses 0 → may return empty list; still 200
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?unitname=QA&CompanyId=0");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(45)]
    public async Task TC045_Create_ShortNameAtMaxLength_Returns200()
    {
        // Exactly 10 chars (max boundary for ShortName) — should pass
        var payload = BuildValidCreatePayload(shortName: "TENCHARSTR");   // exactly 10
        var resp    = await _f.Client.PostAsJsonAsync(BaseRoute, payload);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ParseAsync(resp);
        doc.RootElement.CreatedId().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Update_NonExistentUnitId_Returns200()
    {
        // No pre-query in Update → validator passes → handler updates 0 rows → 200
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, BuildValidUpdatePayload(999999));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Payload builders — Unit has deeply nested DTOs
    // ─────────────────────────────────────────────────────────────────────────

    private object BuildValidCreatePayload(
        string? unitName      = null,
        string? shortName     = null,
        int     unitTypeId    = -1,
        string? contactNumber = "9876543210",
        string? email         = "qa@test.com",
        int     pinCode       = 123456,
        string? addressLine1  = "QA Test Address Line 1") => new
    {
        unitName          = unitName      ?? $"QA Unit {_f.EntityCode}{Guid.NewGuid().ToString("N")[..6]}",
        shortName         = shortName     ?? _f.EntityCode[..6],
        companyId         = QACompanyId,
        divisionId        = QADivisionId,
        unitHeadName      = "QA Head",
        cinno             = "CINTEST001",
        oldUnitId         = "OLD001",
        isMaintenanceStopStart = false,
        spindlesCapacity  = 100,
        unitTypeId        = unitTypeId < 0 ? _f.SecondaryId : unitTypeId,
        unitAddressDto    = new
        {
            countryId      = QACountryId,
            stateId        = QAStateId,
            cityId         = QACityId,
            addressLine1   = addressLine1 ?? "QA Test Address Line 1",
            addressLine2   = "QA Test Line 2",
            pinCode        = pinCode,
            contactNumber  = contactNumber,
            alternateNumber = ""
        },
        unitContactsDto = new
        {
            name        = "QA Contact Person",
            designation = "QA Manager",
            email       = email,
            phoneNo     = "9876543210",
            remarks     = ""
        }
    };

    private object BuildValidUpdatePayload(
        int     unitId        = 0,
        string? unitName      = null,
        string? shortName     = null,
        string? contactNumber = "9876543210",
        string? email         = "qa@test.com",
        int     pinCode       = 123456) => new
    {
        updateUnitDto = new
        {
            id                = unitId,   // UpdateUnitsDto property is Id (the handler reads UpdateUnitDto.Id)
            unitName          = unitName   ?? $"QA Unit Updated {_f.EntityCode}",
            shortName         = shortName  ?? _f.EntityCode[..6],
            companyId         = QACompanyId,
            divisionId        = QADivisionId,
            unitHeadName      = "QA Head Updated",
            cinno             = "CINTEST002",
            oldUnitId         = "OLD002",
            isMaintenanceStopStart = false,
            spindlesCapacity  = 200,
            isActive          = (byte)1,
            unitTypeId        = _f.SecondaryId,
            unitAddressDto    = new
            {
                countryId      = QACountryId,
                stateId        = QAStateId,
                cityId         = QACityId,
                addressLine1   = "QA Updated Address Line 1",
                addressLine2   = "QA Updated Line 2",
                pinCode        = pinCode,
                contactNumber  = contactNumber,
                alternateNumber = ""
            },
            unitContactsDto = new
            {
                name        = "QA Updated Contact",
                designation = "QA Lead",
                email       = email,
                phoneNo     = "9876543210",
                remarks     = ""
            }
        }
    };

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
