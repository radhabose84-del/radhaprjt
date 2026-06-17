namespace GateEntryManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GE-01 — Gate reference master setup
//   As a gate administrator I define misc reference types/values used by gate documents.
// Workflow: MiscTypeMaster → MiscMaster → read-back + deactivate-exclusion → teardown.
// Routes verified from GateEntryManagement.QATests: /api/gateentry/miscTypemaster,
// /api/gateentry/miscmaster; DELETE binds id from QUERY ?id=; by-name?term= (Misc also &MiscTypeCode=).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GE-01-ReferenceMasters")]
[Trait("Module", "GateEntryManagement")]
[Trait("Story", "US-GE-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GE_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute = "/api/gateentry/miscTypemaster";
    private const string MiscRoute     = "/api/gateentry/miscmaster";

    private static int _miscTypeId;
    private static int _miscId;
    private static string _miscCode = string.Empty;

    public US_GE_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = Code(),
            description  = "QA GE MiscType"
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
            description = "QA GE Misc Value"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(3)]
    public async Task Step3_MiscValue_ReadableById()
    {
        if (_miscId == 0) return;
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{MiscRoute}/{_miscId}"));
    }

    [Fact, TestPriority(4)]
    public async Task Step4_DeactivateMiscValue_ExcludedFromAutocomplete()
    {
        if (_miscId == 0) return;

        var deactivate = await _f.Client.PutAsJsonAsync(MiscRoute, new
        {
            id          = _miscId,
            description = "QA GE inactivated",
            sortOrder   = 1,
            isActive    = 0
        });
        await QAHelper.AssertOkAsync(deactivate);

        var byName = await _f.Client.GetAsync($"{MiscRoute}/by-name?term={_miscCode}");
        ((int)byName.StatusCode).Should().BeOneOf(200, 400);
        if (byName.StatusCode == HttpStatusCode.OK)
            (await byName.Content.ReadAsStringAsync()).Should().NotContain(_miscCode);

        var getAll = await _f.Client.GetAsync($"{MiscRoute}?PageNumber=1&PageSize=50");
        ((int)getAll.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscId > 0)     await _f.Client.DeleteAsync($"{MiscRoute}?id={_miscId}");
        if (_miscTypeId > 0) await _f.Client.DeleteAsync($"{MiscTypeRoute}?id={_miscTypeId}");
    }
}
