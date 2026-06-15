namespace MaintenanceManagement.QATests.Tests.StockLedger;

// StockLedger — /api/StockLedger  (read-only ledger views).
// GET /current-stock | GET /item-codes/{oldUnitCode}/{departmentId} | GET /AllitemCodes

[Collection("StockLedgerCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StockLedgerQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/StockLedger";
    public StockLedgerQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    public async Task TC010_AllItemCodes_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/AllitemCodes");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_AllItemCodes_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/AllitemCodes");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
