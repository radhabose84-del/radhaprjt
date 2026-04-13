using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSubGroup
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSubGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSubGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSubGroupCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedAssetGroupAsync(string code = "ASG_G1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AssetGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "Group " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static FAM.Domain.Entities.AssetSubGroup BuildEntity(
            int groupId,
            string code = "ASG_S1",
            string name = "Sub Group") =>
            new FAM.Domain.Entities.AssetSubGroup
            {
                Code = code,
                SubGroupName = name,
                GroupId = groupId,
                SubGroupPercentage = 5m,
                AdditionalDepreciation = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // CREATE

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_P", "PersistMe"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetSubGroup.FirstAsync(x => x.Id == newId);
            saved.Code.Should().Be("ASG_P");
            saved.SubGroupName.Should().Be("PersistMe");
            saved.GroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_C3");

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_S1"));
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_S2"));
            ctx.ChangeTracker.Clear();

            var f = await ctx.AssetSubGroup.FirstAsync(x => x.Id == first);
            var s = await ctx.AssetSubGroup.FirstAsync(x => x.Id == second);
            s.SortOrder.Should().Be(f.SortOrder + 1);
        }

        // UPDATE

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_U1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_U", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetSubGroup
            {
                Id = newId,
                SubGroupName = "Updated",
                GroupId = groupId,
                SortOrder = 7,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.AssetSubGroup.FirstAsync(x => x.Id == newId)).SubGroupName.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetSubGroup
            {
                Id = 9999,
                SubGroupName = "X",
                GroupId = 1
            });

            result.Should().Be(-1);
        }

        // DELETE

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_D1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetSubGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetSubGroup.IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new FAM.Domain.Entities.AssetSubGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        // ExistsByCode / ExistsAsync

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_EX"));

            (await CreateRepository(ctx).ExistsByCodeAsync("ASG_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_GroupId_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_E2");

            (await CreateRepository(ctx).ExistsAsync(groupId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_GroupId_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            (await CreateRepository(ctx).ExistsAsync(9999)).Should().BeFalse();
        }

        // CheckForDuplicates

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("ASG_DUP");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ASG_DUP", "DupName"));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0);

            nameDup.Should().BeTrue();
        }
    }
}
