namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-03 — Depreciation policy & specification setup
//   As a finance/asset administrator I configure a DepreciationGroup and a
//   SpecificationMaster for an AssetGroup so depreciation and specs apply to its assets.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-03-DepreciationSetup")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_03_DepreciationSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string GroupRoute   = "/api/AssetGroup";
    private const string DepGroupRoute = "/api/DepreciationGroup";
    private const string SpecRoute    = "/api/SpecificationMaster";

    private static int _groupId;
    private static int _depGroupId;
    private static int _specId;

    public US_FAM_03_DepreciationSetup_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    // AC1 — an AssetGroup exists (created in-flow).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateAssetGroup()
    {
        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new { code = Code(), groupName = "QA Dep Group", groupPercentage = 10.0 });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    // AC2 — a DepreciationGroup can be created for that AssetGroup.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateDepreciationGroupForGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        var depCode = Code();
        var resp = await _f.Client.PostAsJsonAsync(DepGroupRoute, new
        {
            code = depCode, depreciationGroupName = "QA Dep Policy", bookType = 1,
            assetGroupId = _groupId, depreciationMethod = 1, usefulLife = 5, residualValue = 1
        });
        await QAHelper.AssertOkAsync(resp);
        // Create returns 200 with data:null — resolve the new id by the unique code.
        _depGroupId = await QAHelper.FirstIdAsync(_f.Client, $"{DepGroupRoute}?SearchTerm={depCode}");
        _depGroupId.Should().BeGreaterThan(0);
    }

    // AC3 — a SpecificationMaster can be created for that AssetGroup.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateSpecificationForGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync(SpecRoute, new
        {
            specificationName = "QA Spec " + _f.EntityCode[..6], assetGroupId = _groupId, isDefault = (byte)0
        });
        await QAHelper.AssertOkAsync(resp);
        _specId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _specId.Should().BeGreaterThan(0);
    }

    // AC4 — the spec is returned for the group via by-name.
    [Fact, TestPriority(4)]
    public async Task Step4_SpecReachableByGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        var resp = await _f.Client.GetAsync($"{SpecRoute}/by-name?assetGroupId={_groupId}&name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // AC5 — teardown removes the depreciation group + specification (+ group).
    [Fact, TestPriority(5)]
    public async Task Step5_Teardown()
    {
        if (_specId > 0)     await _f.Client.DeleteAsync($"{SpecRoute}/{_specId}");
        if (_depGroupId > 0) await _f.Client.DeleteAsync($"{DepGroupRoute}/{_depGroupId}");
        if (_groupId > 0)    await _f.Client.DeleteAsync($"{GroupRoute}/{_groupId}");
    }
}
