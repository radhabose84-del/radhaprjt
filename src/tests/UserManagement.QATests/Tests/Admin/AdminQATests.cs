namespace UserManagement.QATests.Tests.Admin;

// ─────────────────────────────────────────────────────────────────────────────
// AdminController — VERIFIED CONTRACT (UserManagement.Presentation/Controllers/AdminController.cs)
//   Route base: /api/Admin   (ApiControllerBase, no [Authorize] on controller — global JWT middleware)
//   POST  /api/Admin                  CreateEntityLevelAdminCommand { Email?, EntityId, CompanyId }  (IRequirePermission CanAdd)
//   POST  /api/Admin/SendOTP          SendOTPCommand
//   PUT   /api/Admin/SetAdminPassword ResetPasswordCommand
//   NO GET endpoints on this controller.
//
// This is an ACTION/RBAC controller, not standard CRUD. Coverage here is:
//   • ACTIVE  — empty-body POST → 400 (model/validation rejects)
//   • ACTIVE  — no-auth POST → 401 (write endpoint must reject anonymous)
//   • SKIPPED — create-admin happy path (needs a valid EntityId/CompanyId + OTP workflow + the
//               testsales user holding CanAdd on the admin entity — not available without seeded data)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AdminCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AdminQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Admin";

    public AdminQATests(QAServerFixture fixture) => _f = fixture;

    // ── ACTIVE: reachability — empty-body POST is rejected (400) ─────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        // EntityId/CompanyId default to 0 → validation/binding rejects with 400.
        await QAHelper.Assert400Async(resp);
    }

    // ── ACTIVE: no-auth on a protected write endpoint → 401 ─────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            email     = $"qa{_f.EntityCode}@example.com",
            entityId  = 1,
            companyId = 1
        });
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: no-auth on SetAdminPassword (PUT) → 401 ─────────────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_SetAdminPassword_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/SetAdminPassword", new { });
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: empty-body SendOTP is rejected (400) ────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_SendOTP_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/SendOTP", new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SKIPPED: create-admin happy path ────────────────────────────────────────
    [Fact(Skip = "needs seeded data: a valid EntityId + CompanyId, the OTP send/verify workflow, and the testsales user holding CanAdd on the admin entity. Un-skip when a seeded admin-entity fixture exists."), TestPriority(5)]
    public async Task TC005_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            email     = $"qa{_f.EntityCode}@example.com",
            entityId  = 1,
            companyId = 1
        });
        await QAHelper.AssertOkAsync(resp);
    }
}
