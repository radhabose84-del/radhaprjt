using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSubCategories
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSubCategoriesQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSubCategoriesQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSubCategoriesQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetSubCategoriesQueryRepository(conn);
        }

        private async Task<int> SeedAssetGroupAsync(string code)
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

        private async Task<int> SeedAssetCategoryAsync(int groupId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = "Test Category",
                Description = "Test",
                AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityAsync(int catId, string code = "SC_QRY001", string name = "Query Test Sub Cat")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetSubCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code,
                SubCategoryName = name,
                Description = "Test",
                AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetSubCategories]");
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetCategories]");
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetGroup]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_GA1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_GA1");
            await SeedEntityAsync(catId);

            var (items, total) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_GA2");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_GA2");
            var id = await SeedEntityAsync(catId, "SC_DEL1", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetSubCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_GA3");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_GA3");
            await SeedEntityAsync(catId, "SC_ALPHA", "Alpha Sub Category");
            await SeedEntityAsync(catId, "SC_BETA", "Beta Sub Category");

            var (items, _) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].SubCategoryName.Should().Be("Alpha Sub Category");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_ID1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_ID1");
            var id = await SeedEntityAsync(catId, "SC_ID1", "Get By Id Sub Cat");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.SubCategoryName.Should().Be("Get By Id Sub Cat");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_DEL");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_DEL");
            var id = await SeedEntityAsync(catId, "SC_DEL2", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetSubCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetAssetSubCategories_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("AG_SCQ_AC1");
            var catId = await SeedAssetCategoryAsync(groupId, "CAT_AC1");
            await SeedEntityAsync(catId, "SC_AC1", "Autocomplete Sub Cat");

            var results = await CreateQueryRepo().GetAssetSubCategories("Autocomplete");

            results.Should().NotBeEmpty();
            results[0].SubCategoryName.Should().Be("Autocomplete Sub Cat");
        }
    }
}
