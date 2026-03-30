using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetCategories
{
    [Collection("DatabaseCollection")]
    public sealed class AssetCategoriesQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetCategoriesQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetCategoriesQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetCategoriesQueryRepository(conn);
        }

        private async Task<int> SeedAssetGroupAsync(string code = "AG_CAT_QRY")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "Test Group",
                SortOrder = 1,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityAsync(int assetGroupId, string code = "CAT_QRY001", string name = "Query Test Category")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetCategoriesCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = name,
                Description = "Test",
                AssetGroupId = assetGroupId,
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
        public async Task GetAllAssetCategoriesAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_GA1");
            await SeedEntityAsync(assetGroupId);

            var (items, total) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetCategoriesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_GA2");
            var id = await SeedEntityAsync(assetGroupId, "CAT_DEL1", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAssetCategoriesAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_GA3");
            await SeedEntityAsync(assetGroupId, "CAT_ALPHA", "Alpha Category");
            await SeedEntityAsync(assetGroupId, "CAT_BETA", "Beta Category");

            var (items, _) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].CategoryName.Should().Be("Alpha Category");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_ID1");
            var id = await SeedEntityAsync(assetGroupId, "CAT_ID1", "Get By Id Category");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CategoryName.Should().Be("Get By Id Category");
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
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_DEL");
            var id = await SeedEntityAsync(assetGroupId, "CAT_DEL2", "Soft Deleted Category");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetAssetCategories_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var assetGroupId = await SeedAssetGroupAsync("AG_QRY_AC1");
            await SeedEntityAsync(assetGroupId, "CAT_AC1", "Autocomplete Category");

            var results = await CreateQueryRepo().GetAssetCategories("Autocomplete");

            results.Should().NotBeEmpty();
            results[0].CategoryName.Should().Be("Autocomplete Category");
        }
    }
}
