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

        private async Task<int> SeedEntityAsync(string code = "AGQ_001", string name = "Group Q", decimal pct = 5m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                GroupPercentage = pct,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Return_Seeded()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("AGQ_A", "Alpha");
            await SeedEntityAsync("AGQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].GroupName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllAssetGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetGroupCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAssetGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("AGQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("AGQ_ID");
            result.GroupName.Should().Be("ById");
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
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetGroupCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetAssetGroups (autocomplete) ---

        [Fact]
        public async Task GetAssetGroups_Should_Return_Matching_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("AGQ_AC", "AutoComp");

            var result = await CreateQueryRepo().GetAssetGroups("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAssetGroups_Should_Return_Empty_When_NoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var result = await CreateQueryRepo().GetAssetGroups("ZZZNoSuch");

            result.Should().BeEmpty();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            var id1 = await SeedEntityAsync("AGQ_M1", "Multi1");
            var id2 = await SeedEntityAsync("AGQ_M2", "Multi2");

            var result = await CreateQueryRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdsAsync(System.Array.Empty<int>());

            result.Should().BeEmpty();
        }

        // --- IsAssetGroupLinkedAsync ---

        [Fact]
        public async Task IsAssetGroupLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var linked = await CreateQueryRepo().IsAssetGroupLinkedAsync(id);

            linked.Should().BeFalse();
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var blocked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            blocked.Should().BeFalse();
        }
    }
}
