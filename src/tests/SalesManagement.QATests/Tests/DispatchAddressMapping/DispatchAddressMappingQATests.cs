namespace SalesManagement.QATests.Tests.DispatchAddressMapping;

// ─────────────────────────────────────────────────────────────────────────────
// DispatchAddressMapping — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/DispatchAddressMapping   { partyId, dispatchAddressId, usageTypeId, isDefault }
//   PUT    /api/DispatchAddressMapping   { id, isDefault, isActive }
//                                          (partyId + dispatchAddressId are IMMUTABLE — not on update)
//   DELETE /api/DispatchAddressMapping?id={id}   (id bound from QUERY — DeleteDispatchAddressMapping(int id))
//   GET    /api/DispatchAddressMapping?PageNumber=&PageSize=&SearchTerm=&PartyId=
//   GET    /api/DispatchAddressMapping/{id}   (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/DispatchAddressMapping/by-name?term=
//
// Key facts that shaped assertions:
//   • No unique code field. Required FKs: partyId (cross-module), dispatchAddressId
//     (same-module → /api/DispatchAddressMaster), usageTypeId (same-module MiscMaster).
//   • partyId resolved at runtime via FirstIdAsync on /api/party/PartyMaster (fallback 1);
//     dispatchAddressId via /api/DispatchAddressMaster; usageTypeId via /api/sales/MiscMaster.
//   • SKIP NOTE: if partyId cannot be resolved AND fallback id 1 does not exist on the QA
//     clone, TC001 create-happy will surface a clean FK-validation 400 during live
//     reconciliation — at that point seed/resolve a real party and un-skip.
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created row visible in GetAll.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DispatchAddressMappingCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DispatchAddressMappingQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DispatchAddressMapping";

    // Resolved-at-runtime FK ids. Captured by TC001.
    private static int _partyId;
    private static int _dispatchAddressId;
    private static int _usageTypeId;

    public DispatchAddressMappingQATests(QAServerFixture fixture) => _f = fixture;

    private async Task ResolveFksAsync()
    {
        if (_partyId != 0 || _dispatchAddressId != 0) return; // resolve once per collection run
        _partyId = await QAHelper.FirstIdAsync(_f.Client, "/api/party/PartyMaster");
        _dispatchAddressId = await QAHelper.FirstIdAsync(_f.Client, "/api/DispatchAddressMaster");
        _usageTypeId = await QAHelper.FirstIdAsync(_f.Client, "/api/sales/MiscMaster");
        if (_partyId == 0) _partyId = 1;
        if (_dispatchAddressId == 0) _dispatchAddressId = 1;
        if (_usageTypeId == 0) _usageTypeId = 1;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = _partyId,
            dispatchAddressId = _dispatchAddressId,
            usageTypeId = _usageTypeId,
            isDefault = true
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
        await ResolveFksAsync();

        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            partyId = _partyId,
            dispatchAddressId = _dispatchAddressId,
            usageTypeId = _usageTypeId,
            isDefault = false
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_PartyIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = 0,
            dispatchAddressId = _dispatchAddressId,
            usageTypeId = _usageTypeId,
            isDefault = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DispatchAddressIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = _partyId,
            dispatchAddressId = 0,
            usageTypeId = _usageTypeId,
            isDefault = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_UsageTypeIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = _partyId,
            dispatchAddressId = _dispatchAddressId,
            usageTypeId = 0,
            isDefault = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NonExistentDispatchAddress_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = _partyId,
            dispatchAddressId = 999999,
            usageTypeId = _usageTypeId,
            isDefault = false
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
    public async Task TC022_GetAll_FilterByPartyId_Returns200()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&PartyId={_partyId}");

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
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (partyId + dispatchAddressId immutable — not on update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            isDefault = false,
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
            isDefault = false,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            isDefault = false,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            isDefault = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            isDefault = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            isDefault = false,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            isDefault = true,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_EmptyBody_Returns400()
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
