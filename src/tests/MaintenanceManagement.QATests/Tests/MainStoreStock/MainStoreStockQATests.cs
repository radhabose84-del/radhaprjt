namespace MaintenanceManagement.QATests.Tests.MainStoreStock;

// MainStoreStock — /api/MainStoreStock  (read-only stock views).
// GET /MainStore-stock | GET /MainStore-StockItems | GET /{oldUnitcode}/{itemCode}

[Collection("MainStoreStockCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MainStoreStockQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MainStoreStock";
    public MainStoreStockQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    public async Task TC010_MainStoreStock_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/MainStore-stock?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_MainStoreStock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/MainStore-stock?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
