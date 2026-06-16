namespace SalesManagement.QATests.Tests.DispatchAddressMaster;

// ─────────────────────────────────────────────────────────────────────────────
// DispatchAddressMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/DispatchAddressMaster   { dispatchAddressName, addressLine1?, addressLine2?,
//                                          cityId, stateId, countryId, pinCode?, contactPerson?,
//                                          mobileNumber?, email?, gstin?, latitude?, longitude?,
//                                          freightId }
//   PUT    /api/DispatchAddressMaster   { id, dispatchAddressName, addressLine1?, addressLine2?,
//                                          cityId, stateId, countryId, pinCode?, …, freightId, isActive }
//   DELETE /api/DispatchAddressMaster?id={id}   (id bound from QUERY — DeleteDispatchAddressMaster(int id))
//   GET    /api/DispatchAddressMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/DispatchAddressMaster/{id}   (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/DispatchAddressMaster/by-name?term=
//
// Key facts that shaped assertions:
//   • No unique code field; dispatchAddressName is the required name.
//   • Required FKs: cityId (→ _f.CityId), stateId (cross-module /api/State), countryId
//     (cross-module /api/Country), freightId (cross-module /api/logistics/FreightMaster).
//     All resolved at runtime via FirstIdAsync (the QA clone has no guaranteed seed ids).
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created row visible in GetAll.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DispatchAddressMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DispatchAddressMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DispatchAddressMaster";

    // Run-unique name (derived from the per-run EntityCode) so repeated runs never collide
    // on the DispatchAddressName uniqueness check against accumulated clone data.
    private string TestName => $"QA Dispatch {_f.EntityCode[..8]}";

    // Resolved-at-runtime FK ids. Captured by TC001.
    private static int _cityId;
    private static int _stateId;
    private static int _countryId;
    private static int _freightId;

    public DispatchAddressMasterQATests(QAServerFixture fixture) => _f = fixture;

    private async Task ResolveFksAsync()
    {
        if (_cityId != 0 || _stateId != 0) return; // resolve once per collection run
        _cityId = _f.CityId != 0 ? _f.CityId : await QAHelper.FirstIdAsync(_f.Client, "/api/City");
        _stateId = await QAHelper.FirstIdAsync(_f.Client, "/api/State");
        _countryId = await QAHelper.FirstIdAsync(_f.Client, "/api/Country");
        _freightId = await QAHelper.FirstIdAsync(_f.Client, "/api/logistics/FreightMaster");
        if (_cityId == 0) _cityId = 1;
        if (_stateId == 0) _stateId = 1;
        if (_countryId == 0) _countryId = 1;
        if (_freightId == 0) _freightId = 1;
    }

    private object ValidCreateBody(string? name = null) => new
    {
        dispatchAddressName = name ?? TestName,
        addressLine1 = "QA Line 1",
        addressLine2 = "QA Line 2",
        cityId = _cityId,
        stateId = _stateId,
        countryId = _countryId,
        pinCode = "641001",
        contactPerson = "QA Tester",
        mobileNumber = "9876543210",
        email = "qa@bsoft.test",
        gstin = "22AAAAA1234A1Z5",
        latitude = 11.0m,
        longitude = 77.0m,
        freightId = _freightId
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidCreateBody());

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        await ResolveFksAsync();

        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, ValidCreateBody("No Auth Address"));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = "",
            cityId = _cityId,
            stateId = _stateId,
            countryId = _countryId,
            freightId = _freightId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CityIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = TestName,
            cityId = 0,
            stateId = _stateId,
            countryId = _countryId,
            freightId = _freightId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_StateIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = TestName,
            cityId = _cityId,
            stateId = 0,
            countryId = _countryId,
            freightId = _freightId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CountryIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = TestName,
            cityId = _cityId,
            stateId = _stateId,
            countryId = 0,
            freightId = _freightId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_FreightIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = TestName,
            cityId = _cityId,
            stateId = _stateId,
            countryId = _countryId,
            freightId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NonExistentCountry_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchAddressName = TestName,
            cityId = _cityId,
            stateId = _stateId,
            countryId = 999999,
            freightId = _freightId
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
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
    public async Task TC022_GetAll_SearchByName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={TestName}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
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
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("id").GetInt32()
            .Should().Be(_f.CreatedId);
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

    private object ValidUpdateBody(string name = "QA Updated Address", int isActive = 1) => new
    {
        id = _f.CreatedId,
        dispatchAddressName = name,
        addressLine1 = "QA Updated Line 1",
        cityId = _cityId,
        stateId = _stateId,
        countryId = _countryId,
        pinCode = "641002",
        freightId = _freightId,
        isActive
    };

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, ValidUpdateBody());
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        await ResolveFksAsync();
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, ValidUpdateBody());
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Returns400()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, ValidUpdateBody(name: ""));
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, ValidUpdateBody(isActive: 2));
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400_NotFound()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            dispatchAddressName = "QA Updated Address",
            cityId = _cityId,
            stateId = _stateId,
            countryId = _countryId,
            freightId = _freightId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_IdZero_Returns400()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            dispatchAddressName = "QA Updated Address",
            cityId = _cityId,
            stateId = _stateId,
            countryId = _countryId,
            freightId = _freightId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        await ResolveFksAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, ValidUpdateBody(isActive: 0));
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, ValidUpdateBody(isActive: 1));
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
