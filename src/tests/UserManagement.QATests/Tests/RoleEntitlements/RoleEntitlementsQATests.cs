namespace UserManagement.QATests.Tests.RoleEntitlements;

// ─────────────────────────────────────────────────────────────────────────────
// RoleEntitlementsController — VERIFIED CONTRACT (UserManagement.Presentation/Controllers/RoleEntitlementsController.cs)
//   Route base: /api/RoleEntitlements   (ApiControllerBase, global JWT middleware)
//   GET  /api/RoleEntitlements/{id}                    GetRoleEntitlementByIdQuery (id<=0 → controller 400)
//   POST /api/RoleEntitlements                         CreateRoleEntitlementCommand
//                                                        { RoleId, RoleModules[], RoleParents[],
//                                                          RoleChildren[], RoleMenuPrivileges[] }  (IRequirePermission CanAdd)
//   PUT  /api/RoleEntitlements                         UpdateRoleEntitlementCommand (nested arrays)
//   GET  /api/RoleEntitlements/roleprivileges/{UserId} GetRolePrivilegesQuery → { StatusCode, data:[] }
//
// RBAC controller (nested role/module/menu privilege graph). Coverage:
//   • ACTIVE (Smoke) — GET /roleprivileges/{userId} → tolerant 200/404 (reachability)
//   • ACTIVE          — no-auth on GET /roleprivileges/{userId} → 401
//   • ACTIVE          — empty-body POST → 400 (RoleId 0 / null nested arrays)
//   • ACTIVE          — GET /{id} with id=0 → 400 (controller guard)
//   • SKIPPED         — create/update (needs valid Role + Module + Menu ids to build the
//                       nested entitlement graph, plus the CanAdd permission)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("RoleEntitlementsCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class RoleEntitlementsQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/RoleEntitlements";

    public RoleEntitlementsQATests(QAServerFixture fixture) => _f = fixture;

    // ── ACTIVE (Smoke): GET role privileges for a user is reachable ─────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetRolePrivileges_HappyPath_Returns200()
    {
        // Resolve a real user id when possible; fall back to 1 (the QA clone has no guaranteed seed).
        var userId = await QAHelper.FirstIdAsync(_f.Client, "/api/UserRoleAllocation");
        if (userId <= 0) userId = 1;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/roleprivileges/{userId}");
        // Tolerant: 200 (privilege list, possibly empty) or 404 if the user has none.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── ACTIVE: no-auth on GET /roleprivileges/{userId} → 401 ───────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetRolePrivileges_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/roleprivileges/1");
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: GET /{id} with id=0 → 400 (controller guard) ────────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_GetById_IdZero_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
    }

    // ── ACTIVE: empty-body POST → 400 ───────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        // RoleId defaults to 0 and nested privilege arrays are null → validation rejects.
        await QAHelper.Assert400Async(resp);
    }

    // ── ACTIVE: no-auth on POST → 401 ───────────────────────────────────────────
    [Fact, TestPriority(5)]
    public async Task TC005_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { roleId = 1 });
        await QAHelper.Assert401Async(resp);
    }

    // ── SKIPPED: create role entitlement ────────────────────────────────────────
    [Fact(Skip = "needs seeded data: valid RoleId + Module/Parent/Child + Menu ids to build the nested entitlement graph, plus the testsales user holding CanAdd. Un-skip when an RBAC fixture is seeded."), TestPriority(6)]
    public async Task TC006_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            roleId             = 1,
            roleModules        = new object[0],
            roleParents        = new object[0],
            roleChildren       = new object[0],
            roleMenuPrivileges = new object[0]
        });
        await QAHelper.AssertOkAsync(resp);
    }

    // ── SKIPPED: update role entitlement ────────────────────────────────────────
    [Fact(Skip = "needs seeded data: an existing role entitlement plus valid nested Role/Module/Menu ids to update. Un-skip alongside the create happy path."), TestPriority(7)]
    public async Task TC007_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { roleId = 1 });
        await QAHelper.AssertOkAsync(resp);
    }
}
