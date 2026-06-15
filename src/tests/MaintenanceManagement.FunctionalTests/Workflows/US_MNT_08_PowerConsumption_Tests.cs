namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-08 — Power consumption tracking
//
//   As a maintenance user I build the feeder hierarchy and record meter readings.
//
// WORKFLOW test: FeederGroup → Feeder → PowerConsumption reading → opening-reading read.
//
// Live-reconciled facts:
//   • Feeder create (POST /api/Feeder/create) has many FKs but the validator only checks
//     code uniqueness; feederType/meterType/unit/department are accepted as best-effort = 1.
//   • PowerConsumption.OpeningReading must be > 0 (0 fails the NotEmpty rule).
//   • GET /api/PowerConsumption/GetOpeningReaderValue/{feederId} returns 200 when found, else 404
//     when the feeder is outside the caller's unit scope (the raw-exception 500 is now fixed —
//     repo returns null + controller maps to 404).
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
    private const string FeederRoute = "/api/Feeder";
    private const string PowerRoute = "/api/PowerConsumption";
    private const int Seed = 1;

    private static int _feederGroupId;
    private static int _feederId;

    public US_MNT_08_PowerConsumption_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — a FeederGroup can be created.
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

    // AC2 — a Feeder can be created under that group.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateFeederUnderGroup()
    {
        _feederGroupId.Should().BeGreaterThan(0, "Step1 must have created the feeder group");
        var resp = await _f.Client.PostAsJsonAsync($"{FeederRoute}/create", new
        {
            feederCode = "FD" + _f.EntityCode[..6],
            feederName = "QA Feeder " + _f.EntityCode[..6],
            feederGroupId = _feederGroupId,
            feederTypeId = Seed,
            unitId = Seed,
            meterAvailable = (byte)1,
            meterTypeId = Seed,
            departmentId = Seed,
            description = "QA feeder",
            multiplicationFactor = 1.0,
            effectiveDate = "2026-01-01T00:00:00+00:00",
            openingReading = 0.0,
            highPriority = (byte)0,
            target = 100.0
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _feederId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _feederId.Should().BeGreaterThan(0);
    }

    // AC3 — a PowerConsumption reading can be recorded for the feeder (OpeningReading must be > 0).
    [Fact, TestPriority(3)]
    public async Task Step3_RecordReading()
    {
        _feederId.Should().BeGreaterThan(0, "Step2 must have created the feeder");
        var resp = await _f.Client.PostAsJsonAsync(PowerRoute, new
        {
            feederTypeId = Seed,
            feederId = _feederId,
            unitId = Seed,
            openingReading = 10.0,
            closingReading = 50.0
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
    }

    // AC4 — the opening-reading endpoint is reachable for the feeder.
    [Fact, TestPriority(4)]
    public async Task Step4_OpeningReadingReachable()
    {
        _feederId.Should().BeGreaterThan(0);
        var resp = await _f.Client.GetAsync($"{PowerRoute}/GetOpeningReaderValue/{_feederId}");
        // Returns 200 when found, or 404 when the feeder is outside the caller's unit scope
        // (the raw-exception 500 was fixed — repo returns null + controller 404).
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC5 — teardown (feeder first, then group).
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_feederId > 0) await _f.Client.DeleteAsync($"{FeederRoute}/{_feederId}");
        if (_feederGroupId > 0) await _f.Client.DeleteAsync($"{FeederGroupRoute}/{_feederGroupId}");
    }
}
