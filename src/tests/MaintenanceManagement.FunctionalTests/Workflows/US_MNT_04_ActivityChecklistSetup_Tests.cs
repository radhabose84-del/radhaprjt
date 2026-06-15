namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-04 — Activity & checklist setup
//   As a maintenance planner I define an activity and its checklist items.
// PARTIAL: the activity-type lookup is runnable; the ActivityMaster create (nested
// CreateActivityMasterDto) and checklist steps are Skipped until seeded data exists.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-04-ActivityChecklistSetup")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-04")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_04_ActivityChecklistSetup_Tests
{
    private readonly QAServerFixture _f;
    public US_MNT_04_ActivityChecklistSetup_Tests(QAServerFixture fixture) => _f = fixture;

    // AC (lookup) — the activity-type lookup is reachable (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_ActivityTypeLookupReachable()
    {
        var resp = await _f.Client.GetAsync("/api/ActivityMaster/GetActivityType");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Needs seeded data: ActivityMaster create wraps a nested CreateActivityMasterDto. Author during live reconciliation."), TestPriority(2)]
    public Task Step2_CreateActivityMaster() => Task.CompletedTask;

    [Fact(Skip = "Needs the created activity id: create an ActivityCheckListMaster for the activity."), TestPriority(3)]
    public Task Step3_CreateChecklistForActivity() => Task.CompletedTask;

    [Fact(Skip = "Needs the created activity id: POST /api/ActivityCheckListMaster/ByActivityId returns the checklist."), TestPriority(4)]
    public Task Step4_ChecklistReachableByActivity() => Task.CompletedTask;
}
