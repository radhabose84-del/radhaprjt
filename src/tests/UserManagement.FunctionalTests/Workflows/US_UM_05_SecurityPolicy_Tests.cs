namespace UserManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-UM-05 — Security policy configuration
//
//   As a security administrator I define password-complexity, admin-security and
//   company-settings policies so authentication is governed by the configured rules.
//
// This is a WORKFLOW test: it configures the security-policy surface end to end —
// a PasswordComplexityRule (full create→read-back lifecycle), AdminSecuritySettings
// (config/singleton create), and CompanySettings (singleton read + blocked create).
//
// Notes from the catalogue (Stories/Story-Catalogue.md) that shape these assertions:
//   • AC05.2 [verify]: AdminSecuritySettings Create returns 201 with NO id in the body
//     (config/singleton) → cannot capture a CreatedId; a "singleton already exists" rule
//     may also make it 400. Assert TOLERANTLY (200/201/400).
//   • AC05.3 [blocked]: CompanySettings Create needs a valid FK combo (CompanyId +
//     Currency + Language + TimeZone + FinancialYear) and a company without existing
//     settings → Skipped.
//   • AC05.4: CompanySettings GET is a singleton; controller null-guards → 404 when not
//     set. ⚠ Tolerant 200/404.
//   • Routes verified against PasswordComplexityRuleQATests / AdminSecuritySettingsQATests
//     / CompanySettingsQATests:
//       PasswordComplexityRule : POST {pwdComplexityRule}; PUT {id,pwdComplexityRule,isActive};
//                                DELETE /{id} (ROUTE, controller pre-checks GetById → 404)
//       AdminSecuritySettings  : POST flat int/byte payload; DELETE /{id}
//       CompanySettings        : GET singleton (no id); POST flat payload; PUT /update
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-UM-05-SecurityPolicy")]
[Trait("Module", "UserManagement")]
[Trait("Story", "US-UM-05")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_UM_05_SecurityPolicy_Tests
{
    private readonly QAServerFixture _f;

    private const string PwdRuleRoute              = "/api/PasswordComplexityRule";
    private const string AdminSecurityRoute        = "/api/AdminSecuritySettings";
    private const string CompanySettingsRoute      = "/api/CompanySettings";

    // Workflow state carried across ordered steps (static — collection runs serially).
    private static int _pwdRuleId;

    public US_UM_05_SecurityPolicy_Tests(QAServerFixture fixture) => _f = fixture;

    // STEP 1 (AC05.1) — PasswordComplexityRule create + read-back ----------------
    [Fact, TestPriority(1)]
    public async Task Step1_CreatePasswordComplexityRule_AndReadBack()
    {
        var ruleText = $"QA FT Rule {_f.EntityCode[..10]}";

        var create = await _f.Client.PostAsJsonAsync(PwdRuleRoute, new
        {
            pwdComplexityRule = ruleText
        });
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        _pwdRuleId = (await ParseAsync(create)).RootElement.CreatedId();
        _pwdRuleId.Should().BeGreaterThan(0, "the security-policy workflow starts with a password rule");

        // Read back by id — GetById has no null guard → 200 for the new row.
        var read = await _f.Client.GetAsync($"{PwdRuleRoute}/{_pwdRuleId}");
        read.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 2 (AC05.2) — AdminSecuritySettings create (config/singleton) ----------
    [Fact, TestPriority(2)]
    public async Task Step2_CreateAdminSecuritySettings_IsReachable()
    {
        // ⚠ AC05.2 [verify]: no FK → should succeed (200/201), but Create returns no id body
        //   and a singleton "already exists" rule may make it 400. Assert tolerantly.
        var resp = await _f.Client.PostAsJsonAsync(AdminSecurityRoute, new
        {
            passwordHistoryCount              = 3,
            sessionTimeoutMinutes             = 30,
            maxFailedLoginAttempts            = 5,
            accountAutoUnlockMinutes          = 15,
            passwordExpiryDays                = 90,
            passwordExpiryAlertDays           = 7,
            isTwoFactorAuthenticationEnabled  = (byte)0,
            maxConcurrentLogins               = 2,
            isForcePasswordChangeOnFirstLogin = (byte)1,
            passwordResetCodeExpiryMinutes    = 10,
            isCaptchaEnabledOnLogin           = (byte)0
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201, 400);
    }

    // STEP 3 (AC05.3) — CompanySettings create (BLOCKED → Skipped) ---------------
    [Fact(Skip = "needs seeded data: AC05.3 CompanySettings Create requires a valid FK combo " +
                 "(CompanyId+Currency+Language+TimeZone+FinancialYear) and a company without " +
                 "existing settings — see CompanySettingsQATests."),
     TestPriority(3)]
    public async Task Step3_CreateCompanySettings_ForCompany()
    {
        // Intended once seeded: POST /api/CompanySettings with the resolved FK combo → 200.
        var resp = await _f.Client.PostAsJsonAsync(CompanySettingsRoute, new
        {
            companyId                = 1,
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
            currency                 = 1,
            language                 = 1,
            timeZone                 = 1,
            financialYear            = 1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 4 (AC05.4) — CompanySettings singleton GET is readable ----------------
    [Fact, TestPriority(4)]
    public async Task Step4_GetCompanySettings_Singleton_IsReachable()
    {
        // Singleton GET (no id). ⚠ Controller null-guards → 404 when no settings exist for the
        //   JWT company; 200 when present. Tolerate both.
        var resp = await _f.Client.GetAsync(CompanySettingsRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // STEP 5 (AC05.5) — Teardown removes the policy rows where supported (LAST) --
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown_DeletesSupportedPolicyRows()
    {
        // ⚠ AC05.5 [verify]: only PasswordComplexityRule supports a delete (DELETE /{id} ROUTE,
        //   controller pre-checks GetById → 404 when missing). AdminSecuritySettings /
        //   CompanySettings have no captured id / no per-row delete here → not torn down.
        _pwdRuleId.Should().BeGreaterThan(0, "Step 1 must have created the password rule");

        var resp = await _f.Client.DeleteAsync($"{PwdRuleRoute}/{_pwdRuleId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
