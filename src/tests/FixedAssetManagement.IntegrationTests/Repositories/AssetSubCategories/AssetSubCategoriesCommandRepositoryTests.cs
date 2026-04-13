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

        private async Task<int> SeedAssetCategoryAsync(string code = "SCC1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var groupRepo = new AssetGroupCommandRepository(ctx);
            var groupId = await groupRepo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code + "_G",
                GroupName = "Group for " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var catRepo = new AssetCategoriesCommandRepository(ctx);
            return await catRepo.CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = "Cat for " + code,
                AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static FAM.Domain.Entities.AssetSubCategories BuildEntity(
            int categoryId,
            string code = "ASC001",
            string name = "Sub Test") =>
            new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code,
                SubCategoryName = name,
                Description = "Test sub",
                AssetCategoriesId = categoryId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCC1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCC2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_P", "PersistMe"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetSubCategories.FirstAsync(x => x.Id == newId);
            saved.Code.Should().Be("ASC_P");
            saved.SubCategoryName.Should().Be("PersistMe");
            saved.AssetCategoriesId.Should().Be(catId);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCC3");

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_S1"));
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_S2"));
            ctx.ChangeTracker.Clear();

            var f = await ctx.AssetSubCategories.FirstAsync(x => x.Id == first);
            var s = await ctx.AssetSubCategories.FirstAsync(x => x.Id == second);
            s.SortOrder.Should().Be(f.SortOrder + 1);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCU1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_U1", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetSubCategories
            {
                Id = newId,
                SubCategoryName = "Renamed",
                Description = "New desc",
                AssetCategoriesId = catId,
                SortOrder = 8,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AssetSubCategories.FirstAsync(x => x.Id == newId);
            updated.SubCategoryName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetSubCategories
            {
                Id = 9999,
                SubCategoryName = "X",
                AssetCategoriesId = 1
            });

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCD1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(catId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetSubCategories.IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        // --- ExistsBy* ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCE1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_EX"));

            (await CreateRepository(ctx).ExistsByCodeAsync("ASC_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_When_Active_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCE2");
            await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_EN", "ActiveSub"));

            (await CreateRepository(ctx).ExistsByNameAsync("ActiveSub")).Should().BeTrue();
        }

        // --- Duplicates ---

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var catId = await SeedAssetCategoryAsync("SCDUP");
            await CreateRepository(ctx).CreateAsync(BuildEntity(catId, "ASC_DUP", "DupName"));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0);

            nameDup.Should().BeTrue();
        }
    }
}
