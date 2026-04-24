using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Lookups;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// MiscMasterLookupRepository queries Inventory.MiscMaster and Inventory.MiscTypeMaster,
    /// which live in the InventoryManagement module's schema. The Warehouse test database
    /// only provisions the Warehouse schema (per CLAUDE.md cross-module isolation rule),
    /// so these cross-schema tables do not exist in this test DB.
    ///
    /// These tests verify:
    ///   1. The repository can be constructed with its real dependencies.
    ///   2. SQL methods throw SqlException when the cross-schema tables are missing.
    /// Full data-flow coverage lives in the consuming module's integration tests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterLookupRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Throw_SqlException_When_Inventory_Schema_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetMiscMasterByIdAsync("AnyType");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetMiscTypeIdsAsync_Should_Throw_SqlException_When_Inventory_Schema_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetMiscTypeIdsAsync();

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
