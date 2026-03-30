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
            string name = "Test Asset Group",
            int sortOrder = 1,
            decimal pct = 10m) =>
            new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                SortOrder = sortOrder,
                GroupPercentage = pct,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetSubCategories]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetCategories]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetGroup]");
        }

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
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("BLDG001", "Buildings", 1, 5m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("BLDG001");
            saved.GroupName.Should().Be("Buildings");
            saved.GroupPercentage.Should().Be(5m);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("UPD001", "Original Group"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new FAM.Domain.Entities.AssetGroup
            {
                Id = newId,
                Code = "UPD001",
                GroupName = "Updated Group",
                SortOrder = 1,
                GroupPercentage = 15m,
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(newId, toUpdate);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("UPD002", "Original Name"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetGroup
            {
                Id = newId,
                Code = "UPD002",
                GroupName = "Updated Name",
                SortOrder = 2,
                GroupPercentage = 20m,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.AssetGroup.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.GroupName.Should().Be("Updated Name");
            updated.GroupPercentage.Should().Be(20m);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Value_GreaterThanZero_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("DEL001", "Delete Group"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new FAM.Domain.Entities.AssetGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
