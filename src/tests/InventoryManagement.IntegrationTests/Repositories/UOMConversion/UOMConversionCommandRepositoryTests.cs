using Dapper;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using InventoryManagement.Infrastructure.Repositories.UOMConversion;
using InventoryManagement.Infrastructure.Repositories.UOMs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.UOMConversion
{
    [Collection("DatabaseCollection")]
    public sealed class UOMConversionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMConversionCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMConversionCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "UOMConv Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "UOMConv Misc Master",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedUOMAsync(int uomTypeId, string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new UOMCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = uomTypeId,
                SortOrder = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private static InventoryManagement.Domain.Entities.UOMConversion BuildEntity(
            int fromUOMId,
            int toUOMId,
            decimal conversionValue = 1000m) =>
            new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = fromUOMId,
                ToUOMId = toUOMId,
                ConversionValue = conversionValue,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[UOMConversion]");
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
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_C1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_C1");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_C1", "Kilogram C1");
            var toId = await SeedUOMAsync(miscId, "UC_GM_C1", "Gram C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId, 1000m));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_C2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_C2");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_C2", "Kilogram C2");
            var toId = await SeedUOMAsync(miscId, "UC_GM_C2", "Gram C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId, 500m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UOMConversions.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.FromUOMId.Should().Be(fromId);
            saved.ToUOMId.Should().Be(toId);
            saved.ConversionValue.Should().Be(500m);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_C3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_C3");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_C3", "Kilogram C3");
            var toId = await SeedUOMAsync(miscId, "UC_GM_C3", "Gram C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UOMConversions.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Updated_Entity()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_U1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_U1");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_U1", "Kilogram U1");
            var toId = await SeedUOMAsync(miscId, "UC_GM_U1", "Gram U1");

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId, 1000m));
            ctx.ChangeTracker.Clear();

            var updated = await CreateRepository(ctx).UpdateAsync(created.Id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = fromId,
                ToUOMId = toId,
                ConversionValue = 2000m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            updated.Should().NotBeNull();
            updated!.ConversionValue.Should().Be(2000m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_U2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_U2");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_U2", "Kilogram U2");
            var toId = await SeedUOMAsync(miscId, "UC_GM_U2", "Gram U2");

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId, 1000m));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(created.Id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = fromId,
                ToUOMId = toId,
                ConversionValue = 750m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var persisted = await ctx.UOMConversions.FirstOrDefaultAsync(x => x.Id == created.Id);
            persisted!.ConversionValue.Should().Be(750m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = 1,
                ToUOMId = 2,
                ConversionValue = 100m,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeNull();
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_D1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_D1");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_D1", "Kilogram D1");
            var toId = await SeedUOMAsync(miscId, "UC_GM_D1", "Gram D1");

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("UC_MT_D2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UC_MM_D2");
            var fromId = await SeedUOMAsync(miscId, "UC_KG_D2", "Kilogram D2");
            var toId = await SeedUOMAsync(miscId, "UC_GM_D2", "Gram D2");

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(fromId, toId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.UOMConversions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new InventoryManagement.Domain.Entities.UOMConversion
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            });

            result.Should().BeFalse();
        }
    }
}
