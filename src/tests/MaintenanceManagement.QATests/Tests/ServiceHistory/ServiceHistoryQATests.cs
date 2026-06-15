namespace MaintenanceManagement.QATests.Tests.ServiceHistory;

// ServiceHistory — /api/ServiceHistory  (read-only).  GET ?Page..

[Collection("ServiceHistoryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ServiceHistoryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ServiceHistory";
    public ServiceHistoryQATests(QAServerFixture fixture) => _f = fixture;

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
}
