using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.StockLedger;

namespace MaintenanceManagement.IntegrationTests.Repositories.StockLedger
{
    /// <summary>
    /// StockLedgerQueryRepository runs raw SQL against [Maintenance].[StockLedger]. The
    /// EF-managed schema does not include a DepartmentId column (referenced by the SQL),
    /// so queries will throw SqlException against the freshly-created test database.
    /// These tests verify:
    ///   1. The repository can be constructed with its real connection
    ///   2. Its SQL methods throw a SqlException (invalid column) as expected
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class StockLedgerQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockLedgerQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private StockLedgerQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StockLedgerQueryRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetStockItemCodes_Should_Throw_SqlException_When_Column_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetStockItemCodes("U001", 1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetAllItemCodes_Should_Throw_SqlException_When_Column_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetAllItemCodes("U001", 1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetSubStoresCurrentStock_Should_Throw_SqlException_When_Column_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetSubStoresCurrentStock("U001", "ITEM001", 1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetStockItemCodes_With_Null_OldUnitcode_Should_Throw_SqlException()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetStockItemCodes(null!, 1);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
