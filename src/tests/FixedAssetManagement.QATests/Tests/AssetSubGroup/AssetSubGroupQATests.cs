namespace FixedAssetManagement.QATests.Tests.AssetSubGroup;

// AssetSubGroup — /api/AssetSubGroup
// POST { code, subGroupName, groupId, subGroupPercentage, additionalDepreciation } → int
// PUT  { id, subGroupName, sortOrder, groupId, isActive, subGroupPercentage, additionalDepreciation }
// DELETE /{id}; GET ?Page..; GET /{id}; GET /by-name?SubGroupName= ; GET /groupId
// groupId FK best-effort seed id 1.

[Collection("AssetSubGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetSubGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetSubGroup";
    private static int _groupId;
    private static string _code = string.Empty;
    private string NewCode() => _f.EntityCode[..10];

    public AssetSubGroupQATests(QAServerFixture fixture) => _f = fixture;

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
        _code = NewCode();
        _groupId = await EnsureGroupAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _code, subGroupName = "QA Sub Group", groupId = _groupId,
            subGroupPercentage = 5.0, additionalDepreciation = (byte)0
        });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { code = "NOAUTH01", subGroupName = "X", groupId = _groupId });
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?SubGroupName=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, subGroupName = "QA SubGrp Upd " + _f.EntityCode[..6], sortOrder = QAHelper.RunUniqueInt(_f.EntityCode), groupId = _groupId,
            isActive = (byte)1, subGroupPercentage = 6.0, additionalDepreciation = (byte)0
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, subGroupName = "X", groupId = _groupId, isActive = (byte)1 });
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
