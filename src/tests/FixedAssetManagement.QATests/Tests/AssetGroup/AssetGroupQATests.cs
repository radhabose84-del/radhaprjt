namespace FixedAssetManagement.QATests.Tests.AssetGroup;

// ─────────────────────────────────────────────────────────────────────────────
// AssetGroup — live-server QA suite.  Route: /api/AssetGroup
// Contract (verified 2026-06-09):
//   POST   { code, groupName, groupPercentage? }            → data = new int id
//   PUT    { id, groupName, sortOrder, isActive, groupPercentage? }  (Code immutable)
//   DELETE /{id}                                            (route param)
//   GET    ?PageNumber=&PageSize=&SearchTerm=
//   GET    /{id}                                            (no id guard → 200 + data:null when absent)
//   GET    /by-name?groupname=
// Create returns int. Code is required + unique. Clean master (no FK).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AssetGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetGroup";

    private static string _code = string.Empty;
    private string NewCode() => _f.EntityCode[..10];

    public AssetGroupQATests(QAServerFixture fixture) => _f = fixture;

    // ── CREATE ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _code = NewCode();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _code,
            groupName = "QA Asset Group",
            groupPercentage = 10.5
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { code = "NOAUTH01", groupName = "No Auth" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { code = "", groupName = "QA Asset Group" });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { code = _code, groupName = "QA Asset Group" });
        await QAHelper.Assert400Async(resp);
    }

    // ── GET ALL (smoke) ────────────────────────────────────────────────────────
    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET BY ID ───────────────────────────────────────────────────────────────
    [Fact, TestPriority(20)]
    public async Task TC020_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── AUTOCOMPLETE ────────────────────────────────────────────────────────────
    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?groupname=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?groupname=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── UPDATE ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            groupName = "QA Grp Upd " + _f.EntityCode[..6],
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            isActive = (byte)1,
            groupPercentage = 12.0
        });
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId, groupName = "X", sortOrder = 1, isActive = (byte)1
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
