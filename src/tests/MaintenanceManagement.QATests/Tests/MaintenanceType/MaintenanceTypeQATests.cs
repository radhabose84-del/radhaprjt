namespace MaintenanceManagement.QATests.Tests.MaintenanceType;

// MaintenanceType — /api/MaintenanceType
// POST { typeName } → int; PUT { id, ... }; DELETE (id from body/query)
// GET ?Page.. ; GET /{id}; GET /by-name?Typename=

[Collection("MaintenanceTypeCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MaintenanceTypeQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MaintenanceType";
    private static string _name = string.Empty;
    public MaintenanceTypeQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _name = "QA Type " + _f.EntityCode[..6];
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { typeName = _name });
        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { typeName = "X" });
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

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?Typename=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = _f.CreatedId, typeName = _name + " Upd", isActive = (byte)1 });
        await QAHelper.AssertOkAsync(resp);
    }
}
