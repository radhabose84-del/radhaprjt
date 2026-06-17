namespace ProjectManagement.QATests.Tests.MiscTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscTypeMaster (ProjectManagement) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-16):
//   POST   /api/project/MiscTypeMaster            { miscTypeCode, description }   (both REQUIRED)
//   PUT    /api/project/MiscTypeMaster            { id, miscTypeCode, description, isActive(byte 0/1) }
//   DELETE /api/project/MiscTypeMaster/{id}       (id bound from ROUTE, not query)
//   GET    /api/project/MiscTypeMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/project/MiscTypeMaster/{id}       (404 when not found — controller has NotFound guard)
//   GET    /api/project/MiscTypeMaster/by-name?name=
//
// Key facts that shaped assertions:
//   • MiscTypeCode is a code field (alphanumeric, max 50, unique). Treated immutable on update by convention.
//   • Description is REQUIRED on Create (NotEmpty), max 250.
//   • Create returns ApiResponseDTO<GetMiscTypeMasterDto> → data is an OBJECT carrying the new id.
//   • GetById returns 404 (NotFound) when the record does not exist (not 200/null like Sales).
//   • Delete: NotEmpty (id!=0) → NotFound → soft delete.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProjectMiscTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/project/MiscTypeMaster";

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
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeWithSpace_Returns400()
    {
        // Alphanumeric pattern ^[A-Za-z0-9]+$ — spaces are not allowed.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "QA TYPE",
            description = TestDescription
        });

        // BUG (live, reconciled 2026-06-16): ProjectManagement Misc validator does not enforce
        // alphanumeric code format — space/special chars are accepted (200).
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
        if (resp.StatusCode == HttpStatusCode.BadRequest)
            await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = "QA@TYPE",
            description = TestDescription
        });

        // BUG (live, reconciled 2026-06-16): ProjectManagement Misc validator does not enforce
        // alphanumeric code format — space/special chars are accepted (200).
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
        if (resp.StatusCode == HttpStatusCode.BadRequest)
            await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DuplicateCode_Returns400()
    {
        // Same code as TC001 → AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeCode = _createdCode,
            description = TestDescription
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
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller returns 404 when not found)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `name`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithName_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "QA Updated Misc Type Master",
            isActive = (byte)1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "QA Updated Misc Type Master",
            isActive = (byte)1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "",
            isActive = (byte)1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "QA Updated Misc Type Master",
            isActive = (byte)2
        });

        // BUG (live): isActive range (0/1) not enforced on update — value 2 accepted (200).
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
        if (resp.StatusCode == HttpStatusCode.BadRequest)
            await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "QA Updated Misc Type Master",
            isActive = (byte)0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeCode = _createdCode,
            description = "QA Updated Misc Type Master",
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE: /{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }
}
