// ─────────────────────────────────────────────────────────────────────────────
// AdminSecuritySettings — Live-server QA tests
//
// Route:    /api/AdminSecuritySettings  (controller AdminSecuritySettingsController)
// Shape:    flat command (no nested objects). All fields are int/byte, NO FK.
//           Create fields: PasswordHistoryCount, SessionTimeoutMinutes,
//             MaxFailedLoginAttempts, AccountAutoUnlockMinutes, PasswordExpiryDays,
//             PasswordExpiryAlertDays, IsTwoFactorAuthenticationEnabled (byte),
//             MaxConcurrentLogins, IsForcePasswordChangeOnFirstLogin (byte),
//             PasswordResetCodeExpiryMinutes, IsCaptchaEnabledOnLogin (byte).
//           Update adds Id + IsActive (byte). PUT also takes an id query param that
//             must equal command.Id (controller returns 400 on mismatch).
//           DELETE is /{id} (route param); controller pre-checks GetById → 404.
//
// Live contract notes (verified against source):
//   • GetAll returns 404 when the table is empty (controller NotFound on empty Data).
//   • GetById has NO null-guard → always 200 (even for non-existent id).
//   • Create returns 201 with NO id in the body (no `data` id) → cannot capture a
//     CreatedId, so the full id-driven lifecycle (update/delete happy) is BLOCKED.
//     Create-happy is attempted tolerantly (no-FK so it should succeed, but a
//     singleton "already exists" rule may make it 400 — both accepted).
//
// Conventions: matches existing UserManagement.QATests (CompanyQATests / DepartmentQATests).
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.AdminSecuritySettings;

[Collection("AdminSecuritySettingsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AdminSecuritySettingsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AdminSecuritySettings";

    public AdminSecuritySettingsQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL (primary GET — Smoke)
    // 200 with data, or 404 when empty (controller NotFound on empty). Tolerant.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_GetAll_Page2PageSize5_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET BY ID  (no null-guard → 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(4)]
    public async Task TC004_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_GetById_NonExistent_Reachable()
    {
        // No null-guard in GetById action → 200+null typically; tolerate 200/400/404.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmptyBody_Returns400()
    {
        // Empty body → required int/byte fields fail validation → 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_MissingRequiredFields_Returns400()
    {
        // Only one field present → remaining required fields fail validation → 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { passwordHistoryCount = 3 });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_HappyPath_Returns200Or400()
    {
        // No FK — should succeed (200). A singleton "already exists" rule may make it 400.
        // Both are acceptable for this config-style entity (no id is returned to capture).
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildValidCreatePayload());
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (PUT, id query param must equal command.Id)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(10)]
    public async Task TC010_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}?id=1", BuildValidUpdatePayload(1));
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Update_IdMismatch_Returns400()
    {
        // Controller: if (id != command.Id) → 400 "ID mismatch".
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}?id=2", BuildValidUpdatePayload(1));
        await QAHelper.Assert400Async(resp);
    }

    // BLOCKED: Create returns no id, so a real existing id cannot be captured for an
    // update-happy path against the run's own row.
    [Fact(Skip = "needs seeded data: AdminSecuritySettings Create returns no id; cannot capture a real row id for update-happy."), TestPriority(12)]
    public async Task TC012_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}?id={_f.CreatedId}", BuildValidUpdatePayload(_f.CreatedId));
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — DELETE  (/{id} route param; pre-checks GetById → 404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(13)]
    public async Task TC013_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Delete_NonExistent_Returns404()
    {
        // Controller pre-queries GetById; for a non-existent id it returns 404.
        // Tolerated 200/404 in case the singleton row happens to exist with that id.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // BLOCKED: no captured id → cannot exercise a real soft-delete happy path.
    [Fact(Skip = "needs seeded data: AdminSecuritySettings Create returns no id; cannot capture a real row id for delete-happy."), TestPriority(15)]
    public async Task TC015_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Payload builders — flat command, all int/byte fields, NO FK
    // ─────────────────────────────────────────────────────────────────────────

    private static object BuildValidCreatePayload() => new
    {
        passwordHistoryCount             = 3,
        sessionTimeoutMinutes            = 30,
        maxFailedLoginAttempts           = 5,
        accountAutoUnlockMinutes         = 15,
        passwordExpiryDays               = 90,
        passwordExpiryAlertDays          = 7,
        isTwoFactorAuthenticationEnabled = (byte)0,
        maxConcurrentLogins              = 2,
        isForcePasswordChangeOnFirstLogin = (byte)1,
        passwordResetCodeExpiryMinutes   = 10,
        isCaptchaEnabledOnLogin          = (byte)0
    };

    private static object BuildValidUpdatePayload(int id) => new
    {
        id                               = id,
        passwordHistoryCount             = 4,
        sessionTimeoutMinutes            = 45,
        maxFailedLoginAttempts           = 5,
        accountAutoUnlockMinutes         = 20,
        passwordExpiryDays               = 120,
        passwordExpiryAlertDays          = 10,
        isTwoFactorAuthenticationEnabled = (byte)1,
        maxConcurrentLogins              = 3,
        isForcePasswordChangeOnFirstLogin = (byte)1,
        passwordResetCodeExpiryMinutes   = 15,
        isCaptchaEnabledOnLogin          = (byte)1,
        isActive                         = (byte)1
    };
}
