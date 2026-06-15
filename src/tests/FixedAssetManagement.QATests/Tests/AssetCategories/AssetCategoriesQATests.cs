namespace FixedAssetManagement.QATests.Tests.AssetCategories;

// ─────────────────────────────────────────────────────────────────────────────
// AssetCategories — live-server QA suite.  Route: /api/AssetCategories
// Contract (verified 2026-06-09):
//   POST   { categoryName, description?, assetGroupId }     → data = new int id
//   PUT    { id, categoryName, description?, assetGroupId, isActive }
//   DELETE /{id}                                            (route param)
//   GET    ?PageNumber=&PageSize=&SearchTerm=
//   GET    /{id}                                            (no id guard → 200 + data:null when absent)
//   GET    /by-name?CategoryName=
//   GET    /group/{AssetGroupId}
// Required: CategoryName, AssetGroupId.  AlreadyExists on CategoryName.  No Code field.
// assetGroupId uses a best-effort seed id (1) — live reconciliation may adjust.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AssetCategoriesCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetCategoriesQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetCategories";

    // Resolved at runtime — the QA clone has no guaranteed AssetGroup id = 1.
    private static int _groupId;
    private static string _name = string.Empty;

    public AssetCategoriesQATests(QAServerFixture fixture) => _f = fixture;

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

    // ── CREATE ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _name = "QA Category " + _f.EntityCode[..6];
        _groupId = await EnsureGroupAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            categoryName = _name,
            description = "Created by QA",
            assetGroupId = _groupId
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            categoryName = "No Auth", assetGroupId = _groupId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            categoryName = "", assetGroupId = _groupId
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_AssetGroupIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            categoryName = "QA Category " + _f.EntityCode[..6], assetGroupId = 0
        });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_DuplicateName_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            categoryName = _name, assetGroupId = _groupId
        });
        await QAHelper.Assert400Async(resp);
    }

    // ── GET ALL (smoke) ────────────────────────────────────────────────────────
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

    // ── GET BY ID / GROUP ───────────────────────────────────────────────────────
    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByGroupId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/group/{_groupId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── AUTOCOMPLETE ────────────────────────────────────────────────────────────
    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?CategoryName=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    // ── UPDATE ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            categoryName = _name + " Upd",
            description = "Updated by QA",
            assetGroupId = _groupId,
            isActive = (byte)1
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, categoryName = "X", assetGroupId = _groupId, isActive = (byte)1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── DELETE (route param; ALWAYS LAST) ───────────────────────────────────────
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
