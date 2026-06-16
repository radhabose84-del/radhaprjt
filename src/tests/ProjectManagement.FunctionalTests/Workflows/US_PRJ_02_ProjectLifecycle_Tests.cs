namespace ProjectManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PRJ-02 — Project → WBS lifecycle
//   As a project manager I create a project and build its work-breakdown structure.
//
// PARTIAL: read/reachability is ACTIVE; the create chain is [Fact(Skip=…)] — ProjectMaster
// create needs a broad cross-module FK chain (type/category/unit/dept/budgetYear/costCenter/
// currency/assetGroup) + an approval-workflow config not resolvable on the clone, and WBS needs
// a ProjectMaster id. Un-skip once that reference data + workflow config are seeded.
//
// Routes verified from ProjectManagement.QATests:
//   ProjectMaster: /api/ProjectMaster (GET ""; GET ProjectByname; GET pending-approvals; DELETE /{id})
//   WBS          : /api/ProjectWorkBreakdownStructure (GET ""; by-project; ProjectWbsParentLookup; DELETE /{id})
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PRJ-02-ProjectLifecycle")]
[Trait("Module", "ProjectManagement")]
[Trait("Story", "US-PRJ-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PRJ_02_ProjectLifecycle_Tests
{
    private readonly QAServerFixture _f;

    private const string ProjectRoute = "/api/ProjectMaster";
    private const string WbsRoute      = "/api/ProjectWorkBreakdownStructure";

    private static int _projectId;

    public US_PRJ_02_ProjectLifecycle_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the project read surface is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_ProjectReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{ProjectRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{ProjectRoute}/ProjectByname?searchTerm=QA")).StatusCode).Should().BeOneOf(200, 400, 404);
        ((int)(await _f.Client.GetAsync($"{ProjectRoute}/pending-approvals?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — WBS read surface is reachable + no-auth rejected.
    [Fact, TestPriority(2)]
    public async Task Step2_WbsReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{WbsRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{ProjectRoute}?PageNumber=1&PageSize=15"));
    }

    // AC2 — create a ProjectMaster. BLOCKED.
    [Fact(Skip = "needs seeded data: ProjectMaster cross-module FK chain (projectType/category/unit/department/budgetYear/costCenter/currency/assetGroup) + approval-workflow config not resolvable on BannariERP_QATest."), TestPriority(3)]
    public async Task Step3_CreateProject()
    {
        await Task.CompletedTask;
    }

    // AC3 — create a WBS node under the project (depends on Step3).
    [Fact(Skip = "needs seeded data: depends on a ProjectMaster id (Step3 blocked)."), TestPriority(4)]
    public async Task Step4_CreateWbs_UnderProject()
    {
        if (_projectId == 0) return;
        await Task.CompletedTask;
    }
}
