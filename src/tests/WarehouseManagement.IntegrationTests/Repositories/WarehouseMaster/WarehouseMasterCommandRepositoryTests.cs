using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.WarehouseMaster
{
    [Collection("DatabaseCollection")]
    public sealed class WarehouseMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseMasterCommandRepository CreateRepository(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var logger = new Mock<ILogger<WarehouseMasterCommandRepository>>(MockBehavior.Loose).Object;
            return new WarehouseMasterCommandRepository(ctx, logger);
        }

        private static WarehouseManagement.Domain.Entities.WarehouseMaster BuildEntity(
            string code = "WH001",
            string name = "Test Warehouse") =>
            new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = code,
                WarehouseName = name,
                UnitId = 1,
                IsGroup = false,
                IsVirtualWarehouse = false,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address Line 1",
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
            };

        private async Task ClearTablesAsync(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WH001", "Main Warehouse"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WarehouseMasters.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.WarehouseCode.Should().Be("WH001");
            saved.WarehouseName.Should().Be("Main Warehouse");
            saved.UnitId.Should().Be(1);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WarehouseMasters.FirstOrDefaultAsync(x => x.Id == newId);

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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepository(ctx).GetByIdAsync(newId);
            entity!.WarehouseName = "Updated Warehouse";

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WH002", "Original Name"));
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepository(ctx).GetByIdAsync(newId);
            entity!.WarehouseName = "Updated Name";
            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.WarehouseMasters.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.WarehouseName.Should().Be("Updated Name");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(newId, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.WarehouseMasters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, toDelete);

            result.Should().BeFalse();
        }

        // --- WAREHOUSE ITEM GROUP MAPPING (via WarehouseMaster) ---

        private static WarehouseManagement.Domain.Entities.WarehouseMaster BuildEntityWithMappings(
            string code = "WH001",
            string name = "Test Warehouse",
            params int[] itemGroupIds)
        {
            var entity = BuildEntity(code, name);
            foreach (var groupId in itemGroupIds)
            {
                entity.AllowedItemGroups.Add(new WarehouseManagement.Domain.Entities.WarehouseItemGroupMapping
                {
                    ItemGroupId = groupId,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            }
            return entity;
        }

        [Fact]
        public async Task CreateAsync_WithItemGroupMappings_Should_Persist_Mappings()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntityWithMappings("WH010", "Mapped WH", 1, 2, 3);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WarehouseMasters
                .Include(w => w.AllowedItemGroups)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.AllowedItemGroups.Should().HaveCount(3);
            saved.AllowedItemGroups.Select(m => m.ItemGroupId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task CreateAsync_WithItemGroupMappings_Should_Set_WarehouseId_On_Mappings()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntityWithMappings("WH011", "FK WH", 5);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WarehouseMasters
                .Include(w => w.AllowedItemGroups)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved!.AllowedItemGroups.Should().ContainSingle()
                .Which.WarehouseId.Should().Be(newId);
        }

        [Fact]
        public async Task CreateAsync_WithNoMappings_Should_Persist_EmptyCollection()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity("WH012", "No Mapping WH");
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WarehouseMasters
                .Include(w => w.AllowedItemGroups)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved!.AllowedItemGroups.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_ItemGroupMappings()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntityWithMappings("WH013", "Replace WH", 1, 2);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            // Load, clear old mappings, add new ones
            var loaded = await CreateRepository(ctx).GetByIdAsync(newId);
            loaded!.AllowedItemGroups.Clear();
            loaded.AllowedItemGroups.Add(new WarehouseManagement.Domain.Entities.WarehouseItemGroupMapping
            {
                ItemGroupId = 10,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            loaded.AllowedItemGroups.Add(new WarehouseManagement.Domain.Entities.WarehouseItemGroupMapping
            {
                ItemGroupId = 20,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await CreateRepository(ctx).UpdateAsync(loaded);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.WarehouseMasters
                .Include(w => w.AllowedItemGroups)
                .FirstOrDefaultAsync(x => x.Id == newId);

            updated!.AllowedItemGroups.Select(m => m.ItemGroupId)
                .Should().BeEquivalentTo(new[] { 10, 20 });
        }

        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_ItemGroupMappings()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntityWithMappings("WH014", "Delete WH", 1, 2);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, toDelete);
            ctx.ChangeTracker.Clear();

            var mappings = await ctx.GetWarehouseItemGroupMappings
                .IgnoreQueryFilters()
                .Where(m => m.WarehouseId == newId)
                .ToListAsync();

            mappings.Should().HaveCount(2);
            mappings.Should().AllSatisfy(m =>
                m.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted));
        }

        [Fact]
        public async Task GetByIdAsync_Should_Include_ItemGroupMappings()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntityWithMappings("WH015", "Include WH", 3, 7);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var loaded = await CreateRepository(ctx).GetByIdAsync(newId);

            loaded.Should().NotBeNull();
            loaded!.AllowedItemGroups.Should().HaveCount(2);
            loaded.AllowedItemGroups.Select(m => m.ItemGroupId).Should().BeEquivalentTo(new[] { 3, 7 });
        }
    }
}
