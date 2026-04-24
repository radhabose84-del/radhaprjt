using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Lookups;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class RackLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RackLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private RackLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new RackLookupRepository(conn);
        }

        private async Task<int> SeedWarehouseAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wh = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = "WH-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                WarehouseName = "Test Warehouse",
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                Pincode = "600001",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.WarehouseMasters.Add(wh);
            await ctx.SaveChangesAsync();
            return wh.Id;
        }

        private async Task<int> SeedRackAsync(
            int warehouseId, string code, string name,
            bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var rack = new WarehouseManagement.Domain.Entities.RackMaster
            {
                WarehouseId = warehouseId,
                RackCode = code,
                RackName = name,
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                IsActive = Status.Active,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.RackMasters.Add(rack);
            await ctx.SaveChangesAsync();
            return rack.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_NonDeleted_Racks()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedRackAsync(whId, "R-1", "Rack One");
            await SeedRackAsync(whId, "R-2", "Rack Two");

            var result = await CreateRepo().GetAllAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.RackCode).Should().BeEquivalentTo(new[] { "R-1", "R-2" });
        }

        [Fact]
        public async Task GetAllAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedRackAsync(whId, "R-KEEP", "Kept");
            await SeedRackAsync(whId, "R-DEL", "Deleted", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.RackCode.Should().Be("R-KEEP");
        }

        [Fact]
        public async Task GetAllAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Matching_Racks()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            var id1 = await SeedRackAsync(whId, "R-1", "One");
            var id2 = await SeedRackAsync(whId, "R-2", "Two");
            await SeedRackAsync(whId, "R-3", "Three");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.RackCode).Should().BeEquivalentTo(new[] { "R-1", "R-2" });
        }

        [Fact]
        public async Task GetByWarehouseIdAsync_Filters_By_Warehouse()
        {
            await _fixture.ClearAllTablesAsync();
            var wh1 = await SeedWarehouseAsync();
            var wh2 = await SeedWarehouseAsync();
            await SeedRackAsync(wh1, "R-W1", "For WH1");
            await SeedRackAsync(wh2, "R-W2", "For WH2");

            var result = await CreateRepo().GetByWarehouseIdAsync(wh1);

            result.Should().ContainSingle().Which.RackCode.Should().Be("R-W1");
        }

        [Fact]
        public async Task GetByWarehouseIdAsync_Returns_Empty_When_NoMatch()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByWarehouseIdAsync(9999);

            result.Should().BeEmpty();
        }
    }
}
