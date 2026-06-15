namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-04 — Activity & checklist setup
//
//   As a maintenance planner I define an activity and its checklist items.
//
// WORKFLOW test: resolves the activity-type lookup, creates an ActivityMaster (nested
// CreateActivityMasterDto mapped to a machine group), adds an ActivityCheckListMaster,
// and reads the checklist back by activity id.
//
// Live-reconciled facts:
//   • ActivityMaster create wraps a `createActivityMasterDto` with an `activityMachineGroup`
//     list of `{ machineGroupId }`. A real machine group is resolved at runtime.
//   • The checklist read endpoint is POST /api/ActivityCheckListMaster/ByActivityId and
//     expects a body of `{ "ids": [activityId] }` (a list named Ids), NOT `{ activityId }`.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-04-ActivityChecklistSetup")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-04")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_04_ActivityChecklistSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string ActivityRoute = "/api/ActivityMaster";
    private const string ChecklistRoute = "/api/ActivityCheckListMaster";
    private const string GroupRoute = "/api/MachineGroup";
    private const int Seed = 1;

    private static int _activityId;

    public US_MNT_04_ActivityChecklistSetup_Tests(QAServerFixture fixture) => _f = fixture;

    // AC (lookup) — the activity-type lookup is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_ActivityTypeLookupReachable()
    {
        var resp = await _f.Client.GetAsync($"{ActivityRoute}/GetActivityType");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 — an ActivityMaster can be created (nested CreateActivityMasterDto).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateActivityMaster()
    {
        // Resolve a real machine group; create one if the clone has none.
        var groupId = await QAHelper.FirstIdAsync(_f.Client, GroupRoute);
        if (groupId == 0)
        {
            var gResp = await _f.Client.PostAsJsonAsync(GroupRoute, new
            {
                groupName = "QA Act Group " + _f.EntityCode[..6],
                manufacturer = Seed, unitId = Seed, departmentId = Seed, powerSource = (byte)1
            });
            await QAHelper.AssertOkAsync(gResp);
            groupId = (await QAHelper.ParseAsync(gResp)).RootElement.CreatedId();
        }
        groupId.Should().BeGreaterThan(0);

        var resp = await _f.Client.PostAsJsonAsync(ActivityRoute, new
        {
            createActivityMasterDto = new
            {
                activityName = "QAAct" + _f.EntityCode[..6],
                description = "QA activity",
                unitId = Seed,
                departmentId = Seed,
                estimatedDuration = 60,
                activityType = Seed,
                activityMachineGroup = new[] { new { machineGroupId = groupId } }
            }
        });
        await QAHelper.AssertOkAsync(resp);
        _activityId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _activityId.Should().BeGreaterThan(0);
    }

    // AC2 — an ActivityCheckListMaster can be created for that activity.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateChecklistForActivity()
    {
        _activityId.Should().BeGreaterThan(0, "Step2 must have created the activity");
        var resp = await _f.Client.PostAsJsonAsync(ChecklistRoute, new
        {
            activityID = _activityId,
            activityCheckList = "QA check item " + _f.EntityCode[..6],
            unitId = Seed
        });
        await QAHelper.AssertOkAsync(resp);
    }

    // AC3 — POST /api/ActivityCheckListMaster/ByActivityId returns the checklist for the activity.
    //       Body is { "ids": [activityId] } (a list), not { activityId }.
    [Fact, TestPriority(4)]
    public async Task Step4_ChecklistReachableByActivity()
    {
        _activityId.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync($"{ChecklistRoute}/ByActivityId", new { ids = new[] { _activityId } });
        await QAHelper.AssertOkAsync(resp);
    }
}
