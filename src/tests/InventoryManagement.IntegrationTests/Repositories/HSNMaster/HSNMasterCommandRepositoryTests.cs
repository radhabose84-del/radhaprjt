using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.HSNMaster;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace InventoryManagement.IntegrationTests.Repositories.HSNMaster
{
    [Collection("DatabaseCollection")]
    public sealed class HSNMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HSNMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private HSNMasterCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "HSN_TYPE_C")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test HSN Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "HSN_MM001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "Test Misc Master",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private static InventoryManagement.Domain.Entities.HSNMaster BuildEntity(
            int typeId,
            int gstCategoryId,
            string hsnCode = "1234",
            string description = "Test HSN") =>
            new InventoryManagement.Domain.Entities.HSNMaster
            {
                TypeId = typeId,
                GSTCategoryId = gstCategoryId,
                HSNCode = hsnCode,
                Description = description,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[HSNMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOMConversion]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_C1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_C2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_C2");

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(miscMasterId, miscMasterId, "0101", "Live Animals"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.HSNMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.HSNCode.Should().Be("0101");
            saved.Description.Should().Be("Live Animals");
            saved.TypeId.Should().Be(miscMasterId);
            saved.GSTCategoryId.Should().Be(miscMasterId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_C3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_C3");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId, "0202"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.HSNMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Calculate_CGST_SGST_From_GSTPercentage()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_C4");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_C4");

            var entity = BuildEntity(miscMasterId, miscMasterId, "0303");
            entity.GSTPercentage = 18m;

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.HSNMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CGSTPercentage.Should().Be(9m);
            saved.SGSTPercentage.Should().Be(9m);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Id_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_U1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_U1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId, "1001", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.HSNMaster
            {
                Id = newId,
                TypeId = miscMasterId,
                GSTCategoryId = miscMasterId,
                HSNCode = "1001",
                Description = "Updated Description",
                GSTPercentage = 12m,
                IGSTPercentage = 12m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(newId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_U2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_U2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId, "1002", "Original Desc"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.HSNMaster
            {
                Id = newId,
                TypeId = miscMasterId,
                GSTCategoryId = miscMasterId,
                HSNCode = "1002",
                Description = "Updated Desc",
                GSTPercentage = 5m,
                IGSTPercentage = 5m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.HSNMaster.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.Description.Should().Be("Updated Desc");
            updated.GSTPercentage.Should().Be(5m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.HSNMaster
            {
                Id = 9999,
                TypeId = 1,
                GSTCategoryId = 1,
                HSNCode = "9999",
                Description = "Not Found",
                ValidFrom = DateTimeOffset.UtcNow
            });

            result.Should().Be(0);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_D1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_D1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId, "2001"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("HSN_TYPE_D2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "HSN_D2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, miscMasterId, "2002"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.HSNMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new InventoryManagement.Domain.Entities.HSNMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
