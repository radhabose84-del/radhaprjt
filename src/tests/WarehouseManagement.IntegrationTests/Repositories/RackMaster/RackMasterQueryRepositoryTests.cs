using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.RackMaster;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.RackMaster
{
    [Collection("DatabaseCollection")]
    public sealed class RackMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RackMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private RackMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private WarehouseMasterCommandRepository CreateWarehouseRepo(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var logger = new Mock<ILogger<WarehouseMasterCommandRepository>>(MockBehavior.Loose).Object;
            return new WarehouseMasterCommandRepository(ctx, logger);
        }

        private async Task<int> SeedWarehouseAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateWarehouseRepo(ctx).CreateAsync(new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = "WH001",
                WarehouseName = "Test Warehouse",
                UnitId = 1,
                IsGroup = false,
                IsVirtualWarehouse = false,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "000000",
                IsScrapWarehouse = false,
                IsTransitWarehouse = false,
                MaxCapacity = 100,
                IsDefaultStockEntry = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedRackAsync(int warehouseId, string rackCode = "RACK001", string rackName = "Test Rack")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new RackMasterCommandRepository(ctx);
            return await repo.CreateAsync(new WarehouseManagement.Domain.Entities.RackMaster
            {
                WarehouseId = warehouseId,
                RackCode = rackCode,
                RackName = rackName,
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedRackAsync(warehouseId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Correct_RackCode()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedRackAsync(warehouseId, "RACK001", "Main Rack");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].RackCode.Should().Be("RACK001");
            items[0].RackName.Should().Be("Main Rack");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            var id = await SeedRackAsync(warehouseId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new WarehouseManagement.Domain.Entities.RackMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new RackMasterCommandRepository(ctx).DeleteAsync(id, toDelete);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedRackAsync(warehouseId, "RACK001", "Alpha Rack");
            await SeedRackAsync(warehouseId, "RACK002", "Beta Rack");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].RackName.Should().Be("Alpha Rack");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            var id = await SeedRackAsync(warehouseId, "RACK001", "Query Rack");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.RackCode.Should().Be("RACK001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            var id = await SeedRackAsync(warehouseId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new WarehouseManagement.Domain.Entities.RackMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new RackMasterCommandRepository(ctx).DeleteAsync(id, toDelete);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
