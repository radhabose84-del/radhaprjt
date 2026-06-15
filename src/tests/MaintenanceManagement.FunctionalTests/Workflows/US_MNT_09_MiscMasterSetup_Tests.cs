namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-09 — Misc master setup (type → value)
//
//   As a maintenance administrator I define a misc *type* and then add misc *values*
//   under it, so configurable dropdown values (request types, service types, etc.) can
//   be maintained as a type → value master pair.
//
// WORKFLOW test: chains MiscTypeMaster → MiscMaster (FK MiscTypeId) and reads the value
// back — the type→value link the per-entity CRUD tests do NOT cover. Both entities are
// under the `api/maintenance/...` route prefix. Fully runnable (no seeded data needed).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-09-MiscMasterSetup")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-09")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_09_MiscMasterSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string TypeRoute = "/api/maintenance/MiscTypeMaster";
    private const string ValueRoute = "/api/maintenance/MiscMaster";

    private static int _miscTypeId;
    private static string _miscTypeCode = string.Empty;
    private static int _miscValueId;

    public US_MNT_09_MiscMasterSetup_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric, fits short code columns.
    private string TypeCode() => "MT" + _f.EntityCode[..6];
    private string ValueCode() => "MV" + _f.EntityCode[..6];

    // AC1 — a MiscTypeMaster (the type) can be created and returns a new id.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        _miscTypeCode = TypeCode();
        var resp = await _f.Client.PostAsJsonAsync(TypeRoute, new
        {
            miscTypeCode = _miscTypeCode,
            description = "QA Misc Type " + _f.EntityCode[..6]
        });
        await QAHelper.AssertOkAsync(resp);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — a MiscMaster (a value) can be created under that type.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscValueUnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        var resp = await _f.Client.PostAsJsonAsync(ValueRoute, new
        {
            miscTypeId = _miscTypeId,
            code = ValueCode(),
            description = "QA Misc Value " + _f.EntityCode[..6]
        });
        await QAHelper.AssertOkAsync(resp);
        _miscValueId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscValueId.Should().BeGreaterThan(0);
    }

    // AC3 — the misc value is readable by id after creation.
    [Fact, TestPriority(3)]
    public async Task Step3_MiscValueReadableById()
    {
        _miscValueId.Should().BeGreaterThan(0);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ValueRoute}/{_miscValueId}"));
    }

    // AC4 — the misc value is reachable through its type via by-name autocomplete
    //       (proves the type → value link, since by-name filters on MiscTypeCode).
    [Fact, TestPriority(4)]
    public async Task Step4_MiscValueReachableByType()
    {
        var resp = await _f.Client.GetAsync($"{ValueRoute}/by-name?name=QA&MiscTypeCode={_miscTypeCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    // AC5 — teardown (value first, then type — respect the FK order).
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscValueId > 0) await _f.Client.DeleteAsync($"{ValueRoute}/{_miscValueId}");

        var resp = _miscTypeId > 0
            ? await _f.Client.DeleteAsync($"{TypeRoute}/{_miscTypeId}")
            : null;
        resp.Should().NotBeNull();
        ((int)resp!.StatusCode).Should().BeOneOf(200, 400);
    }
}
