namespace MaintenanceManagement.QATests.Tests.MachineSpecification;

// MachineSpecification — /api/MachineSpecification  (no GET-all, no delete).
// POST | PUT | GET /{id}

[Collection("MachineSpecificationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MachineSpecificationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MachineSpecification";
    public MachineSpecificationQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    public async Task TC020_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
        await QAHelper.Assert400Async(resp);
    }
}
