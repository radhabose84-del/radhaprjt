namespace ProductionManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PROD-01 — Production reference master setup
//   As a production administrator I define misc reference values and a process master.
// Workflow: MiscTypeMaster → MiscMaster + ProcessMaster → read-back → teardown.
// Routes verified from ProductionManagement.QATests: /api/production/misctypemaster,
// /api/production/miscmaster, /api/processmaster; DELETE binds id from QUERY ?id=.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PROD-01-ReferenceMasters")]
[Trait("Module", "ProductionManagement")]
[Trait("Story", "US-PROD-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PROD_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute = "/api/production/misctypemaster";
    private const string MiscRoute     = "/api/production/miscmaster";
    private const string ProcessRoute  = "/api/processmaster";

    private static int _miscTypeId;
    private static int _miscId;
    private static int _processId;
    private static string _miscCode = string.Empty;

    public US_PROD_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = Code(),
            description  = "QA PROD MiscType"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscValue_UnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        _miscCode = Code();
        var resp = await _f.Client.PostAsJsonAsync(MiscRoute, new
        {
            miscTypeId  = _miscTypeId,
            code        = _miscCode,
            description = "QA PROD Misc Value"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(3)]
    public async Task Step3_CreateProcessMaster()
    {
        var resp = await _f.Client.PostAsJsonAsync(ProcessRoute, new
        {
            processName     = $"QAProc{Code()}",
            combingRequired = false,
            description     = "QA PROD process"
        });
        await QAHelper.AssertOkAsync(resp);
        _processId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _processId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(4)]
    public async Task Step4_EachMaster_IsReadableById()
    {
        if (_miscId > 0)    await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{MiscRoute}/{_miscId}"));
        if (_processId > 0) await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ProcessRoute}/{_processId}"));
    }

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscId > 0)     await _f.Client.DeleteAsync($"{MiscRoute}?id={_miscId}");
        if (_miscTypeId > 0) await _f.Client.DeleteAsync($"{MiscTypeRoute}?id={_miscTypeId}");
        if (_processId > 0)  await _f.Client.DeleteAsync($"{ProcessRoute}?id={_processId}");
    }
}
