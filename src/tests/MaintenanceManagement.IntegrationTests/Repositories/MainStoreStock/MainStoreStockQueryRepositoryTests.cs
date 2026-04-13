using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.MainStoreStock;

namespace MaintenanceManagement.IntegrationTests.Repositories.MainStoreStock
{
    /// <summary>
    /// MainStoreStockQueryRepository queries the [dbo].[DivisionProcedureMapping] table
    /// (which doesn't exist in the test DB) and then calls a dynamically-resolved stored
    /// procedure (which also doesn't exist). All methods are expected to throw
    /// SqlException. These tests verify:
    ///   1. The repository can be constructed with its real connection
    ///   2. Its SP-backed methods throw a SqlException as expected
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MainStoreStockQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MainStoreStockQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MainStoreStockQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MainStoreStockQueryRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetByItemCodeIdAsync_Should_Throw_SqlException_When_Mapping_Table_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByItemCodeIdAsync("U001", "ITEM001");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetStockDetails_Should_Throw_SqlException_When_Mapping_Table_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetStockDetails("U001", "GRP001");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetStockItemsCodes_Should_Throw_SqlException_When_Mapping_Table_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetStockItemsCodes("U001", "GRP001");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetByItemCodeIdAsync_With_Null_Inputs_Should_Throw_SqlException()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByItemCodeIdAsync(null!, null!);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
