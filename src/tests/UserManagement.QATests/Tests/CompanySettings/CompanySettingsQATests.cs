// ─────────────────────────────────────────────────────────────────────────────
// CompanySettings — Live-server QA tests
//
// Route:    /api/CompanySettings  (controller CompanySettingsController)
//   • GET  /api/CompanySettings           → singleton GetById (NO id param).
//                                            Controller null-guards → 404 when not set.
//   • POST /api/CompanySettings           → CreateCompanySettingsCommand
//   • PUT  /api/CompanySettings/update     → UpdateCompanySettingsCommand
//
// Create shape (flat command):
//   CompanyId, PasswordHistoryCount, SessionTimeout, FailedLoginAttempts,
//   AutoReleaseTime, PasswordExpiryDays, PasswordExpiryAlert, TwoFactorAuth (byte),
//   MaxConcurrentLogins, ForgotPasswordCodeExpiry, CaptchaOnLogin (byte),
//   Currency (FK), Language (FK), TimeZone (FK), FinancialYear (FK).
//
// BLOCKED (create-happy): needs a valid FK combo (CompanyId + Currency + Language +
//   TimeZone + FinancialYear) AND a company that does not already have settings. The
//   FKs are resolved best-effort in the (skipped) create body for un-skipping later.
//
// ALWAYS-ACTIVE: GET singleton (Smoke, tolerant 200/404), no-auth 401 on GET + POST,
//   empty-body POST 400.
//
// Conventions: matches existing UserManagement.QATests.
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.CompanySettings;

[Collection("CompanySettingsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CompanySettingsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute   = "/api/CompanySettings";
    private const string UpdateRoute = "/api/CompanySettings/update";

    public CompanySettingsQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET (singleton, primary GET — Smoke)
    // Controller returns 404 when no company settings exist; 200 when present.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_Get_Singleton_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Get_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        await QAHelper.Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, await BuildValidCreatePayloadAsync());
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MissingRequiredFkFields_Returns400()
    {
        // CompanyId only, no Currency/Language/TimeZone/FinancialYear → validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { companyId = 1 });
        await QAHelper.Assert400Async(resp);
    }

    // BLOCKED: needs a valid FK combo and a company without existing settings.
    [Fact(Skip = "needs seeded data: CompanySettings Create requires valid CompanyId+Currency+Language+TimeZone+FinancialYear and a company without existing settings."), TestPriority(6)]
    public async Task TC006_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, await BuildValidCreatePayloadAsync());
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — UPDATE  (PUT /update)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(7)]
    public async Task TC007_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(UpdateRoute, await BuildValidUpdatePayloadAsync());
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Update_EmptyBody_Reachable()
    {
        // Empty body → either validation 400 or handler no-op; tolerate 200/400/404.
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // BLOCKED: depends on a valid FK combo + an existing settings row to update.
    [Fact(Skip = "needs seeded data: CompanySettings Update requires an existing settings row and a valid FK combo."), TestPriority(9)]
    public async Task TC009_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, await BuildValidUpdatePayloadAsync());
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Payload builders — FKs resolved best-effort at runtime (no hard-coded seed ids)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<object> BuildValidCreatePayloadAsync()
    {
        var companyId     = await ResolveFkAsync("/api/Company", fallback: 1);
        var currency      = await ResolveFkAsync("/api/Currency", fallback: 1);
        var language      = await ResolveFkAsync("/api/Language", fallback: 1);
        var financialYear = await ResolveFkAsync("/api/FinancialYear", fallback: 1);
        var timeZone      = 1; // TimeZones endpoint is non-paged; default to 1.

        return new
        {
            companyId,
            passwordHistoryCount     = 3,
            sessionTimeout           = 30,
            failedLoginAttempts      = 5,
            autoReleaseTime          = 15,
            passwordExpiryDays       = 90,
            passwordExpiryAlert      = 7,
            twoFactorAuth            = (byte)0,
            maxConcurrentLogins      = 2,
            forgotPasswordCodeExpiry = 10,
            captchaOnLogin           = (byte)0,
            currency,
            language,
            timeZone,
            financialYear
        };
    }

    private async Task<object> BuildValidUpdatePayloadAsync() => await BuildValidCreatePayloadAsync();

    private async Task<int> ResolveFkAsync(string route, int fallback)
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, route);
        return id > 0 ? id : fallback;
    }
}
