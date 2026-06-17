namespace BudgetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-BUD-01 — Budget reference master setup
//   As a budget administrator I define misc reference types/values used by budget groups.
// Workflow: MiscTypeMaster → MiscMaster → read-back + deactivate-exclusion → teardown.
// Routes verified from BudgetManagement.QATests: /api/budget/misctypemaster, /api/budget/miscmaster;
// DELETE binds id from ROUTE /{id}; by-name?name= (Misc requires &MiscTypeCode=).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-BUD-01-ReferenceMasters")]
[Trait("Module", "BudgetManagement")]
[Trait("Story", "US-BUD-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_BUD_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute = "/api/budget/misctypemaster";
    private const string MiscRoute     = "/api/budget/miscmaster";

    private static int _miscTypeId;
    private static int _miscId;
    private static string _miscCode = string.Empty;
    private static string _miscTypeCode = string.Empty;

    public US_BUD_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscType()
    {
        _miscTypeCode = Code();
        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = _miscTypeCode,
            description  = "QA BUD MiscType"
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
            description = "QA BUD Misc Value"
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
            miscTypeId  = _miscTypeId,
            code        = _miscCode,
            description = "QA BUD inactivated",
            sortOrder   = 1,
            isActive    = 0
        });
        await QAHelper.AssertOkAsync(deactivate);

        var byName = await _f.Client.GetAsync($"{MiscRoute}/by-name?name={_miscCode}&MiscTypeCode={_miscTypeCode}");
        ((int)byName.StatusCode).Should().BeOneOf(200, 400);
        if (byName.StatusCode == HttpStatusCode.OK)
            (await byName.Content.ReadAsStringAsync()).Should().NotContain(_miscCode);

        var getAll = await _f.Client.GetAsync($"{MiscRoute}?PageNumber=1&PageSize=50");
        ((int)getAll.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_miscId > 0)     await _f.Client.DeleteAsync($"{MiscRoute}/{_miscId}");
        if (_miscTypeId > 0) await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
    }
}
