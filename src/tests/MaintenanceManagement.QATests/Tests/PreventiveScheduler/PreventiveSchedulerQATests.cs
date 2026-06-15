namespace MaintenanceManagement.QATests.Tests.PreventiveScheduler;

// PreventiveScheduler — /api/PreventiveScheduler  (transactional; action-routed endpoints).
// POST | DELETE /{id} | PUT /reschedule | GET /SchedulerAbstractByDate ...
// Create payload is complex → auth + empty-body coverage; full lifecycle deferred.

[Collection("PreventiveSchedulerCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PreventiveSchedulerQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PreventiveScheduler";
    public PreventiveSchedulerQATests(QAServerFixture fixture) => _f = fixture;

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
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
