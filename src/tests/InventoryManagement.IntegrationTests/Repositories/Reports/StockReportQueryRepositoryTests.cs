using InventoryManagement.Infrastructure.Repositories.Reports;
using Microsoft.Data.SqlClient;

namespace InventoryManagement.IntegrationTests.Repositories.Reports
{
    [Collection("DatabaseCollection")]
    public sealed class StockReportQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockReportQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private StockReportQueryRepository CreateRepository()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StockReportQueryRepository(conn, _fixture.IpMock.Object);
        }

        [Fact]
        public async Task GetStockSummaryAsync_Should_Return_Empty_When_NoStock()
        {
            var result = await CreateRepository().GetStockSummaryAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStockSummaryAsync_Should_Return_Empty_With_Filters()
        {
            var result = await CreateRepository().GetStockSummaryAsync(itemId: 9999, warehouseId: 1);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSubStoresStockSummaryAsync_Should_Return_Empty_When_NoStock()
        {
            var result = await CreateRepository().GetSubStoresStockSummaryAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStockReportDivisionSummaryAsync_Should_Return_Empty_When_NoStock()
        {
            var result = await CreateRepository().GetStockReportDivisionSummaryAsync(new List<int> { 1, 2 });

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStockReportDivisionSummaryAsync_Should_Handle_EmptyUnitIds()
        {
            var result = await CreateRepository().GetStockReportDivisionSummaryAsync(new List<int>());

            result.Should().NotBeNull();
        }
    }
}
