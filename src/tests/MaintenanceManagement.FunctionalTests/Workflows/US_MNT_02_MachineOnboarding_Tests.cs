namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-02 — Machine onboarding
//   As a maintenance administrator I onboard a machine under a group and record its spec.
// PARTIAL: the group prerequisite is runnable; the Machine create (complex payload) and
// MachineSpecification steps are Skipped until seeded data exists.
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
    private const int Seed = 1;
    private static int _groupId;

    public US_MNT_02_MachineOnboarding_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a MachineGroup exists (runnable prerequisite).
    [Fact, TestPriority(1)]
    public async Task Step1_MachineGroupExists()
    {
        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new
        {
            groupName = "QA Onboarding Group " + _f.EntityCode[..6], manufacturer = Seed, unitId = Seed, departmentId = Seed, powerSource = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Needs seeded data: Machine create has a complex payload (group/line/asset/specs). Author during live reconciliation."), TestPriority(2)]
    public Task Step2_CreateMachineUnderGroup() => Task.CompletedTask;

    [Fact(Skip = "Needs the created machine id: record a MachineSpecification for the machine."), TestPriority(3)]
    public Task Step3_RecordMachineSpecification() => Task.CompletedTask;

    [Fact(Skip = "Needs the created machine id: GET /api/Machine/MachineGroup/{groupId} returns the machine."), TestPriority(4)]
    public Task Step4_MachineReachableByGroup() => Task.CompletedTask;

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_groupId > 0) await _f.Client.DeleteAsync($"{GroupRoute}/{_groupId}");
    }
}
