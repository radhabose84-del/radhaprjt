namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-05 — Preventive maintenance scheduling
//
//   As a maintenance planner I configure a preventive schedule for a machine and an
//   activity, map machines to it, and have work orders generated.
//
// Live-reconciled status:
//   • Prerequisites (machine under a group + activity) are now creatable — Step1 builds them.
//   • The scheduler ABSTRACT read endpoint is reachable — Step4.
//   • BLOCKED: CreatePreventiveScheduler calls GetMachineByGroupSagaAsync(groupId, UnitId)
//     where UnitId is the CALLER's JWT unit. The QA user `testsales` has UnitId = 0, but
//     machines require UnitId >= 1 (Machine validator), so the saga query never matches and
//     the handler throws "No machines found for selected MachineGroup." This needs a QA user
//     whose UnitId actually owns machines — an environment/seed limitation, not a payload gap.
//     Steps 2–3 therefore remain skipped with that precise reason.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-05-PreventiveScheduling")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-05")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_05_PreventiveScheduling_Tests
{
    private readonly QAServerFixture _f;

    private const string GroupRoute = "/api/MachineGroup";
    private const string ShiftRoute = "/api/ShiftMaster";
    private const string CostCenterRoute = "/api/CostCenter";
    private const string WorkCenterRoute = "/api/WorkCenter";
    private const string MachineRoute = "/api/Machine";
    private const string ActivityRoute = "/api/ActivityMaster";
    private const string SchedulerRoute = "/api/PreventiveScheduler";
    private const int Seed = 1;

    private static int _activityId;

    public US_MNT_05_PreventiveScheduling_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code(string p) => p + _f.EntityCode[..6];

    // AC1 — a machine (under a group) and an activity exist.
    [Fact, TestPriority(1)]
    public async Task Step1_MachineAndActivityExist()
    {
        var gResp = await _f.Client.PostAsJsonAsync(GroupRoute, new
        {
            groupName = "QA PS Group " + _f.EntityCode[..6],
            manufacturer = Seed, unitId = Seed, departmentId = Seed, powerSource = (byte)1
        });
        await QAHelper.AssertOkAsync(gResp);
        var groupId = (await QAHelper.ParseAsync(gResp)).RootElement.CreatedId();

        var ccResp = await _f.Client.PostAsJsonAsync(CostCenterRoute, new
        {
            costCenterCode = Code("CC"), costCenterName = "QA CC " + _f.EntityCode[..6], unitId = Seed, departmentId = Seed,
            effectiveDate = "2026-01-01T00:00:00+00:00", responsiblePerson = "QA", budgetAllocated = 1000.0, remarks = "PS"
        });
        await QAHelper.AssertOkAsync(ccResp);
        var ccId = (await QAHelper.ParseAsync(ccResp)).RootElement.CreatedId();

        var wcResp = await _f.Client.PostAsJsonAsync(WorkCenterRoute, new
        {
            workCenterCode = Code("WC"), workCenterName = "QA WC " + _f.EntityCode[..6], unitId = Seed, departmentId = Seed
        });
        await QAHelper.AssertOkAsync(wcResp);
        var wcId = (await QAHelper.ParseAsync(wcResp)).RootElement.CreatedId();
        var shiftId = await QAHelper.FirstIdAsync(_f.Client, ShiftRoute);
        if (shiftId == 0)
        {
            shiftId = (await QAHelper.ParseAsync(await _f.Client.PostAsJsonAsync(ShiftRoute, new
            {
                shiftCode = Code("SH"), shiftName = "QA Shift", effectiveDate = "2026-01-01"
            }))).RootElement.CreatedId();
        }

        var mResp = await _f.Client.PostAsJsonAsync(MachineRoute, new
        {
            machineCode = Code("MC"), machineName = "QA Machine " + _f.EntityCode[..6],
            machineGroupId = groupId, unitId = Seed, productionCapacity = 10.0, uomId = Seed,
            shiftMasterId = shiftId, costCenterId = ccId, workCenterId = wcId,
            installationDate = "2026-01-01T00:00:00+00:00", assetId = Seed, lineNo = Seed, isProductionMachine = (byte)1
        });
        await QAHelper.AssertOkAsync(mResp);
        (await QAHelper.ParseAsync(mResp)).RootElement.CreatedId().Should().BeGreaterThan(0);

        var aResp = await _f.Client.PostAsJsonAsync(ActivityRoute, new
        {
            createActivityMasterDto = new
            {
                activityName = "QAPS" + _f.EntityCode[..6], description = "QA", unitId = Seed,
                departmentId = Seed, estimatedDuration = 60, activityType = Seed,
                activityMachineGroup = new[] { new { machineGroupId = groupId } }
            }
        });
        await QAHelper.AssertOkAsync(aResp);
        _activityId = (await QAHelper.ParseAsync(aResp)).RootElement.CreatedId();
        _activityId.Should().BeGreaterThan(0);
    }

    // AC2 — create a PreventiveScheduler.
    // BLOCKED: the handler matches machines by the caller's JWT UnitId; testsales has UnitId=0
    // while machines require UnitId>=1, so no machine ever matches. Needs a unit-owning QA user.
    [Fact(Skip = "Blocked (environment): CreatePreventiveScheduler matches machines by the caller's JWT UnitId via GetMachineByGroupSagaAsync; testsales has UnitId=0 but machines require UnitId>=1, so it always throws 'No machines found for selected MachineGroup'. Needs a QA user whose UnitId owns machines."), TestPriority(2)]
    public Task Step2_CreateScheduler() => Task.CompletedTask;

    // AC3 — map machines to the schedule.
    [Fact(Skip = "Needs a created scheduler id (blocked by Step2)."), TestPriority(3)]
    public Task Step3_MapMachines() => Task.CompletedTask;

    // AC4 — the scheduler abstract endpoint is reachable by date range.
    [Fact, TestPriority(4)]
    public async Task Step4_AbstractReachableByDate()
    {
        var resp = await _f.Client.GetAsync($"{SchedulerRoute}/SchedulerAbstractByDate?FromDate=2030-01-01&ToDate=2030-12-31");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
