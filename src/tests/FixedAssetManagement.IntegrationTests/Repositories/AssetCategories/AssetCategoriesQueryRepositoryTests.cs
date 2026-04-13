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

        private async Task<int> SeedAssetGroupAsync(string code = "FAM_AGQ_C1", string name = "Group Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityAsync(int groupId, string code = "ACQ_001", string name = "Cat Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetCategoriesCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = name,
                Description = "Q desc",
                AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAssetCategoriesAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ1");
            await SeedEntityAsync(groupId);

            var (items, total) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetCategoriesAsync_Should_Populate_AssetGroupName()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ2", "Joined Group");
            await SeedEntityAsync(groupId);

            var (items, _) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, null);

            items[0].AssetGroupName.Should().Be("Joined Group");
        }

        [Fact]
        public async Task GetAllAssetCategoriesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ3");
            var id = await SeedEntityAsync(groupId, "ACQ_DEL", "ToDelete");

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
            var groupId = await SeedAssetGroupAsync("FAM_AGQ4");
            await SeedEntityAsync(groupId, "ACQ_A", "AlphaCat");
            await SeedEntityAsync(groupId, "ACQ_B", "BetaCat");

            var (items, _) = await CreateQueryRepo().GetAllAssetCategoriesAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].CategoryName.Should().Be("AlphaCat");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ5");
            var id = await SeedEntityAsync(groupId, "ACQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("ACQ_ID");
            result.CategoryName.Should().Be("ById");
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
            var groupId = await SeedAssetGroupAsync("FAM_AGQ6");
            var id = await SeedEntityAsync(groupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetCategoriesCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AutoComplete by AssetGroupId ---

        [Fact]
        public async Task GetByAssetgroupIdAsync_Should_Return_Active_Children()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ7");
            await SeedEntityAsync(groupId, "ACQ_C1", "Cat1");
            await SeedEntityAsync(groupId, "ACQ_C2", "Cat2");

            var result = await CreateQueryRepo().GetByAssetgroupIdAsync(groupId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByAssetgroupIdAsync_Should_Return_Empty_When_GroupId_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByAssetgroupIdAsync(9999);

            result.Should().BeEmpty();
        }

        // --- GetAssetCategories (autocomplete by name) ---

        [Fact]
        public async Task GetAssetCategories_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ8");
            await SeedEntityAsync(groupId, "ACQ_AC1", "AutoCompleteCat");

            var result = await CreateQueryRepo().GetAssetCategories("AutoComp");

            result.Should().HaveCount(1);
            result[0].CategoryName.Should().Be("AutoCompleteCat");
        }

        [Fact]
        public async Task GetAssetCategories_Should_Return_Empty_When_NoMatch()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ9");
            await SeedEntityAsync(groupId, "ACQ_X", "Existing");

            var result = await CreateQueryRepo().GetAssetCategories("NoSuchName");

            result.Should().BeEmpty();
        }

        // --- IsAssetCategoryLinkedAsync ---

        [Fact]
        public async Task IsAssetCategoryLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ10");
            var id = await SeedEntityAsync(groupId);

            var linked = await CreateQueryRepo().IsAssetCategoryLinkedAsync(id);

            linked.Should().BeFalse();
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("FAM_AGQ11");
            var id = await SeedEntityAsync(groupId);

            var blocked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            blocked.Should().BeFalse();
        }
    }
}
