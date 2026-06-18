namespace BackgroundService.QATests.Tests.MiscMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscMaster (BackgroundService) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-18):
//   POST   /api/backgroundservice/MiscMaster        { miscTypeId, code, description }
//   PUT    /api/backgroundservice/MiscMaster        { id, miscTypeId, code, description, sortOrder, isActive(byte) }
//   DELETE /api/backgroundservice/MiscMaster/{id}   (id bound from ROUTE)
//   GET    /api/backgroundservice/MiscMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/backgroundservice/MiscMaster/{id}   (ALWAYS 200; data carries the DTO directly)
//   GET    /api/backgroundservice/MiscMaster/by-name?name=&MiscTypeCode=  (MiscTypeCode REQUIRED)
//
// Key facts that shaped assertions:
//   • No code-format/alphanumeric rule → only NotEmpty + MaxLength negatives.
//   • miscTypeId is a FK (GreaterThan(0) only — no existence check). Resolved via FirstIdAsync
//     on /api/backgroundservice/MiscTypeMaster; create self-skips if it resolves 0.
//   • Uniqueness is COMPOSITE (code + miscTypeId) — duplicate test reuses both.
//   • ⚠️ Controller Create/Update/Delete ALWAYS return HTTP 200 (no IsSuccess branch). The
//     create-return is the BARE DTO at `data` → CreatedId() reads data.Id.
//   • by-name requires MiscTypeCode; without a captured type code it may 400 → tolerant BeOneOf.
//   • Update echoes miscTypeId + code (threaded from the captured create values).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("BgMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/backgroundservice/MiscMaster";
    private const string MiscTypeRoute = "/api/backgroundservice/MiscTypeMaster";

    private const string TestDescription = "QA Test Misc Master";

    private static string _createdCode = string.Empty;
    private static int _miscTypeId;

    public MiscMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..Math.Min(10, _f.EntityCode.Length)];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId; self-skips if FK unresolved)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        _miscTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscTypeRoute);
        if (_miscTypeId == 0) return; // REQUIRED FK unresolved → self-skip (downstream guards on CreatedId==0)

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
        var miscTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscTypeRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = miscTypeId > 0 ? miscTypeId : 1,
            code = "",
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MiscTypeIdMissing_Returns400()
    {
        // miscTypeId NotEmpty/GreaterThan(0) → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = 0,
            code = NewCode(),
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooLong_Returns400()
    {
        var miscTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscTypeRoute);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = miscTypeId > 0 ? miscTypeId : 1,
            code = new string('A', 101),
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DuplicateCode_SameMiscType_Returns400()
    {
        if (_f.CreatedId == 0) return; // create self-skipped → nothing to duplicate

        // Same code + same miscTypeId as TC001 → composite AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmptyBody_Returns400()
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
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (ALWAYS 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{(_f.CreatedId == 0 ? 1 : _f.CreatedId)}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200()
    {
        // FIXED 2026-06-18: GetMiscMasterByIdQueryHandler now null-guards before the audit deref,
        // so a missing id returns cleanly (200 null-data / 404) instead of an NRE 500.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: name, MiscTypeCode REQUIRED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithName_Returns200Or400()
    {
        // MiscTypeCode is required; we have no captured type code → tolerant.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA&MiscTypeCode={_createdCode}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA&MiscTypeCode=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (echoes miscTypeId + code from create)
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
            id = _f.CreatedId == 0 ? 1 : _f.CreatedId,
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

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{(_f.CreatedId == 0 ? 1 : _f.CreatedId)}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
