namespace MaintenanceManagement.QATests.Tests.ActivityCheckListMaster;

// ActivityCheckListMaster — /api/ActivityCheckListMaster
// GET ?Page.. | GET /{id} | POST { activityID, activityCheckList, unitId } | PUT | POST /ByActivityId | DELETE
// activityID/unitId FKs best-effort seed id 1.

[Collection("ActivityCheckListMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ActivityCheckListMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ActivityCheckListMaster";
    private const int Seed = 1;
    public ActivityCheckListMasterQATests(QAServerFixture fixture) => _f = fixture;

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

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { activityID = Seed, activityCheckList = "X", unitId = Seed });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }
}
