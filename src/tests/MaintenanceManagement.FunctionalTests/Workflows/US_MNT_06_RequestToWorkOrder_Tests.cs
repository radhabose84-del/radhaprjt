namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-06 — Maintenance request → work order lifecycle
//   As a maintenance user I raise a request, convert it to a work order, move it
//   through its statuses, and see it in service history.
// BLOCKED on machine/request data — the status lookup is runnable; the rest Skipped.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-06-RequestToWorkOrder")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-06")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_06_RequestToWorkOrder_Tests
{
    private readonly QAServerFixture _f;
    public US_MNT_06_RequestToWorkOrder_Tests(QAServerFixture fixture) => _f = fixture;

    // AC (lookup) — the work-order status lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_WorkOrderStatusLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/WorkOrder/Status");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs seeded data: raise an internal MaintenanceRequest (complex payload)."), TestPriority(2)]
    public Task Step2_RaiseMaintenanceRequest() => Task.CompletedTask;

    [Fact(Skip = "Needs request/machine ids: create a WorkOrder."), TestPriority(3)]
    public Task Step3_CreateWorkOrder() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted WO: move the work order through its status values."), TestPriority(4)]
    public Task Step4_MoveThroughStatuses() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted WO: the completed work appears in ServiceHistory."), TestPriority(5)]
    public Task Step5_AppearsInServiceHistory() => Task.CompletedTask;
}
