using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSubCategories
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSubCategoriesCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSubCategoriesCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSubCategoriesCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedAssetGroupAsync(string code = "AG_SC_CMD")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "Test Group",
                SortOrder = 1,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedAssetCategoryAsync(int assetGroupId, string code = "CAT_SC_CMD")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = "Test Category",
                Description = "Test",
                AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static FAM.Domain.Entities.AssetSubCategories BuildEntity(
            int assetCategoriesId,
            string code = "SC001",
            string name = "Test Sub Category") =>
            new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code,
                SubCategoryName = name,
                Description = "Test Description",
                AssetCategoriesId = assetCategoriesId,
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
            var groupId = await SeedAssetGroupAsync("AG_SC_C1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("AG_SC_C2");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "SC_LAND", "Land"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetSubCategories.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("SC_LAND");
            saved.SubCategoryName.Should().Be("Land");
            saved.AssetCategoriesId.Should().Be(catId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("AG_SC_C3");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_C3");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetSubCategories.FirstOrDefaultAsync(x => x.Id == newId);

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
            var groupId = await SeedAssetGroupAsync("AG_SC_U1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_U1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "SC_UPD001", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetSubCategories
            {
                Id = newId,
                Code = "SC_UPD001",
                SubCategoryName = "Updated Sub Category",
                Description = "Updated",
                AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("AG_SC_U2");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_U2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "SC_UPD002", "Original Name"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetSubCategories
            {
                Id = newId,
                Code = "SC_UPD002",
                SubCategoryName = "Updated Name",
                Description = "Updated",
                AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AssetSubCategories.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.SubCategoryName.Should().Be("Updated Name");
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Value_GreaterThanZero_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("AG_SC_D1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_D1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("AG_SC_D2");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_SC_D2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "SC_DEL001", "Delete Me"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetSubCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
