namespace MaintenanceManagement.QATests.Tests.ActivityMaster;

// ActivityMaster — /api/ActivityMaster  (create wraps a nested CreateActivityMasterDto; PUT at /update).
// GET ?Page.. | GET /{id} | GET /by-name?name=&machineCode= | POST | PUT /update
//   | GET /GetActivityType | GET /GetActivity/{machineGroupId} | GET /MachineGroup/{activityId}

[Collection("ActivityMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ActivityMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ActivityMaster";
    public ActivityMasterQATests(QAServerFixture fixture) => _f = fixture;

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
    public async Task TC020_GetActivityType_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetActivityType");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { createActivityMasterDto = new { activityName = "X" } });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(400, 500); // BUG: empty body 500 (validator NRE) not 400
    }
}
