using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetCategories
{
    [Collection("DatabaseCollection")]
    public sealed class AssetCategoriesCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetCategoriesCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetCategoriesCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedAssetGroupAsync(string code = "AG_CAT_CMD")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "Test Group for Category",
                SortOrder = 1,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static FAM.Domain.Entities.AssetCategories BuildEntity(
            int assetGroupId,
            string code = "CAT001",
            string name = "Test Category") =>
            new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = name,
                Description = "Test Description",
                AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetSubCategories]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetCategories]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetGroup]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId, "CAT_BLDG", "Buildings"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetCategories.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("CAT_BLDG");
            saved.CategoryName.Should().Be("Buildings");
            saved.AssetGroupId.Should().Be(assetGroupId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_C3");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetCategories.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_U1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId, "CAT_UPD001", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetCategories
            {
                Id = newId,
                Code = "CAT_UPD001",
                CategoryName = "Updated Category",
                Description = "Updated Desc",
                AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_U2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId, "CAT_UPD002", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetCategories
            {
                Id = newId,
                Code = "CAT_UPD002",
                CategoryName = "Updated Name",
                Description = "Updated",
                AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AssetCategories.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.CategoryName.Should().Be("Updated Name");
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Value_GreaterThanZero_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_D1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetGroupId = await SeedAssetGroupAsync("AG_CMD_D2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetGroupId, "CAT_DEL001", "Delete Me"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
