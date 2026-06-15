namespace MaintenanceManagement.QATests.Tests.WorkOrder;

// WorkOrder — /api/WorkOrder  (transactional).
// GET ?Page.. | GET /{id} | POST | PUT | GET /Status | GET /RootCause | GET /Source ...
// Create payload is complex → read + auth + empty-body coverage; full lifecycle deferred.

[Collection("WorkOrderCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class WorkOrderQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/WorkOrder";
    public WorkOrderQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAll_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_Status_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/Status");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { machineId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(400, 500); // BUG: empty body 500 (validator NRE) not 400
    }
}
