using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using InventoryManagement.Infrastructure.Repositories.UOMs;

namespace InventoryManagement.IntegrationTests.Repositories.UOM
{
    [Collection("DatabaseCollection")]
    public sealed class UOMCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "UOM_TYPE_C")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test UOM Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "UOM_MM001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "UOM Type Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private static InventoryManagement.Domain.Entities.UOM BuildEntity(
            int uomTypeId,
            string code = "PC",
            string uomName = "Piece") =>
            new InventoryManagement.Domain.Entities.UOM
            {
                Code = code,
                UOMName = uomName,
                UOMTypeId = uomTypeId,
                SortOrder = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_C1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_C2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "KG", "Kilogram"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UOMs.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("KG");
            saved.UOMName.Should().Be("Kilogram");
            saved.UOMTypeId.Should().Be(miscMasterId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_C3");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "LT", "Litre"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UOMs.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_AutoAssign_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_C4");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_C4");

            var entity = BuildEntity(miscMasterId, "MT", "Meter");
            entity.SortOrder = 0;

            var result = await CreateRepository(ctx).CreateAsync(entity);

            result.SortOrder.Should().BeGreaterThan(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_U1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_U1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "SQ", "Square"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Id = entity.Id,
                Code = "SQ",
                UOMName = "Square Meter",
                UOMTypeId = miscMasterId,
                SortOrder = entity.SortOrder,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_U2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_U2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "CU", "Cubic"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Id = entity.Id,
                Code = "CU",
                UOMName = "Cubic Meter",
                UOMTypeId = miscMasterId,
                SortOrder = entity.SortOrder,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.UOMs.FirstOrDefaultAsync(x => x.Id == entity.Id);
            updated!.UOMName.Should().Be("Cubic Meter");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Id = 9999,
                Code = "XX",
                UOMName = "Not Found",
                UOMTypeId = 1,
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeFalse();
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_D1");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_D1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "DL", "Delete UOM"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(entity.Id,
                new InventoryManagement.Domain.Entities.UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_TYPE_D2");
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "UOM_D2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "SD", "Soft Del"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(entity.Id,
                new InventoryManagement.Domain.Entities.UOM { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.UOMs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new InventoryManagement.Domain.Entities.UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
