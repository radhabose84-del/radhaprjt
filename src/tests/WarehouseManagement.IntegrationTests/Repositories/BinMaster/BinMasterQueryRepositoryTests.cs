using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.BinMaster;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.BinMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BinMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BinMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BinMasterQueryRepository CreateQueryRepo() =>
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

        private async Task<int> SeedBinAsync(int warehouseId, string binCode = "BIN001", string binName = "Test Bin")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new BinMasterCommandRepository(ctx);
            return await repo.CreateAsync(new WarehouseManagement.Domain.Entities.BinMaster
            {
                WarehouseId = warehouseId,
                BinCode = binCode,
                BinName = binName,
                BinCapacity = 50.0m,
                CapacityUOMId = 1,
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
            await SeedBinAsync(warehouseId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Correct_BinCode()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedBinAsync(warehouseId, "BIN001", "Main Bin");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].BinCode.Should().Be("BIN001");
            items[0].BinName.Should().Be("Main Bin");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            var id = await SeedBinAsync(warehouseId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new BinMasterCommandRepository(ctx).DeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedBinAsync(warehouseId, "BIN001", "Alpha Bin");
            await SeedBinAsync(warehouseId, "BIN002", "Beta Bin");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].BinName.Should().Be("Alpha Bin");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Multiple_Records()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedBinAsync(warehouseId, "BIN001", "Bin One");
            await SeedBinAsync(warehouseId, "BIN002", "Bin Two");
            await SeedBinAsync(warehouseId, "BIN003", "Bin Three");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(3);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            var id = await SeedBinAsync(warehouseId, "BIN001", "Query Bin");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.BinCode.Should().Be("BIN001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- EXISTS BY WAREHOUSE AND CODE ---

        [Fact]
        public async Task ExistsByWarehouseAndCodeAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();
            await SeedBinAsync(warehouseId, "BIN001");

            var exists = await CreateQueryRepo().ExistsByWarehouseAndCodeAsync(warehouseId, "BIN001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByWarehouseAndCodeAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();
            var warehouseId = await SeedWarehouseAsync();

            var exists = await CreateQueryRepo().ExistsByWarehouseAndCodeAsync(warehouseId, "NONEXISTENT");

            exists.Should().BeFalse();
        }
    }
}
