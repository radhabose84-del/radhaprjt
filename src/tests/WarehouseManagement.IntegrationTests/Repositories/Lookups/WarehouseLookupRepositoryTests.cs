using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Infrastructure.Repositories.Lookups;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class WarehouseLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WarehouseLookupRepository(conn);
        }

        private async Task<int> SeedWarehouseAsync(
            string code, string name,
            int? parentWarehouseId = null,
            bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wh = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = code,
                WarehouseName = name,
                UnitId = 1,
                ParentWarehouseId = parentWarehouseId,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                Pincode = "600001",
                IsActive = Status.Active,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.WarehouseMasters.Add(wh);
            await ctx.SaveChangesAsync();
            return wh.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_NonDeleted_Warehouses()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync("WH-A", "Alpha Warehouse");
            await SeedWarehouseAsync("WH-B", "Beta Warehouse");

            var result = await CreateRepo().GetAllAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.WarehouseCode).Should().BeEquivalentTo(new[] { "WH-A", "WH-B" });
        }

        [Fact]
        public async Task GetAllAsync_Orders_By_Code_Asc()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync("WH-Z", "Zulu");
            await SeedWarehouseAsync("WH-A", "Alpha");
            await SeedWarehouseAsync("WH-M", "Mike");

            var result = await CreateRepo().GetAllAsync();

            result.Select(r => r.WarehouseCode).Should().ContainInOrder("WH-A", "WH-M", "WH-Z");
        }

        [Fact]
        public async Task GetAllAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync("WH-KEEP", "Kept");
            await SeedWarehouseAsync("WH-DEL", "Deleted", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.WarehouseCode.Should().Be("WH-KEEP");
        }

        [Fact]
        public async Task GetAllAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_Includes_ParentWarehouseId()
        {
            await _fixture.ClearAllTablesAsync();
            var parentId = await SeedWarehouseAsync("WH-PARENT", "Parent");
            await SeedWarehouseAsync("WH-CHILD", "Child", parentWarehouseId: parentId);

            var result = await CreateRepo().GetAllAsync();

            var child = result.First(r => r.WarehouseCode == "WH-CHILD");
            child.ParentWarehouseId.Should().Be(parentId);
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Matching_Warehouses()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedWarehouseAsync("WH-1", "One");
            var id2 = await SeedWarehouseAsync("WH-2", "Two");
            var id3 = await SeedWarehouseAsync("WH-3", "Three");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.WarehouseCode).Should().BeEquivalentTo(new[] { "WH-1", "WH-2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedWarehouseAsync("WH-KEEP", "Kept");
            var id2 = await SeedWarehouseAsync("WH-DEL", "Deleted", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().ContainSingle().Which.WarehouseCode.Should().Be("WH-KEEP");
        }
    }
}
