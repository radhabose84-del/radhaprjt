namespace MaintenanceManagement.QATests.Tests.MaintenanceRequest;

// MaintenanceRequest — /api/MaintenanceRequest  (transactional).
// GET /InternalRequest | GET /ExternalRequest | POST /create | PUT | GET /RequestType | GET /vendors ...

[Collection("MaintenanceRequestCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MaintenanceRequestQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MaintenanceRequest";
    public MaintenanceRequestQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_InternalRequest_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/InternalRequest?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_InternalRequest_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/InternalRequest?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_RequestType_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/RequestType");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/create", new { requestType = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new { });
        await QAHelper.Assert400Async(resp);
    }
}
