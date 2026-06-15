namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-05 — Preventive maintenance scheduling
//   As a maintenance planner I configure a preventive schedule for a machine and an
//   activity, map machines to it, and have work orders generated.
// BLOCKED on machine/activity data — all steps Skipped until US-MNT-02/04 produce ids.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-05-PreventiveScheduling")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-05")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_05_PreventiveScheduling_Tests
{
    private readonly QAServerFixture _f;
    public US_MNT_05_PreventiveScheduling_Tests(QAServerFixture fixture) => _f = fixture;

    [Fact(Skip = "Depends on US-MNT-02/04: a machine and an activity must exist."), TestPriority(1)]
    public Task Step1_MachineAndActivityExist() => Task.CompletedTask;

    [Fact(Skip = "Needs seeded data: create a PreventiveScheduler (machine + activity + frequency)."), TestPriority(2)]
    public Task Step2_CreateScheduler() => Task.CompletedTask;

    [Fact(Skip = "Needs the created schedule id: map machines to the schedule (MapMachines)."), TestPriority(3)]
    public Task Step3_MapMachines() => Task.CompletedTask;

    [Fact(Skip = "Needs a posted schedule: the scheduler abstract reflects the schedule by date."), TestPriority(4)]
    public Task Step4_AbstractReflectsSchedule() => Task.CompletedTask;
}
