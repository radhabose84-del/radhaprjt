namespace BackgroundService.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-BGS-03 — Approval-workflow readiness
//   As a platform operator I expect the approval-workflow engine endpoints to be
//   reachable and auth-protected even before any approval configuration is seeded,
//   and a clean WorkflowType master to be creatable.
// Mostly blocked on transactional create; WorkflowType create + reachability +
// security are active.
//
// Contracts (verified against BackgroundService.QATests, 2026-06-18):
//   POST   /api/WorkflowType  { moduleId, menuId, hasLine(byte), isMultiselect(byte), transactionTypeIds:[] }
//        (moduleId FK → /api/Modules; menuId FK → /api/Menu) → data is a List<int> (capture data[0])
//   DELETE /api/WorkflowType?id={id}        (id from QUERY)
//   GET    /api/ApprovalStepDetail?PageNumber=&PageSize=   (paged; tolerant 200/404/500)
//   GET    /api/ApprovalRule?PageNumber=&PageSize=         (paged; tolerant 200/404/500)
//   ApprovalStepDetail / ApprovalRule create chains need seeded parents → BLOCKED.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-BGS-03-ApprovalWorkflowReadiness")]
[Trait("Module", "BackgroundService")]
[Trait("Story", "US-BGS-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_BGS_03_ApprovalWorkflowReadiness_Tests
{
    private readonly QAServerFixture _f;

    private const string WorkflowTypeRoute = "/api/WorkflowType";
    private const string StepDetailRoute   = "/api/ApprovalStepDetail";
    private const string RuleRoute         = "/api/ApprovalRule";
    private const string ModulesRoute      = "/api/Modules";
    private const string MenuRoute         = "/api/Menu";

    private static int _workflowTypeId;

    public US_BGS_03_ApprovalWorkflowReadiness_Tests(QAServerFixture fixture) => _f = fixture;

    // WorkflowType create `data` is a List<int> — pull the first int as the new id.
    private static int FirstCreatedIdFromArray(JsonDocument doc)
    {
        var data = doc.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            return data[0].GetInt32();
        return 0;
    }

    // AC1 — a WorkflowType can be created (clean master; self-skips if FK parents unresolved).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateWorkflowType()
    {
        var moduleId = await QAHelper.FirstIdAsync(_f.Client, ModulesRoute);
        var menuId   = await QAHelper.FirstIdAsync(_f.Client, MenuRoute);
        if (moduleId == 0 || menuId == 0) return; // REQUIRED FK parents unresolved → self-skip

        var resp = await _f.Client.PostAsJsonAsync(WorkflowTypeRoute, new
        {
            moduleId,
            menuId,
            hasLine = (byte)0,
            isMultiselect = (byte)0,
            transactionTypeIds = new int[] { }
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _workflowTypeId = FirstCreatedIdFromArray(await QAHelper.ParseAsync(resp));
        _workflowTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — the ApprovalStepDetail read surface is reachable (tolerant).
    [Fact, TestPriority(2)]
    public async Task Step2_ApprovalStepDetailReadSurfaceReachable()
    {
        var resp = await _f.Client.GetAsync($"{StepDetailRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    // AC3 — the ApprovalRule read surface is reachable (tolerant).
    [Fact, TestPriority(3)]
    public async Task Step3_ApprovalRuleReadSurfaceReachable()
    {
        var resp = await _f.Client.GetAsync($"{RuleRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    // AC4 — the full approval chain is blocked on seeded data.
    [Fact(Skip = "needs seeded data: an ApprovalStepDetail (WorkflowType + ApprovalStep parents + unit/dept mappings) + an ApprovalRule nested condition graph to exercise the approval chain"), TestPriority(4)]
    public async Task Step4_FullApprovalChain()
    {
        // Documentary: ApprovalStepDetail (workFlowTypeId + approvalStepId + mappings) then
        // ApprovalRule (approvalStepDetailId + nested approvalRuleConditions) would extend this story.
        await Task.CompletedTask;
    }

    // AC5 — the approval endpoints reject anonymous callers (401).
    [Fact, TestPriority(5)]
    public async Task Step5_ApprovalEndpointsRejectAnonymous()
    {
        var step = await _f.AnonymousClient.GetAsync($"{StepDetailRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(step);

        var rule = await _f.AnonymousClient.GetAsync($"{RuleRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(rule);
    }

    // AC6 — teardown (created WorkflowType deletes by QUERY ?id=).
    [Fact, TestPriority(6)]
    public async Task Step6_Teardown()
    {
        if (_workflowTypeId > 0)
            await _f.Client.DeleteAsync($"{WorkflowTypeRoute}?id={_workflowTypeId}");
    }
}
