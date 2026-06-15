namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-10 — Misc master setup (type → value)
//
//   As a fixed-asset administrator I define a misc *type* and then add misc *values*
//   under it, so configurable dropdown values are maintained as a type → value pair.
//
// WORKFLOW test: chains MiscTypeMaster → MiscMaster (FK MiscTypeId) under the `api/fam/...`
// route prefix and reads the value back. Fully runnable (no seeded data needed).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-10-MiscMasterSetup")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-10")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_10_MiscMasterSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string TypeRoute = "/api/fam/MiscTypeMaster";
    private const string ValueRoute = "/api/fam/MiscMaster";

    private static int _miscTypeId;
    private static string _miscTypeCode = string.Empty;
    private static int _miscValueId;

    public US_FAM_10_MiscMasterSetup_Tests(QAServerFixture fixture) => _f = fixture;

    private string TypeCode() => "FT" + _f.EntityCode[..6];
    private string ValueCode() => "FV" + _f.EntityCode[..6];

    // AC1 — a MiscTypeMaster (the type) can be created.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        _miscTypeCode = TypeCode();
        var resp = await _f.Client.PostAsJsonAsync(TypeRoute, new
        {
            miscTypeCode = _miscTypeCode,
            description = "QA FAM Misc Type " + _f.EntityCode[..6]
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
            description = "QA FAM Misc Value " + _f.EntityCode[..6]
        });
        await QAHelper.AssertOkAsync(resp);
        _miscValueId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscValueId.Should().BeGreaterThan(0);
    }

    // AC3 — the misc value is readable by id.
    [Fact, TestPriority(3)]
    public async Task Step3_MiscValueReadableById()
    {
        _miscValueId.Should().BeGreaterThan(0);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ValueRoute}/{_miscValueId}"));
    }

    // AC4 — the misc value is reachable through its type via by-name autocomplete.
    [Fact, TestPriority(4)]
    public async Task Step4_MiscValueReachableByType()
    {
        var resp = await _f.Client.GetAsync($"{ValueRoute}/by-name?name=QA&MiscTypeCode={_miscTypeCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    // AC5 — teardown (value first, then type — respect the FK order; type delete is blocked while linked).
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscValueId > 0) await _f.Client.DeleteAsync($"{ValueRoute}/{_miscValueId}");

        var resp = _miscTypeId > 0 ? await _f.Client.DeleteAsync($"{TypeRoute}/{_miscTypeId}") : null;
        resp.Should().NotBeNull();
        ((int)resp!.StatusCode).Should().BeOneOf(200, 400);
    }
}
