using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetMasterGeneral;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetMasterGeneral
{
    [Collection("DatabaseCollection")]
    public sealed class AssetMasterGeneralCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetMasterGeneralCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetMasterGeneralCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<(int groupId, int catId, int subCatId)> SeedFkChainAsync(string codePrefix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new FAM.Domain.Entities.AssetGroup
            {
                Code = codePrefix + "_G", GroupName = "G", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetGroup.Add(group);
            await ctx.SaveChangesAsync();

            var cat = new FAM.Domain.Entities.AssetCategories
            {
                Code = codePrefix + "_C", CategoryName = "C", AssetGroupId = group.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetCategories.Add(cat);
            await ctx.SaveChangesAsync();

            var sub = new FAM.Domain.Entities.AssetSubCategories
            {
                Code = codePrefix + "_SC", SubCategoryName = "SC", AssetCategoriesId = cat.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSubCategories.Add(sub);
            await ctx.SaveChangesAsync();

            return (group.Id, cat.Id, sub.Id);
        }

        private static AssetMasterGenerals BuildEntity(int groupId, int catId, int subCatId, string assetCode = "AM001", string assetName = "Test Asset") =>
            new AssetMasterGenerals
            {
                CompanyId = 1,
                UnitId = 1,
                AssetCode = assetCode,
                AssetName = assetName,
                AssetGroupId = groupId,
                AssetCategoryId = catId,
                AssetSubCategoryId = subCatId,
                Quantity = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, catId, subCatId) = await SeedFkChainAsync("AMG_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, catId, subCatId), CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, catId, subCatId) = await SeedFkChainAsync("AMG_C2");

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(groupId, catId, subCatId, "AMG_P", "PersistMe"),
                CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetMasterGenerals.FirstAsync(x => x.Id == result.Id);
            saved.AssetCode.Should().Be("AMG_P");
            saved.AssetName.Should().Be("PersistMe");
            saved.AssetGroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, catId, subCatId) = await SeedFkChainAsync("AMG_C3");

            var result = await CreateRepository(ctx).CreateAsync(
                BuildEntity(groupId, catId, subCatId),
                CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetMasterGenerals.FirstAsync(x => x.Id == result.Id);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, catId, subCatId) = await SeedFkChainAsync("AMG_D1");
            var created = await CreateRepository(ctx).CreateAsync(
                BuildEntity(groupId, catId, subCatId),
                CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id,
                new AssetMasterGenerals { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetMasterGenerals.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new AssetMasterGenerals { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new AssetMasterGenerals
            {
                AssetName = "X",
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1
            });

            result.Should().BeFalse();
        }
    }
}
