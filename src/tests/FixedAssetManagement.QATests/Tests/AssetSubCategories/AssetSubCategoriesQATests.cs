namespace FixedAssetManagement.QATests.Tests.AssetSubCategories;

// AssetSubCategories — /api/AssetSubCategories
// POST { subCategoryName, description?, assetCategoriesId } → int
// PUT  { id, subCategoryName, description?, sortOrder, assetCategoriesId, isActive }
// DELETE /{id}; GET ?Page..; GET /{id}; GET /by-name?subcategoryname= ; GET /Category/{AssetCategoriesId}
// assetCategoriesId FK best-effort seed id 1.

[Collection("AssetSubCategoriesCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetSubCategoriesQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetSubCategories";
    private const int ValidCategoryId = 1;
    private static string _name = string.Empty;

    public AssetSubCategoriesQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _name = "QA SubCategory " + _f.EntityCode[..6];
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            subCategoryName = _name, description = "Created by QA", assetCategoriesId = ValidCategoryId
        });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { subCategoryName = "X", assetCategoriesId = ValidCategoryId });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { subCategoryName = "", assetCategoriesId = ValidCategoryId });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByCategoryId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/Category/{ValidCategoryId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?subcategoryname=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, subCategoryName = _name + " Upd", description = "Updated", sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            assetCategoriesId = ValidCategoryId, isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, subCategoryName = "X", assetCategoriesId = ValidCategoryId, isActive = (byte)1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
