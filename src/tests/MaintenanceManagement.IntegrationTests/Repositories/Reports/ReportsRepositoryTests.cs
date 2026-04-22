using Contracts.Interfaces;
using MaintenanceManagement.Infrastructure.Repositories.Reports;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.Reports
{
    /// <summary>
    /// Integration tests for ReportsRepository.
    /// Most report methods call stored procedures that may not exist in the test DB.
    /// These tests verify the repository can be instantiated and basic non-SP methods work.
    /// Full report tests require the stored procedures to be deployed to the test DB.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ReportsRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ReportsRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ReportsRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ReportsRepository(conn, _fixture.IpMock.Object);
        }

        [Fact]
        public void Constructor_Should_Create_Instance()
        {
            var repo = CreateRepo();

            repo.Should().NotBeNull();
        }

        [Fact(Skip = "StockLedger table created by EF Core does not have DepartmentId column; production DB has it via manual migration")]
        public async Task GetStockDetails_Should_Return_Empty_When_No_Data()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetStockDetails("TESTUNIT", 1);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
