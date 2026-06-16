namespace UserManagement.QATests.Tests.Notifications;

// ─────────────────────────────────────────────────────────────────────────────
// NotificationsController — VERIFIED CONTRACT (UserManagement.Presentation/Controllers/NotificationsController.cs)
//   Route base: /api/Notifications   (ApiControllerBase, global JWT middleware)
//   POST /api/Notifications/PasswordResetNotifications  [FromBody] NotificationRequest { Username? }
//   NO GET endpoints.
//
// ACTION controller (sends a password-reset notification for a user). Coverage:
//   • ACTIVE  — no-auth POST → 401 (protected write endpoint)
//   • ACTIVE  — empty/invalid body → tolerant (400/404/200): the handler reacts to user
//               state; a missing username may surface as 400 (bad request) or 404 (user
//               not found) or 200 (no-op) depending on live backend behavior.
//   • SKIPPED — happy path (depends on a real username whose reset state/expiry is known)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("NotificationsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class NotificationsQATests
{
    private readonly QAServerFixture _f;
    private const string ResetRoute = "/api/Notifications/PasswordResetNotifications";

    public NotificationsQATests(QAServerFixture fixture) => _f = fixture;

    // ── ACTIVE: no-auth on a protected write endpoint → 401 ─────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_PasswordReset_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(ResetRoute, new { username = "someuser" });
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: reachability — empty body (no username) → tolerant ──────────────
    [Fact, TestPriority(2)]
    public async Task TC002_PasswordReset_EmptyBody_IsReachable()
    {
        var resp = await _f.Client.PostAsJsonAsync(ResetRoute, new { });
        // Username is optional on the request type; missing/unknown user may surface as
        // 400 (validation), 404 (user not found), or 200 (no-op). Assert reachable, not 5xx.
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── ACTIVE: reachability — non-existent username → tolerant ─────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_PasswordReset_NonExistentUser_IsReachable()
    {
        var resp = await _f.Client.PostAsJsonAsync(ResetRoute, new
        {
            username = $"no-such-user-{_f.EntityCode}"
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SKIPPED: happy path ─────────────────────────────────────────────────────
    [Fact(Skip = "needs seeded data: a real username whose password-reset state/expiry window is known so the notification actually dispatches. Un-skip when such a user is seeded in the QA clone."), TestPriority(4)]
    public async Task TC004_PasswordReset_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(ResetRoute, new { username = "testsales" });
        await QAHelper.AssertOkAsync(resp);
    }
}
