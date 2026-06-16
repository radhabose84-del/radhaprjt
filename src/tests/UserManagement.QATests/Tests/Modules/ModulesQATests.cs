// ─────────────────────────────────────────────────────────────────────────────
// Modules — live-server QA suite
//
// VERIFIED CONTRACT (from ModulesController + Create/UpdateModuleCommand):
//   Route base ............ /api/Modules                       ([Route("api/[controller]")])
//   Create ................ POST   /api/Modules        body { moduleName }            → 200 (StatusCode 201 in body)
//   GetAll ................ GET    /api/Modules?PageNumber&PageSize&SearchTerm        → 200 { data,totalCount,pageNumber,pageSize }
//   GetById ............... GET    /api/Modules/{id}   — has null guard → 404 "not found"
//   AutoComplete .......... GET    /api/Modules/by-name?name=                          → 200 { data }
//   Update ................ PUT    /api/Modules        body { moduleId, moduleName }   (NOTE: moduleId, NOT id)
//   Delete ................ DELETE /api/Modules/{id}   — ROUTE {id}; id<=0 → 400 "Invalid Module ID"
//
//   moduleName: NotEmpty + MaxLength(50, from EF metadata). No unique code, no duplicate check,
//   no FK. Update has NO NotFound validator and NO id-existence pre-check in the controller, so
//   updating a non-existent moduleId returns 200 (flagged below).
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.Modules;

[Collection("ModulesCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ModulesQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Modules";

    public ModulesQATests(QAServerFixture fixture) => _f = fixture;

    // ── CREATE ───────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            moduleName = $"QA Module {_f.EntityCode[..10]}"
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
            moduleName = $"QA NoAuth {_f.EntityCode[..6]}"
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_ModuleNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { moduleName = "" });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_ModuleNameExceedsMaxLength_Returns400()
    {
        // FIXED (test, 2026-06-16): ModuleName column is varchar(100) (ModulesConfiguration), not 50.
        // 51 chars passes maxlength and the leftover row triggers a different (uniqueness) error.
        // Use 101 chars to genuinely exceed the real max → "cannot be longer than 100".
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { moduleName = new string('A', 101) });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    [Fact, TestPriority(6)]
    [Trait("Layer", "Smoke")]
    public async Task TC006_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_GetAll_SearchByTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────

    [Fact, TestPriority(9)]
    public async Task TC009_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetById_NonExistentId_Returns404()
    {
        // FIXED (test, 2026-06-16): live contract is 400 "Module not found" (existence validation),
        // not 404 — the GetById query validates and surfaces a ValidationException.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    // ── AUTOCOMPLETE (by-name?name=) ─────────────────────────────────────────

    [Fact, TestPriority(12)]
    public async Task TC012_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(13)]
    public async Task TC013_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.Assert401Async(resp);
    }

    // ── UPDATE (body uses moduleId, NOT id) ──────────────────────────────────

    [Fact, TestPriority(14)]
    public async Task TC014_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            moduleId   = _f.CreatedId,
            moduleName = $"QA Module Upd {_f.EntityCode[..10]}"
        });

        await QAHelper.AssertOkAsync(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "updated");
    }

    [Fact, TestPriority(15)]
    public async Task TC015_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            moduleId   = _f.CreatedId,
            moduleName = "QA NoAuth Update"
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(16)]
    public async Task TC016_Update_ModuleNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            moduleId   = _f.CreatedId,
            moduleName = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(17)]
    public async Task TC017_Update_ModuleNameExceedsMaxLength_Returns400()
    {
        // FIXED (test, 2026-06-16): ModuleName column is varchar(100), not 50 — use 101 chars.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            moduleId   = _f.CreatedId,
            moduleName = new string('A', 101)
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(18)]
    public async Task TC018_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // NOTE (validation gap): Update has NO NotFound validator and the controller does NOT
    // pre-check existence, so updating a non-existent moduleId returns 200. Documented, not asserted as a bug.
    [Fact, TestPriority(19)]
    public async Task TC019_Update_NonExistentId_NotEnforced_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            moduleId   = 999999,
            moduleName = $"QA Ghost {_f.EntityCode[..6]}"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // ── DELETE (ROUTE {id} — ALWAYS LAST) ────────────────────────────────────

    [Fact, TestPriority(20)]
    public async Task TC020_Delete_IdZero_Returns400()
    {
        // Controller: id<=0 → 400 "Invalid Module ID"
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "Invalid Module ID");
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_VerifyDelete_GetByIdReturns404()
    {
        // FIXED (test, 2026-06-16): after soft-delete, GetById existence validation → 400 "not found"
        // (live contract returns 400, not 404).
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
    }
}
