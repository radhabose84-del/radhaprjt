namespace SalesManagement.QATests.Tests.SalesOffice;

// ─────────────────────────────────────────────────────────────────────────────
// SalesOffice — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/SalesOffice            { salesOfficeName, salesOrganisationId, cityId, pincode?, phone?, email?, responsibleManager?, regionTerritory?, address? }
//   PUT    /api/SalesOffice            { id, salesOfficeName, salesOrganisationId, cityId, pincode?, phone?, email?, responsibleManager?, regionTerritory?, address?, isActive }
//   DELETE /api/SalesOffice?id={id}    (id bound from QUERY, not route — DeleteSalesOffice(int id))
//   GET    /api/SalesOffice?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesOffice/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/SalesOffice/by-name?term=
//
// Key facts that shaped assertions:
//   • NO unique code field. Uniqueness is on (SalesOfficeName + SalesOrganisationId) composite (AlreadyExistsAsync).
//     → run-unique names avoid "already exists" 400s; NO duplicate-code test.
//   • salesOfficeName has an Alphanumeric rule (rule.Pattern) — name uses plain alphanumeric tokens (no specials).
//   • salesOrganisationId is a SAME-module FK validated via SalesOrganisationExistsAsync — resolved at runtime
//     from /api/SalesOrganisation GETALL (fallback 1).
//   • cityId is a cross-module FK validated via CityExistsAsync — uses _f.CityId (fallback 1).
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created rows are visible in GetAll/GetById.
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesOfficeCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesOfficeQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesOffice";

    private const string TestName = "QATestSalesOffice";

    // Run-unique name captured at create; reused by GetAll-search.
    private static string _createdName = string.Empty;

    // Same-module FK (SalesOrganisation) resolved once at create.
    private static int _salesOrganisationId;

    public SalesOfficeQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric (no spaces/specials) to satisfy the Alphanumeric rule.
    private string NewName() => $"{TestName}{_f.EntityCode[1..7]}";

    private int CityId() => _f.CityId > 0 ? _f.CityId : 1;

    private async Task<int> ResolveSalesOrganisationIdAsync()
    {
        if (_salesOrganisationId > 0) return _salesOrganisationId;
        var id = await QAHelper.FirstIdAsync(_f.Client, "/api/SalesOrganisation");
        _salesOrganisationId = id > 0 ? id : 1;
        return _salesOrganisationId;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdName = NewName();
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            responsibleManager = "QA Manager",
            regionTerritory = "QA Region",
            address = "QA Address"
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = "NoAuthOffice",
            salesOrganisationId = 1,
            cityId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NameEmpty_Returns400()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = "",
            salesOrganisationId,
            cityId = CityId()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_SalesOrganisationIdMissing_Returns400()
    {
        // SalesOrganisationId GreaterThan(0) → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = NewName(),
            salesOrganisationId = 0,
            cityId = CityId()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CityIdMissing_Returns400()
    {
        // CityId GreaterThan(0) → default 0 fails validation.
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = NewName(),
            salesOrganisationId,
            cityId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NameTooLong_Returns400()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = new string('A', 101), // exceeds name max (100)
            salesOrganisationId,
            cityId = CityId()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NonExistentSalesOrganisation_Returns400()
    {
        // SalesOrganisationExistsAsync false → FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = NewName(),
            salesOrganisationId = 999999,
            cityId = CityId()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_NonExistentCity_Returns400()
    {
        // CityExistsAsync false → FK validation fails.
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOfficeName = NewName(),
            salesOrganisationId,
            cityId = 999999
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (reads are NOT company-scoped; TC001 row is visible)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedName_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={Uri.EscapeDataString(_createdName)}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectName()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("salesOfficeName").GetString()
            .Should().Be(_createdName);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200_WithNullData()
    {
        // No null guard in controller → 200 with data:null.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            responsibleManager = "QA Updated Manager",
            address = "QA Updated Address",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = _createdName,
            salesOrganisationId = 1,
            cityId = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Returns400()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = "",
            salesOrganisationId,
            cityId = CityId(),
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400_NotFound()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_IdZero_Returns400()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var salesOrganisationId = await ResolveSalesOrganisationIdAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            salesOfficeName = _createdName,
            salesOrganisationId,
            cityId = CityId(),
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        // After soft delete, GetByIdAsync filters IsDeleted=0 → null → 200 + data:null.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
