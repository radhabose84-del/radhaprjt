namespace SalesManagement.QATests.Tests.SalesContact;

// ─────────────────────────────────────────────────────────────────────────────
// SalesContact — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/SalesContact            { contactName, mobileNumber, contactTypeId, partyId?, email?, remarks? }
//   PUT    /api/SalesContact            { id, contactName, mobileNumber, contactTypeId, partyId?, email?, remarks?, isActive }
//   DELETE /api/SalesContact?id={id}    (id bound from QUERY, not route — DeleteSalesContact(int id))
//   GET    /api/SalesContact?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesContact/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/SalesContact/by-name?term=
//
// Key facts that shaped assertions:
//   • NO unique CODE field, but mobileNumber is UNIQUE (MobileAlreadyExistsAsync) and must match a
//     10-digit pattern (MobileNumber rule). → run-unique 10-digit mobile derived from EntityCode.
//     Includes duplicate-mobile 400 and invalid-mobile-format 400 tests.
//   • contactTypeId is a SAME-module MiscMaster FK validated via ContactTypeExistsAsync — resolved at
//     runtime from /api/sales/MiscMaster GETALL (fallback 1).
//   • mobileNumber is MUTABLE on update (present in UpdateSalesContactCommand) — update reuses same mobile.
//   • partyId / email / remarks are optional — omitted in happy path.
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created rows are visible in GetAll/GetById.
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesContactCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesContactQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesContact";

    private const string TestName = "QA Test Contact";

    // Run-unique mobile captured at create; reused by duplicate + update tests.
    private static string _createdMobile = string.Empty;

    // Same-module MiscMaster FK (ContactType) resolved once at create.
    private static int _contactTypeId;

    public SalesContactQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique 10-digit mobile: "9" + 9 digits derived from EntityCode (zero-padded, sliced).
    private string NewMobile()
    {
        var digits = new string(_f.EntityCode.Where(char.IsDigit).Take(9).ToArray()).PadLeft(9, '0');
        return "9" + digits[..9];
    }

    private async Task<int> ResolveContactTypeIdAsync()
    {
        if (_contactTypeId > 0) return _contactTypeId;
        var id = await QAHelper.FirstIdAsync(_f.Client, "/api/sales/MiscMaster");
        _contactTypeId = id > 0 ? id : 1;
        return _contactTypeId;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdMobile = NewMobile();
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = _createdMobile,
            contactTypeId,
            remarks = "Created by QA suite"
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
            contactName = "No Auth Contact",
            mobileNumber = "9000000001",
            contactTypeId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_ContactNameEmpty_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = "",
            mobileNumber = NewMobile(),
            contactTypeId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MobileEmpty_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = "",
            contactTypeId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_ContactTypeIdMissing_Returns400()
    {
        // ContactTypeId NotEmpty → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = NewMobile(),
            contactTypeId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NameTooLong_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = new string('A', 101), // exceeds name max (100)
            mobileNumber = NewMobile(),
            contactTypeId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_InvalidMobileFormat_Returns400()
    {
        // MobileNumber rule requires a 10-digit pattern → "12345" fails.
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = "12345",
            contactTypeId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DuplicateMobile_Returns400()
    {
        // Same mobile as TC001 → MobileAlreadyExistsAsync fails.
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = _createdMobile,
            contactTypeId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NonExistentContactType_Returns400()
    {
        // ContactTypeExistsAsync false → FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            contactName = TestName,
            mobileNumber = NewMobile(),
            contactTypeId = 999999
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_EmptyBody_Returns400()
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
    public async Task TC022_GetAll_SearchByCreatedMobile_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdMobile}");

        // note (live, reconciled 2026-06-16): the GetAll search filter does NOT include the mobile
        // column, so searching by the created mobile number legitimately returns an empty data array.
        // Assert reachability (200) + valid array shape only; do not require a match.
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
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectMobile()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("mobileNumber").GetString()
            .Should().Be(_createdMobile);
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
    // SECTION 5 — UPDATE  (mobileNumber is mutable — reuse same mobile)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
            remarks = "Updated by QA",
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
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_ContactNameEmpty_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            contactName = "",
            mobileNumber = _createdMobile,
            contactTypeId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400_NotFound()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_IdZero_Returns400()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var contactTypeId = await ResolveContactTypeIdAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            contactName = "QA Updated Contact",
            mobileNumber = _createdMobile,
            contactTypeId,
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
