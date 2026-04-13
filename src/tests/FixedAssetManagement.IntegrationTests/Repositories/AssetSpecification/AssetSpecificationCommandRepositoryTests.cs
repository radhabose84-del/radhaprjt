using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetSpecification;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSpecification
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSpecificationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSpecificationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSpecificationCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<(int assetId, int specMasterId)> SeedAssetAndSpecMasterAsync(string codePrefix)
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

            var asset = new AssetMasterGenerals
            {
                CompanyId = 1, UnitId = 1,
                AssetCode = codePrefix + "_AM", AssetName = "Asset",
                AssetGroupId = group.Id,
                AssetCategoryId = cat.Id,
                AssetSubCategoryId = sub.Id,
                Quantity = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetMasterGenerals.Add(asset);
            await ctx.SaveChangesAsync();

            var specMaster = new SpecificationMasters
            {
                SpecificationName = "TestSpec",
                AssetGroupId = group.Id,
                ISDefault = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.SpecificationMasters.Add(specMaster);
            await ctx.SaveChangesAsync();

            return (asset.Id, specMaster.Id);
        }

        private static AssetSpecifications BuildEntity(int assetId, int specId, string value = "TestValue") =>
            new AssetSpecifications
            {
                AssetId = assetId,
                SpecificationId = specId,
                SpecificationValue = value,
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
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASP_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, specId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASP_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, specId, "Length=100"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetSpecifications.FirstAsync(x => x.Id == result.Id);
            saved.AssetId.Should().Be(assetId);
            saved.SpecificationValue.Should().Be("Length=100");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASP_U1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, specId, "OriginalValue"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(assetId, new AssetSpecifications
            {
                SpecificationId = specId,
                SpecificationValue = "NewValue",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            result.Should().BeGreaterThan(0);
            var updated = await ctx.AssetSpecifications.FirstAsync(x => x.AssetId == assetId);
            updated.SpecificationValue.Should().Be("NewValue");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new AssetSpecifications
            {
                SpecificationId = 1,
                SpecificationValue = "X"
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_All_Asset_Specs()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASP_D1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, specId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(assetId, new AssetSpecifications());

            result.Should().BeGreaterThan(0);
            (await ctx.AssetSpecifications.CountAsync(x => x.AssetId == assetId)).Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new AssetSpecifications());

            result.Should().Be(-1);
        }

        [Fact]
        public async Task ExistsByAssetSpecIdAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASP_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, specId));

            (await CreateRepository(ctx).ExistsByAssetSpecIdAsync(assetId, specId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByManufactureAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            (await CreateRepository(ctx).ExistsByManufactureAsync(9999)).Should().BeFalse();
        }
    }
}
