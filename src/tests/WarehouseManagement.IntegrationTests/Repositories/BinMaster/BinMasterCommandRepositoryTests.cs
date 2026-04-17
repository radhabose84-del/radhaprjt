using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.BinMaster;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.BinMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BinMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BinMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static BinMasterCommandRepository CreateBinRepo(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private WarehouseMasterCommandRepository CreateWarehouseRepo(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var logger = new Mock<ILogger<WarehouseMasterCommandRepository>>(MockBehavior.Loose).Object;
            return new WarehouseMasterCommandRepository(ctx, logger);
        }

        private async Task<int> SeedWarehouseAsync(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
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

        private static WarehouseManagement.Domain.Entities.BinMaster BuildEntity(
            int warehouseId,
            string binCode = "BIN001",
            string binName = "Test Bin") =>
            new WarehouseManagement.Domain.Entities.BinMaster
            {
                WarehouseId = warehouseId,
                BinCode = binCode,
                BinName = binName,
                BinCapacity = 50.0m,
                CapacityUOMId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId, "BIN001", "Slot A"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BinMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.BinCode.Should().Be("BIN001");
            saved.BinName.Should().Be("Slot A");
            saved.WarehouseId.Should().Be(warehouseId);
            saved.BinCapacity.Should().Be(50.0m);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BinMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var entity = await CreateBinRepo(ctx).GetByIdAsync(newId);
            entity!.BinName = "Updated Bin";

            var result = await CreateBinRepo(ctx).UpdateAsync(entity);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId, "BIN001", "Original Bin"));
            ctx.ChangeTracker.Clear();

            var entity = await CreateBinRepo(ctx).GetByIdAsync(newId);
            entity!.BinName = "Modified Bin";
            await CreateBinRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BinMasters.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.BinName.Should().Be("Modified Bin");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Id_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var result = await CreateBinRepo(ctx).DeleteAsync(newId);

            result.Should().Be(newId);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateBinRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            await CreateBinRepo(ctx).DeleteAsync(newId);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.BinMasters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateBinRepo(ctx).DeleteAsync(9999);

            result.Should().Be(0);
        }
    }
}
