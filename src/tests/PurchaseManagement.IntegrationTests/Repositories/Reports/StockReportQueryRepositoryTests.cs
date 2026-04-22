using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.Reports.StockReport;

namespace PurchaseManagement.IntegrationTests.Repositories.Reports
{
    /// <summary>
    /// Integration tests for StockReportQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Stock reports query the Purchase.StockLedger table which is populated as a
    /// side effect of GRN processing, Issue entries, and Issue Returns.
    /// Without seeded StockLedger data, queries return empty results.
    ///
    /// Constructor requires: IDbConnection, IIPAddressService
    ///
    /// Methods:
    /// - GetStockSummaryAsync: Aggregates stock by Item, Warehouse, StorageType, Target
    /// - GetSubStoresStockAsync: Sub-store specific stock summary
    ///
    /// Full testing requires seeding the entire GRN -> StockLedger chain.
    /// Basic empty-result tests are provided here.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class StockReportQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockReportQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StockReportQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StockReportQueryRepository(conn, _fixture.IpMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetStockSummaryAsync_Should_Return_Empty_When_NoStockLedger()
        {
            await _fixture.ClearAllTablesAsync();

            var results = await CreateRepo().GetStockSummaryAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStockSummaryAsync_Should_Accept_Filter_Parameters()
        {
            await _fixture.ClearAllTablesAsync();

            // All filters set — should still return empty without seeded data
            var results = await CreateRepo().GetStockSummaryAsync(
                itemId: 1, warehouseId: 1, storageTypeId: 1, targetId: 1);

            results.Should().BeEmpty();
        }
    }
}
