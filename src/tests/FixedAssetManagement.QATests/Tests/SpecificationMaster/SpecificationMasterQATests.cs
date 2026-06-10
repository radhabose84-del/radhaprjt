namespace FixedAssetManagement.QATests.Tests.SpecificationMaster;

// SpecificationMaster — /api/SpecificationMaster
// POST { specificationName, assetGroupId?, isDefault } → DTO
// PUT  { id, specificationName, assetGroupId?, isDefault, isActive }
// DELETE /{id}; GET ?Page..; GET /{id}; GET /by-name?assetGroupId=&name=
// assetGroupId FK best-effort seed id 1.

[Collection("SpecificationMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SpecificationMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SpecificationMaster";
    private static int ValidAssetGroupId = 1; // resolved at runtime in TC001

    public SpecificationMasterQATests(QAServerFixture fixture) => _f = fixture;

    private async Task<int> EnsureGroupAsync()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, "/api/AssetGroup");
        if (id == 0)
        {
            var g = await _f.Client.PostAsJsonAsync("/api/AssetGroup", new { code = _f.EntityCode[..10], groupName = "QA Parent Group", groupPercentage = 10.0 });
            id = (await QAHelper.ParseAsync(g)).RootElement.CreatedId();
        }
        return id;
    }

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        ValidAssetGroupId = await EnsureGroupAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationName = "QA Spec " + _f.EntityCode[..6], assetGroupId = ValidAssetGroupId, isDefault = (byte)0
        });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { specificationName = "X", assetGroupId = ValidAssetGroupId, isDefault = (byte)0 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
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

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?assetGroupId={ValidAssetGroupId}&name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, specificationName = "QA Spec Upd", assetGroupId = ValidAssetGroupId, isDefault = (byte)0, isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, specificationName = "X", assetGroupId = ValidAssetGroupId, isDefault = (byte)0, isActive = (byte)1 });
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
