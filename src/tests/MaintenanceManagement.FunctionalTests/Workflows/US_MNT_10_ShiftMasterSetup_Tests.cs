namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-10 — Shift master setup (header → detail)
//
//   As a maintenance administrator I define a shift (header) and then add its timing
//   detail (start/end/break per unit) so machine work and schedules can be planned by
//   shift.
//
// WORKFLOW test: chains ShiftMaster → ShiftMasterDetail (FK ShiftMasterId) and reads the
// header back — the header→detail link the per-entity CRUD tests do NOT cover. FK ids
// (unit/supervisor) are best-effort = 1. Fully runnable (no seeded data needed).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-10-ShiftMasterSetup")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-10")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_10_ShiftMasterSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string ShiftRoute = "/api/ShiftMaster";
    private const string ShiftDetailRoute = "/api/ShiftMasterDetail";
    private const int Seed = 1; // best-effort FK ids (unit / supervisor)

    private static int _shiftId;
    private static int _shiftDetailId;

    public US_MNT_10_ShiftMasterSetup_Tests(QAServerFixture fixture) => _f = fixture;

    private string ShiftCode() => "SH" + _f.EntityCode[..6];

    // AC1 — a ShiftMaster (the header) can be created and returns a new id.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateShiftMaster()
    {
        var resp = await _f.Client.PostAsJsonAsync(ShiftRoute, new
        {
            shiftCode = ShiftCode(),
            shiftName = "QA Shift " + _f.EntityCode[..6],
            effectiveDate = "2026-01-01"
        });
        await QAHelper.AssertOkAsync(resp);
        _shiftId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _shiftId.Should().BeGreaterThan(0);
    }

    // AC2 — a ShiftMasterDetail (timing) can be created under that shift.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateShiftDetailUnderShift()
    {
        _shiftId.Should().BeGreaterThan(0, "Step1 must have created the shift master");
        var resp = await _f.Client.PostAsJsonAsync(ShiftDetailRoute, new
        {
            shiftMasterId = _shiftId,
            unitId = Seed,
            startTime = "08:00:00",
            endTime = "16:00:00",
            breakDurationInMinutes = 30,
            effectiveDate = "2026-01-01",
            shiftSupervisorId = Seed
        });
        await QAHelper.AssertOkAsync(resp);
        _shiftDetailId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _shiftDetailId.Should().BeGreaterThan(0);
    }

    // AC3 — the shift header is readable by id after creation.
    [Fact, TestPriority(3)]
    public async Task Step3_ShiftMasterReadableById()
    {
        _shiftId.Should().BeGreaterThan(0);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ShiftRoute}/{_shiftId}"));
    }

    // AC4 — teardown (detail first, then header — respect the FK order).
    [Fact, TestPriority(4)]
    public async Task Step4_Teardown()
    {
        if (_shiftDetailId > 0) await _f.Client.DeleteAsync($"{ShiftDetailRoute}/{_shiftDetailId}");

        var resp = _shiftId > 0
            ? await _f.Client.DeleteAsync($"{ShiftRoute}/{_shiftId}")
            : null;
        resp.Should().NotBeNull();
        ((int)resp!.StatusCode).Should().BeOneOf(200, 400);
    }
}
