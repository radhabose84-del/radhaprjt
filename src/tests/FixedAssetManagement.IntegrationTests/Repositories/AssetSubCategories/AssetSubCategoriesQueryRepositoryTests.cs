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

        private async Task<int> SeedAssetCategoryAsync(string code = "ACQ_SUB1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var groupId = await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code + "_G",
                GroupName = "G " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = "Cat " + code,
                AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityAsync(int catId, string code = "ASCQ001", string name = "Sub Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetSubCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code,
                SubCategoryName = name,
                Description = "Q desc",
                AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB1");
            await SeedEntityAsync(catId);

            var (items, total) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Populate_AssetCategoriesName()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB2");
            await SeedEntityAsync(catId);

            var (items, _) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, null);

            items[0].AssetCategoriesName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllAssetSubCategoriesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB3");
            var id = await SeedEntityAsync(catId);

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
            var catId = await SeedAssetCategoryAsync("ACQ_SUB4");
            await SeedEntityAsync(catId, "ASCQ_A", "AlphaSub");
            await SeedEntityAsync(catId, "ASCQ_B", "BetaSub");

            var (items, _) = await CreateQueryRepo().GetAllAssetSubCategoriesAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].SubCategoryName.Should().Be("AlphaSub");
        }

        // GetById

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB5");
            var id = await SeedEntityAsync(catId, "ASCQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("ASCQ_ID");
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
            var catId = await SeedAssetCategoryAsync("ACQ_SUB6");
            var id = await SeedEntityAsync(catId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetSubCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetSubCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // GetAssetSubCategories (autocomplete)

        [Fact]
        public async Task GetAssetSubCategories_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB7");
            await SeedEntityAsync(catId, "ASCQ_AC", "AutoSub");

            var result = await CreateQueryRepo().GetAssetSubCategories("Auto");

            result.Should().HaveCount(1);
        }

        // GetSubcategoriesByAssetCategoryIdAsync

        [Fact]
        public async Task GetSubcategoriesByAssetCategoryIdAsync_Should_Return_Children()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB8");
            await SeedEntityAsync(catId, "ASCQ_C1");
            await SeedEntityAsync(catId, "ASCQ_C2");

            var result = await CreateQueryRepo().GetSubcategoriesByAssetCategoryIdAsync(catId);

            result.Should().HaveCount(2);
        }

        // IsAssetSubCategoryLinkedAsync / SoftDeleteValidationAsync

        [Fact]
        public async Task IsAssetSubCategoryLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQ_SUB9");
            var id = await SeedEntityAsync(catId);

            (await CreateQueryRepo().IsAssetSubCategoryLinkedAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var catId = await SeedAssetCategoryAsync("ACQS10");
            var id = await SeedEntityAsync(catId);

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeFalse();
        }
    }
}
