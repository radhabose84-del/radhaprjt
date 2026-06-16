namespace GateEntryManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GE-02 — Vehicle movement → gate documents
//   As a security/gate user I record a vehicle movement, raise an inward receipt, and issue a
//   gate pass.
//
// PARTIAL: read/reachability is ACTIVE; the create chain is [Fact(Skip=…)] — the gate documents
// auto-number via Finance TransactionType + DocumentSequence ("Gate Entry"/"Gate Inward"/
// "Gate Outward") not seeded for the GateEntry module, and GateInward needs a warehouse + PO/GRN
// bridge. Un-skip once that numbering + upstream data are seeded.
//
// Routes verified from GateEntryManagement.QATests:
//   VMR        : /api/gateentry/vehiclemovementrecord (GET ""; pending; DELETE ?id=)
//   GateInward : /api/gateentry/gateinward (GET ""; pending-reference-docs; DELETE ?id=)
//   GatePass   : /api/gateentry/gatepass (GET ""; doc-types; DELETE ?id=)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GE-02-GateMovement")]
[Trait("Module", "GateEntryManagement")]
[Trait("Story", "US-GE-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GE_02_GateMovement_Tests
{
    private readonly QAServerFixture _f;

    private const string VmrRoute     = "/api/gateentry/vehiclemovementrecord";
    private const string InwardRoute  = "/api/gateentry/gateinward";
    private const string GatePassRoute = "/api/gateentry/gatepass";

    private static int _vmrId;

    public US_GE_02_GateMovement_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the gate document read surface is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_GateReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{VmrRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{VmrRoute}/pending")).StatusCode).Should().BeOneOf(200, 400, 404);
        ((int)(await _f.Client.GetAsync($"{InwardRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{GatePassRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — no-auth rejected on the VMR list.
    [Fact, TestPriority(2)]
    public async Task Step2_VmrList_NoAuth_Returns401()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{VmrRoute}?PageNumber=1&PageSize=15"));
    }

    // AC2 — create a VehicleMovementRecord. BLOCKED.
    [Fact(Skip = "needs seeded data: GateEntry document-numbering (TransactionType 'Gate Entry' + DocumentSequence) not seeded for the GateEntry module + unitId/purposeOfVisitId."), TestPriority(3)]
    public async Task Step3_CreateVmr()
    {
        await Task.CompletedTask;
    }

    // AC3 — create a GateInward against the VMR. BLOCKED.
    [Fact(Skip = "needs seeded data: 'Gate Inward' numbering + receivingWarehouse + PO/GRN bridge; depends on a VMR."), TestPriority(4)]
    public async Task Step4_CreateGateInward()
    {
        if (_vmrId == 0) return;
        await Task.CompletedTask;
    }

    // AC4 — issue a GatePass (flips VMR → OUT). BLOCKED.
    [Fact(Skip = "needs seeded data: 'Gate Outward' numbering + a VehicleMovementRecord id."), TestPriority(5)]
    public async Task Step5_IssueGatePass()
    {
        if (_vmrId == 0) return;
        await Task.CompletedTask;
    }
}
