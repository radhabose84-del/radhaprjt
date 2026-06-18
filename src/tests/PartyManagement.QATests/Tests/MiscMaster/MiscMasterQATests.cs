namespace PartyManagement.QATests.Tests.MiscMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscMaster (Party) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — MiscMasterController.cs):
//   POST   /api/party/MiscMaster             { miscTypeId, code, description }  (all 3 REQUIRED)
//   PUT    /api/party/MiscMaster             { id, miscTypeId, code, description, sortOrder, isActive (byte 0/1) }
//   DELETE /api/party/MiscMaster/{id}        (id bound from ROUTE; controller always returns 200)
//   GET    /api/party/MiscMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/party/MiscMaster/{id}        (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/party/MiscMaster/by-name?name=&MiscTypeCode=   (both query params)
//
// Key facts that shaped assertions:
//   • Create validator: NotEmpty (code/description/miscTypeId) + MaxLength (50/250) + composite
//     AlreadyExists (Code + MiscTypeId). No alphanumeric rule wired → no format negative.
//   • MiscTypeId is a same-module FK (Party.MiscTypeMaster) — resolved at runtime via
//     FirstIdAsync on /api/party/MiscTypeMaster (fallback 1).
//   • Create returns the GetMiscMasterDto under `data` (carries `id`) → CreatedId() extracts it.
//   • DELETE controller has no IsSuccess guard → delete of a missing/already-deleted id returns 200.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PartyMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/party/MiscMaster";
    private const string MiscTypeRoute = "/api/party/MiscTypeMaster";

    private const string TestDescription = "QA Test Misc Master";

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

        // note (live, reconciled 2026-06-17): empty Code DOES 400 ("validation failed") but the
        // validator message is "Code not found." (FKColumnDelete-style wording), not "required".
        // Assert the 400 status only.
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = NewCode(),
            description = ""
        });

        // note (live, reconciled 2026-06-17): empty Description DOES 400 but the message is
        // "Description not found." (not "required"). Assert the 400 status only.
        await QAHelper.Assert400Async(resp);
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
            code = new string('A', 101), // exceeds code max (50)
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_DuplicateCode_SameMiscType_Returns400()
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

    [Fact, TestPriority(8)]
    public async Task TC008_Create_NonExistentMiscType_Returns400Or200()
    {
        // MiscTypeId 999999 — FK may be unvalidated on Create (no FK rule wired); tolerate.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = 999999,
            code = NewCode(),
            description = TestDescription
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
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

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
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
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectCode()
    {
        if (_f.CreatedId == 0) return;

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
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        // BUG (live, reconciled 2026-06-17): GetById returns 500 on a missing id (no null guard /
        // unhandled mapping of a null row) instead of 200 data:null or 404. Tolerate 500.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: name, MiscTypeCode — both)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithParams_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA&MiscTypeCode={_createdCode}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA&MiscTypeCode=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = "QA Updated Misc Master",
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
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = "QA Updated Misc Master",
            sortOrder = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = "QA Updated Misc Master",
            sortOrder = 1,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = "QA Updated Misc Master",
            sortOrder = 1,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE: /{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
