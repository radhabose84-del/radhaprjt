using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Lookups;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class BinLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BinLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BinLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BinLookupRepository(conn);
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

        private async Task<int> SeedRackAsync(int warehouseId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var rack = new WarehouseManagement.Domain.Entities.RackMaster
            {
                WarehouseId = warehouseId,
                RackCode = "R-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                RackName = "Test Rack",
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.RackMasters.Add(rack);
            await ctx.SaveChangesAsync();
            return rack.Id;
        }

        private async Task<int> SeedBinAsync(
            int warehouseId, int? rackId, string code, string name,
            bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var bin = new WarehouseManagement.Domain.Entities.BinMaster
            {
                WarehouseId = warehouseId,
                RackId = rackId,
                BinCode = code,
                BinName = name,
                BinCapacity = 100m,
                CapacityUOMId = 1,
                IsActive = Status.Active,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.BinMasters.Add(bin);
            await ctx.SaveChangesAsync();
            return bin.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_NonDeleted_Bins()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            var rackId = await SeedRackAsync(whId);
            await SeedBinAsync(whId, rackId, "B-1", "Bin One");
            await SeedBinAsync(whId, rackId, "B-2", "Bin Two");

            var result = await CreateRepo().GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            var rackId = await SeedRackAsync(whId);
            await SeedBinAsync(whId, rackId, "B-KEEP", "Kept");
            await SeedBinAsync(whId, rackId, "B-DEL", "Deleted", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.BinCode.Should().Be("B-KEEP");
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
        public async Task GetByIdsAsync_Returns_Matching_Bins()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            var rackId = await SeedRackAsync(whId);
            var id1 = await SeedBinAsync(whId, rackId, "B-1", "One");
            var id2 = await SeedBinAsync(whId, rackId, "B-2", "Two");
            await SeedBinAsync(whId, rackId, "B-3", "Three");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.BinCode).Should().BeEquivalentTo(new[] { "B-1", "B-2" });
        }

        [Fact]
        public async Task GetByWarehouseIdAsync_Filters_By_Warehouse()
        {
            await _fixture.ClearAllTablesAsync();
            var wh1 = await SeedWarehouseAsync();
            var wh2 = await SeedWarehouseAsync();
            var r1 = await SeedRackAsync(wh1);
            var r2 = await SeedRackAsync(wh2);
            await SeedBinAsync(wh1, r1, "B-W1", "For WH1");
            await SeedBinAsync(wh2, r2, "B-W2", "For WH2");

            var result = await CreateRepo().GetByWarehouseIdAsync(wh1);

            result.Should().ContainSingle().Which.BinCode.Should().Be("B-W1");
        }

        [Fact]
        public async Task GetByRackIdAsync_Filters_By_Rack()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            var r1 = await SeedRackAsync(whId);
            var r2 = await SeedRackAsync(whId);
            await SeedBinAsync(whId, r1, "B-R1", "For Rack1");
            await SeedBinAsync(whId, r2, "B-R2", "For Rack2");

            var result = await CreateRepo().GetByRackIdAsync(r1);

            result.Should().ContainSingle().Which.BinCode.Should().Be("B-R1");
        }
    }
}
