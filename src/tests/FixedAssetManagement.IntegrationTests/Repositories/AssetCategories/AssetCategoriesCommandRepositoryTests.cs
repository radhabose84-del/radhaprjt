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

        private async Task<int> SeedAssetGroupAsync(string code = "FAM_AG_C1", string name = "Test Group")
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

        private static FAM.Domain.Entities.AssetCategories BuildEntity(
            int assetGroupId,
            string code = "AC001",
            string name = "Test Category") =>
            new FAM.Domain.Entities.AssetCategories
            {
                Code = code,
                CategoryName = name,
                Description = "Integration test",
                AssetGroupId = assetGroupId,
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
            var groupId = await SeedAssetGroupAsync("FAM_AG_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC002", "Furniture"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetCategories.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.Code.Should().Be("AC002");
            saved.CategoryName.Should().Be("Furniture");
            saved.AssetGroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_C3");

            var firstId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_S1"));
            var secondId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_S2"));
            ctx.ChangeTracker.Clear();

            var first = await ctx.AssetCategories.FirstAsync(x => x.Id == firstId);
            var second = await ctx.AssetCategories.FirstAsync(x => x.Id == secondId);
            second.SortOrder.Should().Be(first.SortOrder + 1);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_C4");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetCategories.FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_U1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_U1", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetCategories
            {
                Id = newId,
                CategoryName = "Updated",
                Description = "Updated desc",
                AssetGroupId = groupId,
                SortOrder = 5,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_U2");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_U2", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetCategories
            {
                Id = newId,
                CategoryName = "Renamed",
                Description = "New desc",
                AssetGroupId = groupId,
                SortOrder = 9,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetCategories.FirstAsync(x => x.Id == newId);
            saved.CategoryName.Should().Be("Renamed");
            saved.Description.Should().Be("New desc");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetCategories
            {
                Id = 9999,
                CategoryName = "X",
                AssetGroupId = 1
            });

            result.Should().Be(-1);
        }

        // --- DELETE (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_D1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_D2");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_D2"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetCategories.IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new FAM.Domain.Entities.AssetCategories { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Code_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_EXIST"));

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("AC_EXIST");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("AC_NONE");

            exists.Should().BeFalse();
        }

        // --- ExistsByNameAsync ---

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_When_Name_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_N1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_N1", "Furniture"));

            var exists = await CreateRepository(ctx).ExistsByNameAsync("Furniture", null);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_When_Excluded()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAM_AG_N2");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_N2", "Vehicles"));

            var exists = await CreateRepository(ctx).ExistsByNameAsync("Vehicles", newId);

            exists.Should().BeFalse();
        }

        // --- CheckForDuplicatesAsync ---

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAGDUP1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_DUP1", "DupName"));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0);

            nameDup.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_SortOrder_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("FAGDUP2");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "AC_DUP2", "Name1"));
            ctx.ChangeTracker.Clear();

            var existingSortOrder = (await ctx.AssetCategories.FirstAsync(x => x.Id == newId)).SortOrder;

            var (_, sortDup) = await CreateRepository(ctx).CheckForDuplicatesAsync("Other", existingSortOrder, 0);

            sortDup.Should().BeTrue();
        }
    }
}
