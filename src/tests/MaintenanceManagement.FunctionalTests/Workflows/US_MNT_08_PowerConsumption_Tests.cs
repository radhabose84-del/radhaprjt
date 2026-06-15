namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-08 — Power consumption tracking
//   As a maintenance user I build the feeder hierarchy and record meter readings.
// PARTIAL: the FeederGroup create is runnable; the Feeder (many FKs) and reading steps
// are Skipped until seeded data exists.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-08-PowerConsumption")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-08")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_08_PowerConsumption_Tests
{
    private readonly QAServerFixture _f;
    private const string FeederGroupRoute = "/api/FeederGroup";
    private const int Seed = 1;
    private static int _feederGroupId;

    public US_MNT_08_PowerConsumption_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a FeederGroup can be created (runnable).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateFeederGroup()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{FeederGroupRoute}/create", new
        {
            feederGroupCode = _f.EntityCode[..10], feederGroupName = "QA Feeder Group", unitId = Seed
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201); // FeederGroup create uses CreatedAtAction → 201
        _feederGroupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _feederGroupId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Needs seeded data: Feeder create has many FKs (feeder type/meter/unit). Author during live reconciliation."), TestPriority(2)]
    public Task Step2_CreateFeederUnderGroup() => Task.CompletedTask;

    [Fact(Skip = "Needs the created feeder id: record a PowerConsumption reading."), TestPriority(3)]
    public Task Step3_RecordReading() => Task.CompletedTask;

    [Fact(Skip = "Needs the created feeder id: GET /api/PowerConsumption/GetOpeningReaderValue/{feederId} returns the opening reading."), TestPriority(4)]
    public Task Step4_OpeningReadingReachable() => Task.CompletedTask;

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_feederGroupId > 0) await _f.Client.DeleteAsync($"{FeederGroupRoute}/{_feederGroupId}");
    }
}
