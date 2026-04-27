using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Lookups;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// ItemGroupLookupRepository queries Inventory.ItemGroup, which lives in the
    /// InventoryManagement module's schema. The Warehouse test database only
    /// provisions the Warehouse schema (per CLAUDE.md cross-module isolation rule),
    /// so the Inventory.ItemGroup table does not exist in this test DB.
    ///
    /// These tests verify:
    ///   1. The repository can be constructed with its real dependencies.
    ///   2. SQL methods throw SqlException when the cross-schema table is missing.
    /// Full data-flow coverage lives in the consuming module's integration tests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ItemGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemGroupLookupRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetAllItemGroupsAsync_Should_Throw_SqlException_When_Inventory_Schema_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetAllItemGroupsAsync();

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var repo = CreateRepo();

            var result = await repo.GetItemGroupsByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var repo = CreateRepo();

            var result = await repo.GetItemGroupsByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetItemGroupsByIdsAsync_Should_Throw_SqlException_When_Inventory_Schema_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetItemGroupsByIdsAsync(new[] { 1, 2, 3 });

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
