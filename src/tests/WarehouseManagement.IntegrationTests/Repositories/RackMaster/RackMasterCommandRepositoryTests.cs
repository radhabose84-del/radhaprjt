using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.RackMaster;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.RackMaster
{
    [Collection("DatabaseCollection")]
    public sealed class RackMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RackMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static RackMasterCommandRepository CreateRackRepo(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private static WarehouseManagement.Domain.Entities.RackMaster BuildEntity(
            int warehouseId,
            string rackCode = "RACK001",
            string rackName = "Test Rack") =>
            new WarehouseManagement.Domain.Entities.RackMaster
            {
                WarehouseId = warehouseId,
                RackCode = rackCode,
                RackName = rackName,
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
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

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId, "RACK001", "Main Rack"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RackMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.RackCode.Should().Be("RACK001");
            saved.RackName.Should().Be("Main Rack");
            saved.WarehouseId.Should().Be(warehouseId);
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

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RackMasters.FirstOrDefaultAsync(x => x.Id == newId);

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

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var entity = await CreateRackRepo(ctx).GetByIdAsync(newId);
            entity!.RackName = "Updated Rack";

            var result = await CreateRackRepo(ctx).UpdateAsync(entity);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId, "RACK001", "Original Rack"));
            ctx.ChangeTracker.Clear();

            var entity = await CreateRackRepo(ctx).GetByIdAsync(newId);
            entity!.RackName = "Modified Rack";
            await CreateRackRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.RackMasters.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.RackName.Should().Be("Modified Rack");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var toDelete = new WarehouseManagement.Domain.Entities.RackMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRackRepo(ctx).DeleteAsync(newId, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var warehouseId = await SeedWarehouseAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRackRepo(ctx).CreateAsync(BuildEntity(warehouseId));
            ctx.ChangeTracker.Clear();

            var toDelete = new WarehouseManagement.Domain.Entities.RackMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await CreateRackRepo(ctx).DeleteAsync(newId, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.RackMasters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var toDelete = new WarehouseManagement.Domain.Entities.RackMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRackRepo(ctx).DeleteAsync(9999, toDelete);

            result.Should().BeFalse();
        }
    }
}
