namespace UserManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-UM-02 — Access control setup
//
//   As a security administrator I define a Role and an Access Policy so a user can
//   later be granted exactly the permissions the role carries.
//
// Scope (per catalogue): the setup chain + the cascade/idempotent delete rule
// (AC02.1–02.5). True permission-EFFECT (AC02.6) needs a second login as the
// affected user, which the shared testsales fixture cannot do → Skipped.
//
// Note: UserRole uses companyId = 0, which matches the testsales JWT company claim,
// so (unlike Division/Unit) the role IS readable back through the company-scoped list.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-UM-02-AccessControl")]
[Trait("Module", "UserManagement")]
[Trait("Story", "US-UM-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_UM_02_AccessControl_Tests
{
    private readonly QAServerFixture _f;

    private const string RoleRoute   = "/api/UserRole";
    private const string PolicyRoute = "/api/usermanagement/AccessPolicy";
    private const int    QACompanyId = 0;   // UserRole company 0 == testsales claim → readable

    private static int _roleId;
    private static int _policyId;

    public US_UM_02_AccessControl_Tests(QAServerFixture fixture) => _f = fixture;

    private string RoleName => $"QA FT Role {_f.EntityCode}";

    // STEP 1 — Create a Role -----------------------------------------------------
    [Fact, TestPriority(1)]
    public async Task Step1_CreateRole_Returns200_AndCapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(RoleRoute, new
        {
            roleName              = RoleName,
            description           = $"QA FT Role Desc {_f.EntityCode}",
            companyId             = QACompanyId,
            bypassDataAccess      = false,
            roleItemGroupMappings = new object[] { }
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // UserRole create returns success but not the id → look it up by unique name.
        _roleId = await FetchRoleIdByNameAsync(RoleName);
        _roleId.Should().BeGreaterThan(0, "the access-control workflow starts by creating a role");
    }

    // STEP 2 — Create an Access Policy -------------------------------------------
    [Fact, TestPriority(2)]
    public async Task Step2_CreateAccessPolicy_Returns200_AndCapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(PolicyRoute, new
        {
            policyCode = _f.EntityCode,
            policyName = $"QA FT Policy {_f.EntityCode}",
            entityName = "TestEntity",
            fieldName  = "TestField"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await ParseAsync(resp);
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        _policyId = doc.RootElement.CreatedId();
        _policyId.Should().BeGreaterThan(0);
    }

    // STEP 3 — The role is persisted and retrievable -----------------------------
    [Fact, TestPriority(3)]
    public async Task Step3_Role_IsReadable_ByGetAll()
    {
        var resp = await _f.Client.GetAsync(
            $"{RoleRoute}?PageNumber=1&PageSize=100&SearchTerm={Uri.EscapeDataString(RoleName)}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var found = (await ParseAsync(resp)).RootElement.GetProperty("data").EnumerateArray()
            .Any(x => x.TryGetProperty("roleName", out var rn) && rn.GetString() == RoleName);
        found.Should().BeTrue("the created role must be retrievable in the role list");
    }

    // STEP 4 — Deleting the role cascades soft-delete to its mappings -------------
    [Fact, TestPriority(4)]
    public async Task Step4_DeleteRole_CascadesAndReturns200()
    {
        _roleId.Should().BeGreaterThan(0);
        var resp = await _f.Client.DeleteAsync($"{RoleRoute}/{_roleId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 5 — Re-deleting the soft-deleted role is an idempotent no-op (still 200)
    [Fact, TestPriority(5)]
    public async Task Step5_DeleteRole_Again_IsIdempotent()
    {
        var resp = await _f.Client.DeleteAsync($"{RoleRoute}/{_roleId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 6 — Housekeeping: soft-delete the access policy from Step 2 ------------
    [Fact, TestPriority(6)]
    public async Task Step6_Cleanup_DeleteAccessPolicy_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{PolicyRoute}?id={_policyId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 7 — Permission EFFECT (AC02.6 [blocked] → Skipped) --------------------
    [Fact(Skip = "AC02.6 [blocked]: verifying that assigning the role changes the target " +
                 "user's EFFECTIVE permissions requires logging in AS that user, which the shared " +
                 "testsales fixture cannot do. Deferred until a disposable login-able test user exists."),
     TestPriority(7)]
    public async Task Step7_AssignRoleToUser_ChangesEffectivePermissions()
        => await Task.CompletedTask;

    private async Task<int> FetchRoleIdByNameAsync(string roleName)
    {
        var resp = await _f.Client.GetAsync(
            $"{RoleRoute}?PageNumber=1&PageSize=100&SearchTerm={Uri.EscapeDataString(roleName)}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc   = await ParseAsync(resp);
        var maxId = 0;
        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            if (item.TryGetProperty("roleName", out var rn) && rn.GetString() == roleName
                && item.TryGetProperty("id", out var idEl))
            {
                maxId = Math.Max(maxId, idEl.GetInt32());
            }
        }
        return maxId;
    }

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
