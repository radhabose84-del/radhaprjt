namespace MaintenanceManagement.QATests.Tests.Feeder;

// Feeder — /api/Feeder
// POST /create { feederCode, feederName, feederGroupId, feederTypeId, unitId, departmentId, ... } → int
// PUT { id, ... }; DELETE /{id}; GET ?Page.. ; GET /{id}; GET /by-name?name=
// Many FKs → full create lifecycle deferred to live reconciliation; core + auth coverage here.

[Collection("FeederCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class FeederQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Feeder";
    public FeederQATests(QAServerFixture fixture) => _f = fixture;

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
    public async Task TC020_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/create", new { feederCode = "X", feederName = "X" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
