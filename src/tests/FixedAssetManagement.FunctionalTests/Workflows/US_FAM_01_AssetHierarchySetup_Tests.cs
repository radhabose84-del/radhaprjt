namespace FixedAssetManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-FAM-01 — Asset classification hierarchy setup
//
//   As an asset administrator I build the classification hierarchy
//   (AssetGroup → AssetCategory → AssetSubGroup → AssetSubCategory) so assets
//   can be classified.
//
// WORKFLOW test: chains creates across four entities and verifies the parent→child
// linkage (group→category) via a read-back — behaviour the per-entity CRUD tests in
// FixedAssetManagement.QATests do NOT cover. See Stories/Story-Catalogue.md.
//
// xUnit builds a NEW class instance per test, so cross-step ids are held in static
// fields and steps are ordered with [TestPriority] over one shared QAServerFixture.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-FAM-01-AssetHierarchySetup")]
[Trait("Module", "FixedAssetManagement")]
[Trait("Story", "US-FAM-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_FAM_01_AssetHierarchySetup_Tests
{
    private readonly QAServerFixture _f;

    private const string GroupRoute       = "/api/AssetGroup";
    private const string CategoryRoute    = "/api/AssetCategories";
    private const string SubGroupRoute    = "/api/AssetSubGroup";
    private const string SubCategoryRoute = "/api/AssetSubCategories";

    // Workflow state carried across ordered steps (static — new instance per test).
    private static int _groupId;
    private static int _categoryId;
    private static int _subGroupId;
    private static int _subCategoryId;
    private static string _categoryName = string.Empty;

    public US_FAM_01_AssetHierarchySetup_Tests(QAServerFixture fixture) => _f = fixture;

    // Step 1 — create the root AssetGroup.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateAssetGroup()
    {
        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new
        {
            code = _f.EntityCode[..10],
            groupName = "QA FAM Group",
            groupPercentage = 10.0
        });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    // Step 2 — create an AssetCategory under that group.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateCategoryUnderGroup()
    {
        _groupId.Should().BeGreaterThan(0, "Step1 must have created the group");
        _categoryName = "QA FAM Category " + _f.EntityCode[..6];

        var resp = await _f.Client.PostAsJsonAsync(CategoryRoute, new
        {
            categoryName = _categoryName,
            description = "USFAM01 workflow",
            assetGroupId = _groupId
        });
        await QAHelper.AssertOkAsync(resp);
        _categoryId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _categoryId.Should().BeGreaterThan(0);
    }

    // Step 3 — create an AssetSubGroup under the group.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateSubGroupUnderGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync(SubGroupRoute, new
        {
            code = _f.EntityCode[..10],
            subGroupName = "QA FAM SubGroup",
            groupId = _groupId,
            subGroupPercentage = 5.0,
            additionalDepreciation = (byte)0
        });
        await QAHelper.AssertOkAsync(resp);
        _subGroupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _subGroupId.Should().BeGreaterThan(0);
    }

    // Step 4 — create an AssetSubCategory under the category.
    [Fact, TestPriority(4)]
    public async Task Step4_CreateSubCategoryUnderCategory()
    {
        _categoryId.Should().BeGreaterThan(0, "Step2 must have created the category");
        var resp = await _f.Client.PostAsJsonAsync(SubCategoryRoute, new
        {
            subCategoryName = "QA FAM SubCategory " + _f.EntityCode[..6],
            description = "USFAM01 workflow",
            assetCategoriesId = _categoryId
        });
        await QAHelper.AssertOkAsync(resp);
        _subCategoryId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _subCategoryId.Should().BeGreaterThan(0);
    }

    // Step 5 — verify the category is reachable via the group linkage endpoint.
    [Fact, TestPriority(5)]
    public async Task Step5_CategoryAppearsUnderGroup()
    {
        _groupId.Should().BeGreaterThan(0);
        var resp = await _f.Client.GetAsync($"{CategoryRoute}/group/{_groupId}");
        await QAHelper.AssertOkAsync(resp);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain(_categoryName,
            "the category created under this group in Step2 should be returned by group/{groupId}");
    }

    // Step 6 — teardown leaf-first (best-effort; keeps the QA clone clean across runs).
    [Fact, TestPriority(6)]
    public async Task Step6_TeardownLeafFirst()
    {
        if (_subCategoryId > 0) await _f.Client.DeleteAsync($"{SubCategoryRoute}/{_subCategoryId}");
        if (_subGroupId > 0)    await _f.Client.DeleteAsync($"{SubGroupRoute}/{_subGroupId}");
        if (_categoryId > 0)    await _f.Client.DeleteAsync($"{CategoryRoute}/{_categoryId}");

        var resp = _groupId > 0
            ? await _f.Client.DeleteAsync($"{GroupRoute}/{_groupId}")
            : null;
        resp.Should().NotBeNull();
        await QAHelper.AssertOkAsync(resp!);
    }
}
