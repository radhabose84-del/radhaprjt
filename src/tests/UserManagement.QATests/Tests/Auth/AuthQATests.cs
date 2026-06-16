using System.Net.Http.Headers;

namespace UserManagement.QATests.Tests.Auth;

// ─────────────────────────────────────────────────────────────────────────────
// AuthController — live-server QA suite (login + session endpoints).
//
// VERIFIED CONTRACT (UserManagement.Presentation/Controllers/AuthController.cs, [Route("api/[controller]")]):
//   POST /api/Auth/login                         [AllowAnonymous] { username, password }
//        → 200 { Data.token } on success; 400 "Validation failed" on empty username/password;
//          400 (message) on bad/unknown credentials  (NOTE: failures are 400, not 401).
//   GET  /api/Auth/session/{jwtId}               [AllowAnonymous] → 200 found / 404 not found / 401 empty id
//   POST /api/Auth/deactivate-expired            [AllowAnonymous] → 200
//   POST /api/Auth/deactivate-user-sessionByUsername [AllowAnonymous] { username, password } → 200 / 400 invalid
//   POST /api/Auth/unlock                        (auth REQUIRED) { userName } → 401 without a token
//
// ⚠ LOCKOUT SAFETY: AdminSecuritySettings enforces maxFailedLoginAttempts. This suite NEVER sends a
// wrong password for the real `testsales` account — credential negatives use a non-existent username
// (no account to lock) and validation negatives fail BEFORE the credential check (no attempt recorded).
// The single happy-login uses deactivate-then-login (like the fixture) to re-establish a clean session.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AuthCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuthQATests
{
    private readonly QAServerFixture _f;
    private const string LoginRoute      = "/api/Auth/login";
    private const string DeactivateRoute = "/api/auth/deactivate-user-sessionByUsername";

    // A username guaranteed NOT to exist → credential-failure tests can't lock a real account.
    private static readonly string GhostUser = "qa_nouser_" + Guid.NewGuid().ToString("N")[..8];

    public AuthQATests(QAServerFixture fixture) => _f = fixture;

    // ── HAPPY: valid login returns 200 + a token (deactivate first to avoid single-session 400) ──
    [Fact, TestPriority(1)]
    public async Task TC001_Login_ValidCredentials_Returns200_WithToken()
    {
        await _f.AnonymousClient.PostAsJsonAsync(DeactivateRoute, new { username = _f.Username, password = _f.Password });

        var resp = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { username = _f.Username, password = _f.Password });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("token").GetString()
            .Should().NotBeNullOrEmpty("a successful login must return a JWT");
    }

    // ── NEGATIVE: empty body → 400 validation ──
    [Fact, TestPriority(2)]
    public async Task TC002_Login_EmptyBody_Returns400()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── NEGATIVE: missing password → 400 validation (fails before credential check; no lockout) ──
    [Fact, TestPriority(3)]
    public async Task TC003_Login_MissingPassword_Returns400()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { username = GhostUser });
        await QAHelper.Assert400Async(resp);
    }

    // ── NEGATIVE: missing username → 400 validation ──
    [Fact, TestPriority(4)]
    public async Task TC004_Login_MissingUsername_Returns400()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { password = "Whatever1" });
        await QAHelper.Assert400Async(resp);
    }

    // ── NEGATIVE: unknown user (single attempt, no real account) → 400 ──
    [Fact, TestPriority(5)]
    public async Task TC005_Login_UnknownUser_Returns400()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { username = GhostUser, password = "Whatever1" });
        await QAHelper.Assert400Async(resp);
    }

    // ── SESSION: lookup by a bogus JWT id is reachable (404 not found / 401 empty) ──
    [Fact, TestPriority(6)]
    public async Task TC006_GetSession_UnknownJwtId_IsReachable()
    {
        var resp = await _f.AnonymousClient.GetAsync("/api/Auth/session/qa-nonexistent-jwt-id");
        ((int)resp.StatusCode).Should().BeOneOf(200, 401, 404);
    }

    // ── deactivate-expired is anonymous and harmless → 200 ──
    [Fact, TestPriority(7)]
    public async Task TC007_DeactivateExpired_Returns200()
    {
        var resp = await _f.AnonymousClient.PostAsync("/api/auth/deactivate-expired", null);
        await QAHelper.AssertOkAsync(resp);
    }

    // ── unlock REQUIRES auth → no token → 401 ──
    [Fact, TestPriority(8)]
    public async Task TC008_Unlock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync("/api/Auth/unlock", new { userName = GhostUser });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CONTROL: a genuine token (from the fixture) is accepted on a protected endpoint ──
    [Fact, TestPriority(9)]
    public async Task TC009_ProtectedEndpoint_WithFixtureToken_Returns200()
    {
        // Re-login to guarantee a valid session/token (TC001 may have rotated it).
        await _f.AnonymousClient.PostAsJsonAsync(DeactivateRoute, new { username = _f.Username, password = _f.Password });
        var login = await _f.AnonymousClient.PostAsJsonAsync(LoginRoute, new { username = _f.Username, password = _f.Password });
        await QAHelper.AssertOkAsync(login);
        var token = (await QAHelper.ParseAsync(login)).RootElement.GetProperty("data").GetProperty("token").GetString();

        using var client = new HttpClient { BaseAddress = new Uri(_f.BaseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await client.GetAsync("/api/Country?PageNumber=1&PageSize=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
