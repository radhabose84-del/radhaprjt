namespace UserManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-UM-06 — User onboarding & access  (PARTIALLY BLOCKED — needs seeded FK chain)
//
//   As an administrator I create an Entity, onboard a User under it, allocate roles,
//   and drive the password flow so the user can log in with the right access.
//
// This is a WORKFLOW test: it creates the onboarding root (Entity), then exercises the
// user-create / role-allocation / password chain. Most of that chain is [blocked] in the
// catalogue because it needs a seeded userGroup/department/role/unit/division FK chain
// that the QA clone does not guarantee — those steps are Skipped with a precise reason.
//
// Notes from the catalogue (Stories/Story-Catalogue.md) that shape these assertions:
//   • AC06.1: Entity create is satisfiable (no required FK; entityCode auto-generated).
//   • AC06.2/06.3 [blocked]: User create needs 5 nested arrays + group/dept/role/unit/
//     division ids; role allocation depends on a real created user → Skipped.
//   • AC06.4 [blocked]: the password flow needs a real created user; reset endpoints are
//     [AllowAnonymous] (reachability only) — kept Skipped per catalogue.
//   • AC06.5: the Entity is readable by id and lists its companies (404 when none).
//   • Routes verified against EntityQATests:
//       Entity : POST {entityName,entityDescription,address,phone,email} (entityCode auto);
//                GET /api/Entity/{id} (id<=0 → 400) ; GET /api/Entity/{id}/companies (200/404);
//                DELETE /api/Entity/{id} (ROUTE)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-UM-06-UserOnboarding")]
[Trait("Module", "UserManagement")]
[Trait("Story", "US-UM-06")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_UM_06_UserOnboarding_Tests
{
    private readonly QAServerFixture _f;

    private const string EntityRoute = "/api/Entity";
    private const string UserRoute   = "/api/User";

    // Workflow state carried across ordered steps (static — collection runs serially).
    private static int _entityId;

    public US_UM_06_UserOnboarding_Tests(QAServerFixture fixture) => _f = fixture;

    // STEP 1 (AC06.1) — Create the onboarding root: an Entity --------------------
    [Fact, TestPriority(1)]
    public async Task Step1_CreateEntity_Returns200_AndCapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(EntityRoute, new
        {
            entityName        = $"QA FT Entity {_f.EntityCode[..10]}",
            entityDescription = "QA FT onboarding entity",
            address           = "123 QA Street",
            phone             = "9876543210",                          // 10-digit → passes mobile rule
            email             = $"qaft{_f.EntityCode[..6]}@bannari.test"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _entityId = (await ParseAsync(resp)).RootElement.CreatedId();
        _entityId.Should().BeGreaterThan(0, "the onboarding workflow starts by creating an Entity");
    }

    // STEP 2 (AC06.2) — Create a User under the Entity (BLOCKED → Skipped) -------
    [Fact(Skip = "needs seeded data: AC06.2 User create requires a seeded FK chain " +
                 "(userGroup/department/role/unit/division ids) + 5 nested arrays that the QA " +
                 "clone does not guarantee — see UserQATests."),
     TestPriority(2)]
    public async Task Step2_CreateUser_UnderEntity()
    {
        // Intended once seeded: POST /api/User with nested group/dept/role/unit/division arrays
        //   referencing _entityId → 200, capturing the new user id for Step 3/4.
        var resp = await _f.Client.PostAsJsonAsync(UserRoute, new
        {
            entityId = _entityId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 3 (AC06.3) — Allocate roles to the user (BLOCKED → Skipped) ----------
    [Fact(Skip = "needs seeded data: AC06.3 role allocation depends on a real created user " +
                 "(Step 2 is blocked) and seeded role ids — see UserRoleAllocationQATests."),
     TestPriority(3)]
    public async Task Step3_AllocateRolesToUser()
    {
        // Intended once seeded: POST /api/UserRoleAllocation { userId, roleIds[] } → 200/201.
        var resp = await _f.Client.PostAsJsonAsync("/api/UserRoleAllocation", new
        {
            userId  = 0,
            roleIds = new[] { _f.ValidRoleId }
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
    }

    // STEP 4 (AC06.4) — First-time password / reset flow (BLOCKED → Skipped) ----
    [Fact(Skip = "needs seeded data: AC06.4 password flow needs a real created user (Step 2 " +
                 "blocked); reset endpoints are [AllowAnonymous] (reachability only) — kept " +
                 "Skipped per catalogue."),
     TestPriority(4)]
    public async Task Step4_FirstTimePasswordAndResetFlow()
    {
        // Intended once a user exists: set first-time password → request reset code → reset.
        var resp = await _f.AnonymousClient.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            username = "qa-ft-user"
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // STEP 5 (AC06.5) — Entity readable by id + lists its companies -------------
    [Fact, TestPriority(5)]
    public async Task Step5_Entity_IsReadableById_AndListsCompanies()
    {
        _entityId.Should().BeGreaterThan(0, "Step 1 must have created the Entity first");

        // GetById: id<=0 → 400; positive id → 200.
        var byId = await _f.Client.GetAsync($"{EntityRoute}/{_entityId}");
        byId.StatusCode.Should().Be(HttpStatusCode.OK);

        // /{id}/companies → 200 when the entity has companies, 404 when none. ⚠ Tolerant.
        var companies = await _f.Client.GetAsync($"{EntityRoute}/{_entityId}/companies");
        ((int)companies.StatusCode).Should().BeOneOf(200, 404);
    }

    // STEP 6 — Teardown removes the onboarding root (ALWAYS LAST) ----------------
    [Fact, TestPriority(6)]
    public async Task Step6_Teardown_DeletesEntity()
    {
        _entityId.Should().BeGreaterThan(0, "Step 1 must have created the Entity");

        // DELETE /api/Entity/{id} (ROUTE) → 200 soft-delete; tolerate 400/500 if a downstream
        // cascade trips (no GetById guard) — this is teardown.
        var resp = await _f.Client.DeleteAsync($"{EntityRoute}/{_entityId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 500);
    }

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
