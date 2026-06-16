// ─────────────────────────────────────────────────────────────────────────────
// PasswordComplexityRule — live-server QA suite
//
// VERIFIED CONTRACT (from PasswordComplexityRuleController + Create/Update commands):
//   Route base ............ /api/PasswordComplexityRule        ([Route("api/[controller]")])
//   Create ................ POST   /api/PasswordComplexityRule  body { pwdComplexityRule }   → 200 { data }
//   GetAll ................ GET    /api/PasswordComplexityRule?PageNumber&PageSize&SearchTerm → 200 { Data,TotalCount,PageNumber,PageSize }
//   GetById ............... GET    /api/PasswordComplexityRule/{id}  — NO null guard → always 200
//   AutoComplete .......... GET    /api/PasswordComplexityRule/by-name?name=                  → 200 { Data }
//   Update ................ PUT    /api/PasswordComplexityRule  body { id, pwdComplexityRule, isActive(byte) }
//                            controller PRE-CHECKS GetById → 400 "ID mismatch" when not found
//   Delete ................ DELETE /api/PasswordComplexityRule/{id}  — ROUTE {id}
//                            controller PRE-CHECKS GetById → 404 "not found" when not found
//
//   pwdComplexityRule: NotEmpty + MaxLength(from EF metadata, default 150). No unique code,
//   no duplicate check, no FK. isActive is a byte (0/1) on update.
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.PasswordComplexityRule;

[Collection("PasswordComplexityRuleCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PasswordComplexityRuleQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PasswordComplexityRule";

    public PasswordComplexityRuleQATests(QAServerFixture fixture) => _f = fixture;

    // ── CREATE ───────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            pwdComplexityRule = $"QA Rule {_f.EntityCode[..10]}"
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
            pwdComplexityRule = $"QA NoAuth {_f.EntityCode[..6]}"
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
    public async Task TC004_Create_RuleEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { pwdComplexityRule = "" });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_RuleExceedsMaxLength_Returns400()
    {
        // PwdComplexityRule max length (EF metadata; default 150) — 300 chars is safely over any limit
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { pwdComplexityRule = new string('A', 300) });

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

    // ── GET BY ID (no null guard → always 200) ───────────────────────────────

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
    public async Task TC011_GetById_NonExistentId_Returns200_NullData()
    {
        // FIXED (test, 2026-06-16): live GetById validates existence → 400
        // "PasswordComplexityRule not found / Deleted." (not 200+null).
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
    public async Task TC013_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.Assert401Async(resp);
    }

    // ── UPDATE (body { id, pwdComplexityRule, isActive } ) ───────────────────

    [Fact, TestPriority(15)]
    public async Task TC015_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = $"QA Rule Upd {_f.EntityCode[..10]}",
            isActive          = 1
        });

        await QAHelper.AssertOkAsync(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "Updated");
    }

    [Fact, TestPriority(16)]
    public async Task TC016_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = $"QA Rule Inact {_f.EntityCode[..10]}",
            isActive          = 0
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = $"QA Rule React {_f.EntityCode[..10]}",
            isActive          = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(18)]
    public async Task TC018_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = "QA NoAuth Update",
            isActive          = 1
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(19)]
    public async Task TC019_Update_RuleEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = "",
            isActive          = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(20)]
    public async Task TC020_Update_RuleExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            pwdComplexityRule = new string('A', 300),
            isActive          = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Update_NonExistentId_Returns400_Mismatch()
    {
        // Controller pre-checks GetById; null → 400 "PasswordComplexityRule ID mismatch."
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = 999999,
            pwdComplexityRule = $"QA Ghost {_f.EntityCode[..6]}",
            isActive          = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── DELETE (ROUTE {id} — ALWAYS LAST; controller pre-checks GetById) ──────

    [Fact, TestPriority(23)]
    public async Task TC023_Delete_NonExistentId_Returns404()
    {
        // FIXED (test, 2026-06-16): live Delete validates existence → 400
        // "PasswordComplexityRule not found / Deleted." (not 404).
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "deleted");
    }

    [Fact, TestPriority(26)]
    public async Task TC026_Delete_AlreadyDeleted_Returns404()
    {
        // FIXED (test, 2026-06-16): after soft-delete, Delete existence validation → 400
        // "PasswordComplexityRule not found / Deleted." (live contract returns 400, not 404).
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
    }
}
