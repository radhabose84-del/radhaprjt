using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetGroup
{
    [Collection("DatabaseCollection")]
    public sealed class AssetGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetGroupQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "AG_QRY001", string name = "Query Test Group", decimal pct = 10m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                SortOrder = 1,
                GroupPercentage = pct,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetSubCategories]");
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetCategories]");
            await conn.ExecuteAsync("DELETE FROM [FixedAsset].[AssetGroup]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("AG_DEL1", "To Delete Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetGroupCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("AG_A1", "Alpha Asset Group");
            await SeedEntityAsync("AG_B1", "Beta Asset Group");

            var (items, _) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].GroupName.Should().Be("Alpha Asset Group");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("AG_ID1", "Get By Id Group");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.GroupName.Should().Be("Get By Id Group");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("AG_DEL2", "Soft Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetGroupCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetAssetGroups_Should_Return_Matching_Groups()
        {
            await ClearTableAsync();
            await SeedEntityAsync("AG_AC1", "Autocomplete Group");

            var results = await CreateQueryRepo().GetAssetGroups("Autocomplete");

            results.Should().NotBeEmpty();
            results[0].GroupName.Should().Be("Autocomplete Group");
        }

        // --- EXISTS ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedEntityAsync("AG_EX1", "Existing Group");

            var exists = await new AssetGroupCommandRepository(
                    _fixture.CreateFreshDbContext())
                .ExistsByCodeAsync("AG_EX1");

            exists.Should().BeTrue();
        }
    }
}
