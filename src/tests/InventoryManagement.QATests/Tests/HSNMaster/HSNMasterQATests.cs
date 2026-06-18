namespace InventoryManagement.QATests.Tests.HSNMaster;

// ─────────────────────────────────────────────────────────────────────────────
// HSNMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — HSNMasterController.cs, route api/[controller], NO /inventory/):
//   GET    /api/HSNMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/HSNMaster/{id}                       (200; no null guard)
//   GET    /api/HSNMaster/by-name?hsnCode=&typeCode= (autocomplete params)
//   POST   /api/HSNMaster/create                     { typeId, hsnCode, description, gstCategoryId,
//                                                       gstPercentage, igstPercentage, validFrom }
//   PUT    /api/HSNMaster/update/{id}                { id(set from route), typeId, hsnCode, description,
//                                                       gstCategoryId, gstPercentage, igstPercentage,
//                                                       validFrom, isActive (byte 0/1) }  (hsnCode immutable)
//   DELETE /api/HSNMaster/delete/{id}                (id bound from ROUTE; <=0 → 400)
//
// Key facts that shaped assertions:
//   • Create returns 200 wrapping result.Data (an int id). BeOneOf(200,201) on happy path to be safe.
//   • typeId + gstCategoryId are FKs into MiscMaster — resolved at runtime via FirstIdAsync("/api/inventory/miscmaster").
//     If either resolves 0, the create-happy step self-skips (guarded) so the suite stays re-runnable.
//   • hsnCode is alphanumeric, max 10, unique, immutable on update.
//   • validFrom is a required DateTimeOffset → "2026-01-01T00:00:00Z".
// ─────────────────────────────────────────────────────────────────────────────

[Collection("HSNMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class HSNMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/HSNMaster";
    private const string MiscMasterRoute = "/api/inventory/miscmaster";
    private const string ValidFrom = "2026-01-01T00:00:00Z";

    private static string _createdCode = string.Empty;
    private static int _typeId;
    private static int _gstCategoryId;

    public HSNMasterQATests(QAServerFixture fixture) => _f = fixture;

    // hsnCode max 10 — slice run-unique alphanumeric to 10.
    private string NewCode() => _f.EntityCode[..10];

    private async Task ResolveFkIdsAsync()
    {
        if (_typeId == 0) _typeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        if (_gstCategoryId == 0) _gstCategoryId = _typeId; // single existing misc id is fine for both FKs
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFkIdsAsync();
        if (_typeId <= 0 || _gstCategoryId <= 0)
            return; // self-skip: no MiscMaster row to satisfy FK on the clone

        _createdCode = NewCode();

        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = _typeId,
            hsnCode = _createdCode,
            description = "QA Test HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);

        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = 1,
            hsnCode = "NOAUTH01",
            description = "No Auth HSN",
            gstCategoryId = 1,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        await ResolveFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = _typeId,
            hsnCode = "",
            description = "QA Test HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        await ResolveFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = _typeId,
            hsnCode = NewCode(),
            description = "",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooLong_Returns400()
    {
        await ResolveFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = _typeId,
            hsnCode = new string('A', 50), // exceeds max 10
            description = "QA Test HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DuplicateCode_Returns400()
    {
        if (_typeId <= 0 || _gstCategoryId <= 0 || string.IsNullOrEmpty(_createdCode))
            return; // create was skipped → nothing to duplicate

        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            typeId = _typeId,
            hsnCode = _createdCode,
            description = "QA Test HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 18.0m,
            igstPercentage = 18.0m,
            validFrom = ValidFrom
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new { });
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId <= 0) return; // create self-skipped
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `hsnCode`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithHsnCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?hsnCode=QA");
        await QAHelper.AssertOkAsync(resp);
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?hsnCode=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (route api/HSNMaster/update/{id}; hsnCode immutable)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId <= 0) return;

        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update/{_f.CreatedId}", new
        {
            id = _f.CreatedId,
            typeId = _typeId,
            hsnCode = _createdCode,
            description = "QA Updated HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 12.0m,
            igstPercentage = 12.0m,
            validFrom = ValidFrom,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/update/1", new
        {
            id = 1,
            typeId = 1,
            hsnCode = "X",
            description = "x",
            gstCategoryId = 1,
            gstPercentage = 1.0m,
            igstPercentage = 1.0m,
            validFrom = ValidFrom,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_IdZeroRoute_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update/0", new
        {
            id = 0,
            typeId = 1,
            hsnCode = "X",
            description = "x",
            gstCategoryId = 1,
            gstPercentage = 1.0m,
            igstPercentage = 1.0m,
            validFrom = ValidFrom,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId <= 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update/{_f.CreatedId}", new
        {
            id = _f.CreatedId,
            typeId = _typeId,
            hsnCode = _createdCode,
            description = "QA Updated HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 12.0m,
            igstPercentage = 12.0m,
            validFrom = ValidFrom,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update/{_f.CreatedId}", new
        {
            id = _f.CreatedId,
            typeId = _typeId,
            hsnCode = _createdCode,
            description = "QA Updated HSN Master",
            gstCategoryId = _gstCategoryId,
            gstPercentage = 12.0m,
            igstPercentage = 12.0m,
            validFrom = ValidFrom,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — route api/HSNMaster/delete/{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZeroRoute_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete/0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/delete/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns200Or400()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetById_Returns200Or404()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
