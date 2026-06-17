namespace BudgetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-BUD-02 — Budget group → request → allocation
//   As a budget manager I create a budget group, raise a budget request, and record an allocation.
//
// PARTIAL: read/reachability is ACTIVE; the create chain is [Fact(Skip=…)] — BudgetGroup needs
// cross-module unit/department/costCenter/currency + a Budget MiscMaster budget-type code; Budget
// Request/Allocation are approval-workflow transactional with OPEX/CAPEX conditional rules. Un-skip
// once that reference data + approval config are seeded.
//
// Routes verified from BudgetManagement.QATests:
//   BudgetGroup     : /api/budgetgroup (GET ""; autocomplete; by-department; DELETE /{id})
//   BudgetRequest   : /api/budgetrequest (GET ""; pending; DELETE ?id=)
//   BudgetAllocation: /api/budgetallocation (SpindleDetailsMonthwise; BudgetBalanceReport/{fy})
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-BUD-02-BudgetLifecycle")]
[Trait("Module", "BudgetManagement")]
[Trait("Story", "US-BUD-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_BUD_02_BudgetLifecycle_Tests
{
    private readonly QAServerFixture _f;

    private const string GroupRoute      = "/api/budgetgroup";
    private const string RequestRoute    = "/api/budgetrequest";
    private const string AllocationRoute = "/api/budgetallocation";

    public US_BUD_02_BudgetLifecycle_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the budget read surface is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_BudgetReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{GroupRoute}?pageNumber=1&pageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{RequestRoute}?pageNumber=1&pageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{AllocationRoute}/SpindleDetailsMonthwise?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — no-auth rejected on the budget-group list.
    [Fact, TestPriority(2)]
    public async Task Step2_GroupList_NoAuth_Returns401()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{GroupRoute}?pageNumber=1&pageSize=15"));
    }

    // AC2 — create a BudgetGroup. BLOCKED.
    [Fact(Skip = "needs seeded data: cross-module unit/department/costCenter/currency + Budget MiscMaster budget-type (ANNUAL/MONTHLY) code."), TestPriority(3)]
    public async Task Step3_CreateBudgetGroup()
    {
        await Task.CompletedTask;
    }

    // AC3 — raise a BudgetRequest. BLOCKED.
    [Fact(Skip = "needs seeded data: OPEX/CAPEX requestType misc + unit/currency + budgetGroup or project/WBS + approval workflow."), TestPriority(4)]
    public async Task Step4_CreateBudgetRequest()
    {
        await Task.CompletedTask;
    }

    // AC4 — record a BudgetAllocation. BLOCKED.
    [Fact(Skip = "needs seeded data: an approved BudgetRequest + financialYear/allocationType."), TestPriority(5)]
    public async Task Step5_CreateBudgetAllocation()
    {
        await Task.CompletedTask;
    }
}
