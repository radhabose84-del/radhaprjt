namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-04 — Asset onboarding & placement
//   As an asset administrator I register an asset, place it at a location, and
//   capture its specifications.
// PARTIAL: the prerequisite classification is runnable; the AssetMasterGeneral create
// (nested AssetMasterDto), AssetLocation and AssetSpecification steps need seeded data
// and are Skipped — un-skip and fill the payloads during live reconciliation.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-04-AssetOnboarding")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-04")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_04_AssetOnboarding_Tests
{
    private readonly QAServerFixture _f;
    private const string GroupRoute = "/api/AssetGroup";
    private static int _groupId;

    public US_FAM_04_AssetOnboarding_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — prerequisite classification exists.
    [Fact, TestPriority(1)]
    public async Task Step1_PrerequisiteGroupExists()
    {
        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new { code = _f.EntityCode[..10], groupName = "QA Onboarding Group", groupPercentage = 10.0 });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Needs seeded data: AssetMasterGeneral create wraps a complex AssetMasterDto (classification/uom/manufacturer/etc.). Author the payload during live reconciliation."), TestPriority(2)]
    public Task Step2_CreateAssetMasterGeneral() => Task.CompletedTask;

    [Fact(Skip = "Needs the created asset id (US-FAM-04 Step2). Assign asset to Unit/Department/Location/SubLocation/Custodian via AssetLocation."), TestPriority(3)]
    public Task Step3_AssignAssetLocation() => Task.CompletedTask;

    [Fact(Skip = "Needs the created asset id + a SpecificationMaster. Capture AssetSpecification values."), TestPriority(4)]
    public Task Step4_CaptureAssetSpecification() => Task.CompletedTask;

    [Fact(Skip = "Needs the created asset id. Verify GET /api/AssetMasterGeneral/{id} returns the asset with classification."), TestPriority(5)]
    public Task Step5_AssetReadableWithClassification() => Task.CompletedTask;

    // Teardown the runnable prerequisite.
    [Fact, TestPriority(6)]
    public async Task Step6_Teardown()
    {
        if (_groupId > 0) await _f.Client.DeleteAsync($"{GroupRoute}/{_groupId}");
    }
}
