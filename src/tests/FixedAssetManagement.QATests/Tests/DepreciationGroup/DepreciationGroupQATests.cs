namespace FixedAssetManagement.QATests.Tests.DepreciationGroup;

// DepreciationGroup — /api/DepreciationGroup
// POST { code, depreciationGroupName, bookType, assetGroupId, depreciationMethod, usefulLife, residualValue } → DTO
// PUT  { id, code, bookType, depreciationGroupName, assetGroupId, usefulLife?, depreciationMethod, residualValue?, sortOrder, isActive }
// DELETE /{id}; GET ?Page..; GET /{id}; GET /by-name?name= ; GET /bookType ; GET /DepMethod
// assetGroupId/bookType/depreciationMethod FKs best-effort seed id 1.

[Collection("DepreciationGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DepreciationGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DepreciationGroup";
    private static int ValidAssetGroupId = 1; // resolved at runtime in TC001
    private static string _code = string.Empty;
    private string NewCode() => _f.EntityCode[..10];

    public DepreciationGroupQATests(QAServerFixture fixture) => _f = fixture;

    // Always create a FRESH AssetGroup: DepreciationGroup uniqueness is on the
    // (AssetGroup + BookType) combination, so reusing a group collides across runs.
    private async Task<int> EnsureGroupAsync()
    {
        var g = await _f.Client.PostAsJsonAsync("/api/AssetGroup", new { code = _f.EntityCode[..10], groupName = "QA Dep Parent " + _f.EntityCode[..6], groupPercentage = 10.0 });
        return (await QAHelper.ParseAsync(g)).RootElement.CreatedId();
    }

    private object ValidPayload(int id = 0) => new
    {
        id,
        code = _code,
        depreciationGroupName = "QA Dep Group",
        bookType = 1,
        assetGroupId = ValidAssetGroupId,
        depreciationMethod = 1,
        usefulLife = 5,
        residualValue = 1,
        sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
        isActive = 1
    };

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        ValidAssetGroupId = await EnsureGroupAsync();
        _code = NewCode();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidPayload());
        await QAHelper.AssertOkAsync(resp);
        // Create returns 200 with data:null (the new id isn't surfaced — backend mapping gap),
        // so resolve the new id by searching GetAll for the unique code we just created.
        var id = await QAHelper.FirstIdAsync(_f.Client, $"{BaseRoute}?SearchTerm={_code}");
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, ValidPayload());
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, ValidPayload(_f.CreatedId));
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, ValidPayload(_f.CreatedId));
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
