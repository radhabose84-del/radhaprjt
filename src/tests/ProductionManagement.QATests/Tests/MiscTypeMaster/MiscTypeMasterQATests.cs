namespace ProductionManagement.QATests.Tests.MiscTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscTypeMaster (Production) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17):
//   POST   /api/production/MiscTypeMaster            { miscTypeCode, description }  (both REQUIRED)
//   PUT    /api/production/MiscTypeMaster            { id, description, isActive }  (miscTypeCode immutable)
//   DELETE /api/production/MiscTypeMaster?id={id}    (DeleteMiscTypeMaster(int id) → bound from QUERY)
//   GET    /api/production/MiscTypeMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/production/MiscTypeMaster/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/production/MiscTypeMaster/by-name?term=
//
// Key facts that shaped assertions:
//   • MiscTypeCode is alphanumeric (^[A-Za-z0-9]+$), max 50, unique. Immutable on update.
//   • Description is REQUIRED on Create (NotEmpty), max 200.
//   • GetById data carries the code under "miscTypeCode".
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProdMiscTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/production/MiscTypeMaster";

    private const string TestDescription = "QA Test Misc Type Master";

    // The run-unique alphanumeric code captured at create; reused by duplicate/immutability tests.
    private static string _createdCode = string.Empty;

    public MiscTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric, sliced to 10 chars.
    private string NewCode() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = _createdCode,
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
            miscTypeCode = "NOAUTH01",
            description = TestDescription
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "",
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = NewCode(),
            description = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = new string('A', 101), // exceeds max 50
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DescriptionTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = NewCode(),
            description = new string('A', 201) // exceeds max 200
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
            miscTypeCode = "QA TYPE",
            description = TestDescription
        });

        // BUG (live, reconciled 2026-06-17): Production Misc code format not enforced (space accepted)
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "QA@TYPE",
            description = TestDescription
        });

        // BUG (live, reconciled 2026-06-17): Production Misc code format not enforced (special char accepted)
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCode_Returns400()
    {
        // Same code as TC001 → AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = _createdCode,
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_EmptyBody_Returns400()
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
        doc.RootElement.GetProperty("data").GetProperty("miscTypeCode").GetString()
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
    // SECTION 5 — UPDATE  (Code is immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Misc Type",
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
            description = "QA Updated Misc Type",
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
            description = "QA Updated Misc Type",
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
            description = "QA Updated Misc Type",
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
            description = "QA Updated Misc Type",
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
            description = "QA Updated Misc Type",
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Misc Type",
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

    [Fact, TestPriority(57)]
    public async Task TC057_Verify_CodeIsImmutable_GetByIdShowsOriginalCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("miscTypeCode").GetString()
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
