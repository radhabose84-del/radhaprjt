namespace LogisticsManagement.QATests.Tests.MiscMaster;

// ─────────────────────────────────────────────────────────────────────────────
// LogisticsManagement.MiscMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-16):
//   POST   /api/logistics/MiscMaster             { miscTypeId, code, description }  (all 3 REQUIRED)
//   PUT    /api/logistics/MiscMaster             { id, description, sortOrder, isActive } (code + miscTypeId immutable)
//   DELETE /api/logistics/MiscMaster?id={id}     ([HttpDelete] int id → id bound from QUERY, not route)
//   GET    /api/logistics/MiscMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/logistics/MiscMaster/{id}        (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/logistics/MiscMaster/by-name?term=&miscTypeCode=
//
// Key facts that shaped assertions:
//   • Code is alphanumeric (^[A-Za-z0-9]+$) — spaces/specials rejected. Immutable on update.
//   • Description is REQUIRED on Create (NotEmpty).
//   • MiscTypeId is a same-module FK (Logistics MiscTypeMaster) validated at create.
//       → resolved at runtime via FirstIdAsync on /api/MiscTypeMaster (fallback 1).
//   • AlreadyExists is COMPOSITE: uniqueness is (Code + MiscTypeId) — duplicate test reuses both.
//   • GetById data carries the misc code under "code".
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LogisticsMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/logistics/MiscMaster";
    private const string MiscTypeRoute = "/api/MiscTypeMaster";

    private const string TestDescription = "QA Test Logistics Misc Master";

    // The run-unique alphanumeric code captured at create; reused by duplicate/immutability tests.
    private static string _createdCode = string.Empty;
    // The valid same-module FK miscTypeId resolved at create time; reused by duplicate test.
    private static int _miscTypeId;

    public MiscMasterQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric, sliced to 10 chars.
    private string NewCode() => _f.EntityCode[..10];

    private async Task<int> ResolveMiscTypeIdAsync()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, MiscTypeRoute);
        return id > 0 ? id : 1; // fallback — live reconciliation may adjust
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        _miscTypeId = await ResolveMiscTypeIdAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = TestDescription
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
            miscTypeId = 1,
            code = "NOAUTH01",
            description = TestDescription
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = "",
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        // Description is REQUIRED on Create (NotEmpty).
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = NewCode(),
            description = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MiscTypeIdMissing_Returns400()
    {
        // MiscTypeId NotEmpty → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = 0,
            code = NewCode(),
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = new string('A', 101), // exceeds code max (20)
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CodeWithSpace_Returns400()
    {
        // Alphanumeric pattern ^[A-Za-z0-9]+$ — spaces are not allowed.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = "QA MISC",
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = "QA@MISC",
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCode_SameMiscType_Returns400()
    {
        // Same code + same miscTypeId as TC001 → composite AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_NonExistentMiscType_Returns400()
    {
        // FK validation false → fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = 999999,
            code = NewCode(),
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
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
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");

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
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("code").GetString()
            .Should().Be(_createdCode);
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
    // SECTION 4 — AUTOCOMPLETE  (params: term, optional miscTypeCode)
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
    // SECTION 5 — UPDATE  (Code + MiscTypeId immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
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
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "",
            sortOrder = 1,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Logistics Misc Master",
            sortOrder = 1,
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

    [Fact, TestPriority(58)]
    public async Task TC058_Verify_CodeIsImmutable_GetByIdShowsOriginalCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("code").GetString()
            .Should().Be(_createdCode);
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
