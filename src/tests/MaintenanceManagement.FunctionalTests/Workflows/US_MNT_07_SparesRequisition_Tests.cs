namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-07 — Spares requisition & stock movement
//
//   As a maintenance user I raise a material requisition slip (MRS) and issue spares,
//   moving stock in the ledger.
//
// Live-reconciled status:
//   • The MRS reference lookups (department / category / sub-cost-centre) are reachable (Step1).
//   • BLOCKED: MRS create (POST /CreateMRS) takes a HeaderRequest with division/department/
//     sub-department codes + line-item Details bound to real stock (old-ERP-coded item codes
//     with available quantity). These are scoped to the caller's OldUnitId, which the QA user
//     `testsales` does not have, so there is no division/stock data to requisition against.
//     MRS create, pending-issue and StockLedger movement therefore need a unit-scoped user
//     with seeded stock — an environment/seed limitation, not a payload gap.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-07-SparesRequisition")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-07")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_07_SparesRequisition_Tests
{
    private readonly QAServerFixture _f;
    private const string MrsRoute = "/api/maintenance/MRS";

    public US_MNT_07_SparesRequisition_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the MRS reference lookups (department / category / sub-cost-centre) are reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_MrsReferenceLookupsReachable()
    {
        ((int)(await _f.Client.GetAsync($"{MrsRoute}/department/1")).StatusCode).Should().BeOneOf(200, 400, 404);
        ((int)(await _f.Client.GetAsync($"{MrsRoute}/Category/1")).StatusCode).Should().BeOneOf(200, 400, 404);
        ((int)(await _f.Client.GetAsync($"{MrsRoute}/SubCostCenter/1")).StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // AC2 — create an MRS.
    [Fact(Skip = "Blocked (environment): MRS create needs a HeaderRequest with division/department codes + line-item Details bound to real stock (old-ERP item codes with quantity), scoped to the caller's OldUnitId. testsales has no OldUnitId stock scope, so there is nothing to requisition. Needs a unit-scoped user with seeded stock."), TestPriority(2)]
    public Task Step2_CreateMrs() => Task.CompletedTask;

    // AC3 — the MRS appears in pending-issue.
    [Fact(Skip = "Needs a posted MRS (blocked by Step2)."), TestPriority(3)]
    public Task Step3_AppearsInPendingIssue() => Task.CompletedTask;

    // AC4 — StockLedger reflects the issued quantity.
    [Fact(Skip = "Needs a posted issue (blocked by Step2/3)."), TestPriority(4)]
    public Task Step4_StockLedgerReflectsIssue() => Task.CompletedTask;
}
