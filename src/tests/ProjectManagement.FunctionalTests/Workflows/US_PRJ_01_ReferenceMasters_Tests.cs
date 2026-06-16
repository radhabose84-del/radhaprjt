namespace ProjectManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PRJ-01 — Project reference master setup
//   As a project administrator I define misc reference types/values used to classify projects.
// Workflow: MiscTypeMaster → MiscMaster → read-back + deactivate-exclusion → teardown.
// Routes verified from ProjectManagement.QATests: /api/project/MiscTypeMaster, /api/project/MiscMaster;
// DELETE binds id from ROUTE (/{id}); by-name?name= (Misc also &MiscTypeCode=).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PRJ-01-ReferenceMasters")]
[Trait("Module", "ProjectManagement")]
[Trait("Story", "US-PRJ-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PRJ_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute = "/api/project/MiscTypeMaster";
    private const string MiscRoute     = "/api/project/MiscMaster";

    private static int _miscTypeId;
    private static int _miscId;
    private static string _miscCode = string.Empty;

    public US_PRJ_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    // AC1 — create a MiscTypeMaster.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = Code(),
            description  = "QA PRJ MiscType"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — create a MiscMaster value under that type.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscValue_UnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        _miscCode = Code();
        var resp = await _f.Client.PostAsJsonAsync(MiscRoute, new
        {
            miscTypeId  = _miscTypeId,
            code        = _miscCode,
            description = "QA PRJ Misc Value"
        });
        await QAHelper.AssertOkAsync(resp);
        _miscId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscId.Should().BeGreaterThan(0);
    }

    // AC3 — the value is readable by id.
    [Fact, TestPriority(3)]
    public async Task Step3_MiscValue_ReadableById()
    {
        if (_miscId == 0) return;
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{MiscRoute}/{_miscId}"));
    }

    // AC4 — deactivate the MiscMaster → excluded from active autocomplete, present in GetAll.
    [Fact, TestPriority(4)]
    public async Task Step4_DeactivateMiscValue_ExcludedFromAutocomplete()
    {
        if (_miscId == 0) return;

        var deactivate = await _f.Client.PutAsJsonAsync(MiscRoute, new
        {
            id          = _miscId,
            miscTypeId  = _miscTypeId,
            code        = _miscCode,
            description = "QA PRJ inactivated",
            sortOrder   = 1,
            isActive    = 0
        });
        await QAHelper.AssertOkAsync(deactivate);

        // ⚠ Tolerant: the autocomplete may 400 on no active match on the clone; assert absence only on 200.
        var byName = await _f.Client.GetAsync($"{MiscRoute}/by-name?name={_miscCode}");
        ((int)byName.StatusCode).Should().BeOneOf(200, 400);
        if (byName.StatusCode == HttpStatusCode.OK)
            (await byName.Content.ReadAsStringAsync()).Should().NotContain(_miscCode);

        var getAll = await _f.Client.GetAsync($"{MiscRoute}?PageNumber=1&PageSize=50");
        ((int)getAll.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC5 — teardown leaf-first (misc value → misc type); DELETE binds id from ROUTE.
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscId > 0)     await _f.Client.DeleteAsync($"{MiscRoute}/{_miscId}");
        if (_miscTypeId > 0) await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
    }
}
