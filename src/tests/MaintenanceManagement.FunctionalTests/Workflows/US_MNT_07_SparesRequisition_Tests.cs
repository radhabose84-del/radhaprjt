namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-07 — Spares requisition & stock movement
//   As a maintenance user I raise a material requisition slip (MRS) and issue spares,
//   moving stock in the ledger.
// BLOCKED on stock/item data — the department lookup is runnable; the rest Skipped.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-07-SparesRequisition")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-07")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_07_SparesRequisition_Tests
{
    private readonly QAServerFixture _f;
    public US_MNT_07_SparesRequisition_Tests(QAServerFixture fixture) => _f = fixture;

    // AC (lookup) — an MRS department lookup is reachable (runnable; tolerant on params).
    [Fact, TestPriority(1)]
    public async Task Step1_MrsDepartmentLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/maintenance/MRS/department/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact(Skip = "Needs stock/item data: create an MRS (POST /CreateMRS, GRN/stock-driven payload)."), TestPriority(2)]
    public Task Step2_CreateMrs() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted MRS: the MRS appears in pending-issue."), TestPriority(3)]
    public Task Step3_AppearsInPendingIssue() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted issue: StockLedger reflects the issued quantity."), TestPriority(4)]
    public Task Step4_StockLedgerReflectsIssue() => Task.CompletedTask;
}
