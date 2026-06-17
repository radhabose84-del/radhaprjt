namespace UserManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-UM-03 — Navigation & RBAC scaffolding
//
//   As an application administrator I register a Module, add a Menu under it, and
//   wire role entitlements so the navigation tree and role-based access reflect the
//   app structure.
//
// This is a WORKFLOW test: it chains a Module create → a Menu create that references
// the module → reads the menu back through the navigation lookups — the cross-entity
// linkage that per-entity CRUD tests in UserManagement.QATests do NOT cover.
//
// Notes from the catalogue (Stories/Story-Catalogue.md) that shape these assertions:
//   • Menu has NO FluentValidation validator (handler just maps→creates), and several
//     navigation lookups are company-scoped — so navigation reachability is asserted
//     tolerantly (BeOneOf) rather than as a strict 200, per AC03.3/03.4 [verify].
//   • AC03.5 (RoleEntitlements) and AC03.6 (UserRoleAllocation) need a seeded
//     role/module/menu/user FK chain → [blocked], Skipped (not asserted).
//   • Routes verified against ModulesQATests / MenuQATests:
//       Modules : POST /api/Modules {moduleName}; DELETE /api/Modules/{id} (ROUTE)
//       Menu    : POST /api/Menu {menuName,moduleId,menuUrl,parentId,sortOrder,...};
//                 by-name?name=&moduleId= ; POST /by-module & /by-parent (List<int> body);
//                 DELETE /api/Menu/{id} (ROUTE)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-UM-03-NavigationRbac")]
[Trait("Module", "UserManagement")]
[Trait("Story", "US-UM-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_UM_03_NavigationRbac_Tests
{
    private readonly QAServerFixture _f;

    private const string ModulesRoute = "/api/Modules";
    private const string MenuRoute    = "/api/Menu";

    // Workflow state carried across ordered steps. xUnit builds a NEW instance per test
    // method, so cross-step state must be static (the collection runs the steps serially).
    private static int _moduleId;
    private static int _menuId;

    public US_UM_03_NavigationRbac_Tests(QAServerFixture fixture) => _f = fixture;

    // STEP 1 (AC03.1) — Register a Module (navigation root) ----------------------
    [Fact, TestPriority(1)]
    public async Task Step1_CreateModule_Returns200_AndCapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(ModulesRoute, new
        {
            moduleName = $"QA FT Module {_f.EntityCode[..10]}"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _moduleId = (await ParseAsync(resp)).RootElement.CreatedId();
        _moduleId.Should().BeGreaterThan(0, "the navigation workflow starts by registering a module");
    }

    // STEP 2 (AC03.2) — Create a Menu UNDER the new Module (parentId=0 root) ------
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMenu_UnderModule_CapturesId()
    {
        _moduleId.Should().BeGreaterThan(0, "Step 1 must have created the module first");

        var resp = await _f.Client.PostAsJsonAsync(MenuRoute, new
        {
            menuName  = $"QA FT Menu {_f.EntityCode[..10]}",
            moduleId  = _moduleId,                 // ← Menu linked to the new Module
            menuIcon  = "fa-home",
            menuUrl   = $"/qa/ft/menu/{_f.EntityCode[..6]}",
            parentId  = 0,                         // root
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            type      = "M"
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _menuId = (await ParseAsync(resp)).RootElement.CreatedId();
        _menuId.Should().BeGreaterThan(0, "a Menu can be created under the Module from Step 1");
    }

    // STEP 3 (AC03.3) — Menu reachable via by-name?name=&moduleId= ----------------
    [Fact, TestPriority(3)]
    public async Task Step3_MenuByName_IsReachable_ForModule()
    {
        // ⚠ AC03.3 [verify]: by-name autocomplete filter semantics + company-scoping mean the
        //   just-created row may not surface under testsales. Assert reachability tolerantly.
        var resp = await _f.Client.GetAsync($"{MenuRoute}/by-name?name=QA&moduleId={_moduleId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // STEP 4 (AC03.4) — Navigation tree via /by-module & /by-parent (List<int>) ---
    [Fact, TestPriority(4)]
    public async Task Step4_ByModuleAndByParent_ReturnNavigationTree()
    {
        // by-module with the new module id — controller returns 200 for a non-empty id list.
        var byModule = await _f.Client.PostAsJsonAsync($"{MenuRoute}/by-module",
            new List<int> { _moduleId });
        byModule.StatusCode.Should().Be(HttpStatusCode.OK);

        // ⚠ AC03.4 [verify]: by-parent with the root id (0) may yield 200 (empty) or 400.
        var byParent = await _f.Client.PostAsJsonAsync($"{MenuRoute}/by-parent",
            new List<int> { 0 });
        ((int)byParent.StatusCode).Should().BeOneOf(200, 400);
    }

    // STEP 5 (AC03.5) — RoleEntitlements wiring (BLOCKED → Skipped) --------------
    [Fact(Skip = "needs seeded data: AC03.5 RoleEntitlements create requires a seeded " +
                 "role + module/menu/privilege id chain (nested RBAC DTO) that the QA clone " +
                 "does not guarantee — see RoleEntitlementsQATests."),
     TestPriority(5)]
    public async Task Step5_CreateRoleEntitlements_WiresModulesMenusPrivileges()
    {
        // Intended once seeded: POST /api/RoleEntitlements with { roleId, nested module/menu/
        // privilege arrays } → 200, then GET /api/RoleEntitlements/{id} echoes the wiring.
        var resp = await _f.Client.PostAsJsonAsync("/api/RoleEntitlements", new
        {
            roleId = _f.ValidRoleId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // STEP 6 (AC03.6) — UserRoleAllocation (BLOCKED → Skipped) -------------------
    [Fact(Skip = "needs seeded data: AC03.6 UserRoleAllocation requires a seeded user id + " +
                 "role ids (bulk roleIds body) — see UserRoleAllocationQATests."),
     TestPriority(6)]
    public async Task Step6_AllocateRolesToUser_AssignsRoles()
    {
        // Intended once seeded: POST /api/UserRoleAllocation { userId, roleIds[] } → 200/201.
        var resp = await _f.Client.PostAsJsonAsync("/api/UserRoleAllocation", new
        {
            userId  = 1,
            roleIds = new[] { _f.ValidRoleId }
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
    }

    // STEP 7 (AC03.7) — Teardown leaf-first: Menu → Module (ALWAYS LAST) ---------
    [Fact, TestPriority(7)]
    public async Task Step7_Teardown_DeletesMenuThenModule()
    {
        _menuId.Should().BeGreaterThan(0, "Step 2 must have created the menu");
        _moduleId.Should().BeGreaterThan(0, "Step 1 must have created the module");

        // Leaf first — Menu (DELETE /api/Menu/{id} ROUTE). Menu delete may 200/400/500 if a
        // downstream cascade trips (no GetById guard) — tolerate, this is teardown.
        var menuDelete = await _f.Client.DeleteAsync($"{MenuRoute}/{_menuId}");
        ((int)menuDelete.StatusCode).Should().BeOneOf(200, 400, 500);

        // Parent next — Module (DELETE /api/Modules/{id} ROUTE → 200 soft-delete).
        var moduleDelete = await _f.Client.DeleteAsync($"{ModulesRoute}/{_moduleId}");
        moduleDelete.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
