using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetGroup
{
    [Collection("DatabaseCollection")]
    public sealed class AssetGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetGroupCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static FAM.Domain.Entities.AssetGroup BuildEntity(
            string code = "AG001",
            string name = "Test Group",
            decimal pct = 10m) =>
            new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                GroupPercentage = pct,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_P1", "Vehicles", 15m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetGroup.FirstAsync(x => x.Id == newId);
            saved.Code.Should().Be("AG_P1");
            saved.GroupName.Should().Be("Vehicles");
            saved.GroupPercentage.Should().Be(15m);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var firstId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_S1"));
            var secondId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_S2"));
            ctx.ChangeTracker.Clear();

            var first = await ctx.AssetGroup.FirstAsync(x => x.Id == firstId);
            var second = await ctx.AssetGroup.FirstAsync(x => x.Id == secondId);
            second.SortOrder.Should().Be(first.SortOrder + 1);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetGroup.FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_U1", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetGroup
            {
                Id = newId,
                GroupName = "Updated",
                SortOrder = 5,
                GroupPercentage = 12m,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_U2", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetGroup
            {
                Id = newId,
                GroupName = "RenamedGroup",
                SortOrder = 9,
                GroupPercentage = 20m,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetGroup.FirstAsync(x => x.Id == newId);
            saved.GroupName.Should().Be("RenamedGroup");
            saved.GroupPercentage.Should().Be(20m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, BuildEntity());

            result.Should().Be(-1);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_D1"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetGroup.IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepository(ctx).CreateAsync(BuildEntity("AG_EX1"));

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("AG_EX1");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepository(ctx).ExistsByCodeAsync("AG_NONE");

            exists.Should().BeFalse();
        }

        // --- CheckForDuplicatesAsync ---

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_And_Percentage_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepository(ctx).CreateAsync(BuildEntity("AG_DUP1", "DupName", 25m));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0, 25m);

            nameDup.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_SortOrder_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("AG_DUP2"));
            ctx.ChangeTracker.Clear();

            var existingSortOrder = (await ctx.AssetGroup.FirstAsync(x => x.Id == newId)).SortOrder;

            var (_, sortDup) = await CreateRepository(ctx).CheckForDuplicatesAsync("Other", existingSortOrder, 0, 1m);

            sortDup.Should().BeTrue();
        }
    }
}
