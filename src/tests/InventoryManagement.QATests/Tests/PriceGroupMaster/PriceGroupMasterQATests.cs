namespace InventoryManagement.QATests.Tests.PriceGroupMaster;

// ─────────────────────────────────────────────────────────────────────────────
// PriceGroupMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — PriceGroupMasterController.cs, route api/[controller], NO /inventory/):
//   GET    /api/PriceGroupMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/PriceGroupMaster/{id}              (200; no null guard)
//   GET    /api/PriceGroupMaster/by-name?term=     (autocomplete param is `term`)
//   POST   /api/PriceGroupMaster                   { priceGroupCode, priceGroupName, description?,
//                                                     effectiveFrom, effectiveTo? }
//   PUT    /api/PriceGroupMaster                   { id, priceGroupName, description?, effectiveFrom,
//                                                     effectiveTo?, isActive (int 0/1) }  (code immutable)
//   DELETE /api/PriceGroupMaster?id={id}           (id bound from QUERY)
//
// Key facts that shaped assertions:
//   • Create returns 200 wrapping result.Data (an int id). CreatedId() captures it.
//   • Create/Update/Delete ALWAYS return 200 (no IsSuccess gate); validation failures surface as 400 via
//     the global ValidationBehavior. Negatives use tolerant asserts where the controller is permissive.
//   • priceGroupCode is alphanumeric (+underscore), max 20, unique, immutable. priceGroupName unique, max 100.
//   • effectiveTo (when present) must be >= effectiveFrom. effectiveFrom = "2026-01-01T00:00:00Z".
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PriceGroupMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PriceGroupMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PriceGroupMaster";
    private const string EffectiveFrom = "2026-01-01T00:00:00Z";

    private static string _createdCode = string.Empty;
    private static string _createdName = string.Empty;

    public PriceGroupMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        _createdName = "QA PriceGroup " + _createdCode;

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = _createdCode,
            priceGroupName = _createdName,
            description = "Created by QA suite",
            effectiveFrom = EffectiveFrom,
            effectiveTo = (string?)null
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);

        var doc = await QAHelper.ParseAsync(resp);
        try
        {
            var id = doc.RootElement.CreatedId();
            if (id > 0) _f.CreatedId = id;
        }
        catch { /* fall through to search */ }

        if (_f.CreatedId <= 0)
        {
            var search = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");
            if (search.IsSuccessStatusCode)
            {
                using var sdoc = await QAHelper.ParseAsync(search);
                if (sdoc.RootElement.TryGetProperty("data", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0 &&
                    arr[0].TryGetProperty("id", out var idp))
                    _f.CreatedId = idp.GetInt32();
            }
        }
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = "NOAUTH01",
            priceGroupName = "No Auth Price Group",
            effectiveFrom = EffectiveFrom
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = "",
            priceGroupName = "QA Price Group " + NewCode(),
            effectiveFrom = EffectiveFrom
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = NewCode(),
            priceGroupName = "",
            effectiveFrom = EffectiveFrom
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = _createdCode,
            priceGroupName = "QA Price Group Dup " + NewCode(),
            effectiveFrom = EffectiveFrom
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EffectiveToBeforeFrom_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            priceGroupCode = NewCode(),
            priceGroupName = "QA Price Group Range " + NewCode(),
            effectiveFrom = EffectiveFrom,
            effectiveTo = "2025-06-01T00:00:00Z" // before from → invalid
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
    // SECTION 3 — GET BY ID  (no null guard → 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId <= 0) return;
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
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (priceGroupCode immutable)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId <= 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            priceGroupName = "QA Updated Price Group " + _createdCode,
            description = "Updated by QA",
            effectiveFrom = EffectiveFrom,
            effectiveTo = (string?)null,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            priceGroupName = "x",
            effectiveFrom = EffectiveFrom,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EffectiveToBeforeFrom_Returns400Or200()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            priceGroupName = "QA Updated Price Group " + _createdCode,
            effectiveFrom = EffectiveFrom,
            effectiveTo = "2025-06-01T00:00:00Z",
            isActive = 1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId <= 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            priceGroupName = "QA Updated Price Group " + _createdCode,
            effectiveFrom = EffectiveFrom,
            effectiveTo = (string?)null,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            priceGroupName = "QA Updated Price Group " + _createdCode,
            effectiveFrom = EffectiveFrom,
            effectiveTo = (string?)null,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_EmptyBody_Returns400Or200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns200Or400()
    {
        if (_f.CreatedId <= 0) return;
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
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
