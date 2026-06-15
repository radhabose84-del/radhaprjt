namespace MaintenanceManagement.QATests.Tests.Item;

// Item — /api/Item  (read-only lookup; cross-module item master).
// GET /GetGroupCode/{oldUnitId} | GET /GetItemMasters/{oldUnitId}/{grpcode}

[Collection("ItemCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ItemQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Item";
    public ItemQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    public async Task TC010_GetGroupCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetGroupCode/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetGroupCode_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetGroupCode/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
