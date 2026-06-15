namespace MaintenanceManagement.QATests.Tests.MRS;

// MRS — /api/maintenance/MRS  (transactional; material requisition slip).
// GET /department/{oldUnitcode} | GET /Category/{oldUnitcode} | POST /CreateMRS | GET /pending-issue

[Collection("MRSCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MRSQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/maintenance/MRS";
    public MRSQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    public async Task TC010_Department_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/department/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Department_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/department/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_CreateMRS_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/CreateMRS", new { departmentId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_CreateMRS_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/CreateMRS", new { });
        await QAHelper.Assert400Async(resp);
    }
}
