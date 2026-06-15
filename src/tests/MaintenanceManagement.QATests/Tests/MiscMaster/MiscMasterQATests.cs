namespace MaintenanceManagement.QATests.Tests.MiscMaster;

// Maintenance MiscMaster — /api/maintenance/MiscMaster
// POST { miscTypeId, code, description? } → DTO/int; PUT { id, ... , isActive }; DELETE /{id}
// GET ?Page.. ; GET /{id}; GET /by-name. miscTypeId FK best-effort seed id 1.

[Collection("MaintMiscMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MiscMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/maintenance/MiscMaster";
    private const int Seed = 1;
    private string NewCode() => _f.EntityCode[..10];
    public MiscMasterQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { miscTypeId = Seed, code = NewCode(), description = "QA Misc" });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { miscTypeId = Seed, code = "NOAUTH01", description = "X" });
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
    public async Task TC010_GetAll_Returns200()
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

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, miscTypeId = Seed, code = NewCode(), description = "QA Misc Upd", sortOrder = 1, isActive = (byte)1 });
        await QAHelper.AssertOkAsync(resp);
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
