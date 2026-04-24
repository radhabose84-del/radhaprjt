using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Validations;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// HasLinkedUomAsync / HasActiveUomAsync check 4 columns across 3 tables:
    ///   Warehouse.WarehouseMaster.CapacityUOMId
    ///   Warehouse.RackMaster.CapacityUOMId
    ///   Warehouse.RackMaster.DimensionUOMId
    ///   Warehouse.BinMaster.CapacityUOMId
    /// One representative test per source table covers the OR EXISTS branches.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class WarehouseUomValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseUomValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseUomValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WarehouseUomValidationRepository(conn);
        }

        private async Task<int> SeedWarehouseAsync(
            int capacityUomId,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wh = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = "WH-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                WarehouseName = "Test WH",
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = capacityUomId,
                AddressLine1 = "Test Address",
                Pincode = "600001",
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.WarehouseMasters.Add(wh);
            await ctx.SaveChangesAsync();
            return wh.Id;
        }

        private async Task<int> SeedRackAsync(
            int warehouseId,
            int capacityUomId,
            int? dimensionUomId = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
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
                CapacityUOMId = capacityUomId,
                DimensionUOMId = dimensionUomId,
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.RackMasters.Add(rack);
            await ctx.SaveChangesAsync();
            return rack.Id;
        }

        private async Task SeedBinAsync(
            int warehouseId,
            int rackId,
            int capacityUomId,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var bin = new WarehouseManagement.Domain.Entities.BinMaster
            {
                WarehouseId = warehouseId,
                RackId = rackId,
                BinCode = "B-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                BinName = "Test Bin",
                BinCapacity = 100m,
                CapacityUOMId = capacityUomId,
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.BinMasters.Add(bin);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedUomAsync ---

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_Used_By_Warehouse_Capacity()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(capacityUomId: 10);

            var result = await CreateRepo().HasLinkedUomAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_Used_By_Rack_Capacity()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync(capacityUomId: 1);
            await SeedRackAsync(whId, capacityUomId: 20);

            var result = await CreateRepo().HasLinkedUomAsync(20);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_Used_By_Rack_Dimension()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync(capacityUomId: 1);
            await SeedRackAsync(whId, capacityUomId: 1, dimensionUomId: 30);

            var result = await CreateRepo().HasLinkedUomAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_Used_By_Bin_Capacity()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync(capacityUomId: 1);
            var rackId = await SeedRackAsync(whId, capacityUomId: 1);
            await SeedBinAsync(whId, rackId, capacityUomId: 40);

            var result = await CreateRepo().HasLinkedUomAsync(40);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedUomAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_SoftDeleted_Everywhere()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(capacityUomId: 50, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedUomAsync(50);

            result.Should().BeFalse();
        }

        // --- HasActiveUomAsync ---

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_True_When_Warehouse_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(capacityUomId: 60, active: Status.Active);

            var result = await CreateRepo().HasActiveUomAsync(60);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_All_Sources_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(capacityUomId: 70, active: Status.Inactive);

            var result = await CreateRepo().HasActiveUomAsync(70);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(capacityUomId: 80, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasActiveUomAsync(80);

            result.Should().BeFalse();
        }
    }
}
