namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-02 — Machine onboarding
//
//   As a maintenance administrator I onboard a machine under a group and record its
//   specification.
//
// WORKFLOW test: builds the prerequisites (machine group + cost/work centre + shift),
// creates the Machine, records a MachineSpecification, and reads the machine back via
// its group — the full onboarding chain.
//
// Live-reconciled payload facts (verified against the running API):
//   • Machine create requires 11 FK/required fields; the validator only enforces id >= 1,
//     and the DB accepts uom/asset/unit/line as best-effort = 1. CostCenter & WorkCenter
//     are empty in the QA clone, so they are created in-flow; ShiftMaster is resolved at
//     runtime (created in-flow if none exist).
//   • MachineSpecification.SpecificationValue must be a NUMERIC string > 0 (validator quirk).
//   • Machine delete is DELETE /api/Machine?id={id}; GET /api/Machine/MachineGroup/{id}
//     returns the group's machine/department info.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-02-MachineOnboarding")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_02_MachineOnboarding_Tests
{
    private readonly QAServerFixture _f;

    private const string GroupRoute = "/api/MachineGroup";
    private const string ShiftRoute = "/api/ShiftMaster";
    private const string CostCenterRoute = "/api/CostCenter";
    private const string WorkCenterRoute = "/api/WorkCenter";
    private const string MachineRoute = "/api/Machine";
    private const string SpecRoute = "/api/MachineSpecification";
    private const int Seed = 1; // best-effort FK ids (unit / uom / asset / line)

    private static int _groupId;
    private static int _machineId;

    public US_MNT_02_MachineOnboarding_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code(string prefix) => prefix + _f.EntityCode[..6];

    // AC1 — a MachineGroup exists (created in-flow).
    [Fact, TestPriority(1)]
    public async Task Step1_MachineGroupExists()
    {
        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new
        {
            groupName = "QA Onboarding Group " + _f.EntityCode[..6],
            manufacturer = Seed, unitId = Seed, departmentId = Seed, powerSource = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    // AC2 — a Machine can be created under that group (with its prerequisite masters).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMachineUnderGroup()
    {
        _groupId.Should().BeGreaterThan(0, "Step1 must have created the group");

        // CostCenter & WorkCenter are empty in the QA clone — create them in-flow.
        var ccResp = await _f.Client.PostAsJsonAsync(CostCenterRoute, new
        {
            costCenterCode = Code("CC"), costCenterName = "QA CC " + _f.EntityCode[..6],
            unitId = Seed, departmentId = Seed, effectiveDate = "2026-01-01T00:00:00+00:00",
            responsiblePerson = "QA", budgetAllocated = 1000.0, remarks = "USMNT02"
        });
        await QAHelper.AssertOkAsync(ccResp);
        var costCenterId = (await QAHelper.ParseAsync(ccResp)).RootElement.CreatedId();

        var wcResp = await _f.Client.PostAsJsonAsync(WorkCenterRoute, new
        {
            workCenterCode = Code("WC"), workCenterName = "QA WC " + _f.EntityCode[..6],
            unitId = Seed, departmentId = Seed
        });
        await QAHelper.AssertOkAsync(wcResp);
        var workCenterId = (await QAHelper.ParseAsync(wcResp)).RootElement.CreatedId();

        // ShiftMaster — resolve a real one; create one if the clone has none.
        var shiftMasterId = await QAHelper.FirstIdAsync(_f.Client, ShiftRoute);
        if (shiftMasterId == 0)
        {
            var shResp = await _f.Client.PostAsJsonAsync(ShiftRoute, new
            {
                shiftCode = Code("SH"), shiftName = "QA Shift " + _f.EntityCode[..6], effectiveDate = "2026-01-01"
            });
            await QAHelper.AssertOkAsync(shResp);
            shiftMasterId = (await QAHelper.ParseAsync(shResp)).RootElement.CreatedId();
        }
        shiftMasterId.Should().BeGreaterThan(0);

        var resp = await _f.Client.PostAsJsonAsync(MachineRoute, new
        {
            machineCode = Code("MC"),
            machineName = "QA Machine " + _f.EntityCode[..6],
            machineGroupId = _groupId,
            unitId = Seed,
            productionCapacity = 10.0,
            uomId = Seed,
            shiftMasterId,
            costCenterId,
            workCenterId,
            installationDate = "2026-01-01T00:00:00+00:00",
            assetId = Seed,
            lineNo = Seed,
            isProductionMachine = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
        _machineId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _machineId.Should().BeGreaterThan(0);
    }

    // AC3 — a MachineSpecification can be recorded for the machine.
    //       SpecificationValue must be a numeric string > 0 (validator quirk).
    [Fact, TestPriority(3)]
    public async Task Step3_RecordMachineSpecification()
    {
        _machineId.Should().BeGreaterThan(0, "Step2 must have created the machine");
        var resp = await _f.Client.PostAsJsonAsync(SpecRoute, new
        {
            specifications = new[]
            {
                new { specificationId = Seed, specificationValue = "5", machineId = _machineId }
            }
        });
        await QAHelper.AssertOkAsync(resp);
    }

    // AC4 — GET /api/Machine/MachineGroup/{groupId} is reachable for the group.
    [Fact, TestPriority(4)]
    public async Task Step4_MachineReachableByGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{MachineRoute}/MachineGroup/{_groupId}"));
    }

    // AC5 — teardown (machine first, then group).
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_machineId > 0) await _f.Client.DeleteAsync($"{MachineRoute}?id={_machineId}");
        if (_groupId > 0) await _f.Client.DeleteAsync($"{GroupRoute}/{_groupId}");
    }
}
