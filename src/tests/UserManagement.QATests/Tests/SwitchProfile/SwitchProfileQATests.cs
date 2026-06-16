namespace UserManagement.QATests.Tests.SwitchProfile;

// ─────────────────────────────────────────────────────────────────────────────
// SwitchProfileController — VERIFIED CONTRACT (UserManagement.Presentation/Controllers/SwitchProfileController.cs)
//   Route base: /api/SwitchProfile   (ApiControllerBase, global JWT middleware)
//   GET  /api/SwitchProfile/by-name          GetUnitProfileQuery   → { message, statusCode, data }
//   POST /api/SwitchProfile/SwitchProfile     SwitchProfileByUnitCommand { UnitId, CompanyId, DivisionId, OldUnitId? }
//
// ACTION controller (switches the logged-in user's active unit/company profile). Coverage:
//   • ACTIVE (Smoke) — GET /by-name (unit profile list for current user) → tolerant 200/404
//   • ACTIVE          — no-auth on GET /by-name → 401
//   • ACTIVE          — no-auth on POST /SwitchProfile → 401
//   • ACTIVE          — empty-body POST → 400 (UnitId/CompanyId/DivisionId default 0)
//   • SKIPPED         — switch happy path (needs a valid unit/company/division the testsales
//                       user is actually entitled to switch into)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SwitchProfileCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SwitchProfileQATests
{
    private readonly QAServerFixture _f;
    private const string ByNameRoute = "/api/SwitchProfile/by-name";
    private const string SwitchRoute = "/api/SwitchProfile/SwitchProfile";

    public SwitchProfileQATests(QAServerFixture fixture) => _f = fixture;

    // ── ACTIVE (Smoke): GET /by-name reachable for the authenticated user ───────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetUnitProfiles_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync(ByNameRoute);
        // Tolerant: 200 with a unit list, or 404 if the user has no switchable profiles.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── ACTIVE: no-auth on GET /by-name → 401 ───────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetUnitProfiles_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(ByNameRoute);
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: no-auth on POST /SwitchProfile → 401 ────────────────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_SwitchProfile_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(SwitchRoute, new
        {
            unitId     = 1,
            companyId  = 1,
            divisionId = 1
        });
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: empty-body POST → 400 ───────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_SwitchProfile_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(SwitchRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SKIPPED: switch happy path ──────────────────────────────────────────────
    [Fact(Skip = "needs seeded data: a valid UnitId/CompanyId/DivisionId combination that the testsales user is entitled to switch into. Un-skip when those profile assignments are seeded."), TestPriority(5)]
    public async Task TC005_SwitchProfile_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(SwitchRoute, new
        {
            unitId     = 1,
            companyId  = 1,
            divisionId = 1,
            oldUnitId  = "1"
        });
        await QAHelper.AssertOkAsync(resp);
    }
}
