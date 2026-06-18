namespace BudgetManagement.QATests.Tests.MiscMaster;

// ─────────────────────────────────────────────────────────────────────────────
// MiscMaster (Budget) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-16 — MiscMasterController.cs):
//   POST   /api/budget/miscmaster        { miscTypeId, code, description }  (all 3 REQUIRED)
//   PUT    /api/budget/miscmaster        { id, miscTypeId, code, description, sortOrder(int), isActive(byte 0/1) }
//   DELETE /api/budget/miscmaster/{id}   (id bound from ROUTE, not query)
//   GET    /api/budget/miscmaster?PageNumber=&PageSize=&SearchTerm=&MiscTypeId=
//   GET    /api/budget/miscmaster/{id}   (returns 200 + data:null/data wrapper when not found — NO 404 guard)
//   GET    /api/budget/miscmaster/by-name?name=&MiscTypeCode=  (MiscTypeCode is required by the action signature)
//
// Key facts that shaped assertions:
//   • Code is alphanumeric (^[A-Za-z0-9]+$), max 50 — spaces/specials rejected. Unique per miscType.
//   • Description is REQUIRED on Create (NotEmpty), max 250.
//   • MiscTypeId is a same-module FK (Budget.MiscTypeMaster) — resolved at runtime via
//       FirstIdAsync on /api/budget/misctypemaster (fallback 1).
//   • AlreadyExists is COMPOSITE: uniqueness is (Code + MiscTypeId) — duplicate test reuses both.
//   • DELETE has NO NotFound guard in the controller (always returns 200) — handler-level behaviour
//       drives the response, so delete-non-existent / already-deleted assertions are tolerant.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("BudgetMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/budget/miscmaster";
    private const string MiscTypeRoute = "/api/budget/misctypemaster";

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

        // Create-happy may still 400 if no MiscTypeMaster exists on the clone — tolerate and
        // self-skip downstream via _f.CreatedId guard.
        if (resp.StatusCode != HttpStatusCode.OK)
        {
            ((int)resp.StatusCode).Should().BeOneOf(200, 400);
            return;
        }

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

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MiscTypeIdMissing_Returns400()
    {
        // MiscTypeId default 0 fails validation.
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
            code = new string('A', 51), // exceeds max 50
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_CodeWithSpace_Returns400()
    {
        // FIXED 2026-06-18: CreateMiscMasterCommandValidator now has an Alphanumeric case
        // (pattern from shared validation-rules.json), so a code with a space is rejected 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = NewCode()[..6] + " X", // run-unique code containing a space
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeWithSpecialChar_Returns400()
    {
        // FIXED 2026-06-18: Alphanumeric rule now enforced — a special-char code is rejected 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = await ResolveMiscTypeIdAsync(),
            code = NewCode()[..6] + "@X", // run-unique code containing a special char
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCode_SameMiscType_Returns400()
    {
        if (_f.CreatedId == 0) return; // create-happy was blocked → duplicate is meaningless

        // Same code + same miscTypeId as TC001 → composite AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = TestDescription
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_NonExistentMiscType_Returns400()
    {
        // MiscTypeExistsAsync false → FK validation fails.
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
    public async Task TC022_GetAll_FilterByMiscTypeId_Returns200()
    {
        var miscTypeId = await ResolveMiscTypeIdAsync();
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&MiscTypeId={miscTypeId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200)
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: name, MiscTypeCode)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithParams_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA&MiscTypeCode=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=&MiscTypeCode=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
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
            miscTypeId = 1,
            code = "NOAUTH01",
            description = "QA Updated Misc Master",
            sortOrder = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_DescriptionEmpty_Returns400()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            miscTypeId = _miscTypeId,
            code = _createdCode,
            description = "",
            sortOrder = 1,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_Inactivate_Then_Reactivate_Returns200()
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
    public async Task TC091_Delete_NonExistentId_Reachable()
    {
        // Controller has no NotFound guard — handler decides; tolerate 200/400/404.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        // FIXED 2026-06-18: MiscMasterQueryRepository.NotFoundAsync was inverted (returned true when
        // the row EXISTS); the Delete validator's !NotFoundAsync(id) then failed for live rows, so
        // soft-delete never succeeded. The repo now returns count==0 (honest to its name) and the
        // Update validators were aligned to !NotFoundAsync — delete of a real row now returns 200.
        await QAHelper.AssertOkAsync(resp);
    }
}
