using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSpecificationLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSpecificationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSpecificationLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetSpecificationLookupRepository(conn);
        }

        private async Task<(int assetId, int specId)> SeedAssetWithSpecAsync(string codePrefix, string specValue = "Value1")
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
                SpecificationName = codePrefix + "_SPEC",
                AssetGroupId = group.Id,
                ISDefault = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.SpecificationMasters.Add(specMaster);
            await ctx.SaveChangesAsync();

            var assetSpec = new AssetSpecifications
            {
                AssetId = asset.Id,
                SpecificationId = specMaster.Id,
                SpecificationValue = specValue,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSpecifications.Add(assetSpec);
            await ctx.SaveChangesAsync();

            return (asset.Id, specMaster.Id);
        }

        // --- GetAllAssetSpecificationAsync ---

        [Fact]
        public async Task GetAllAssetSpecificationAsync_Should_Return_Seeded_Spec()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAssetWithSpecAsync("ASL_ALL", "Val1");

            var results = await CreateRepo().GetAllAssetSpecificationAsync();

            results.Should().HaveCount(1);
            results[0].SpecificationName.Should().Be("ASL_ALL_SPEC");
            results[0].SpecificationValue.Should().Be("Val1");
        }

        [Fact]
        public async Task GetAllAssetSpecificationAsync_Should_Exclude_SoftDeleted_AssetSpec()
        {
            await _fixture.ClearAllTablesAsync();
            var (assetId, _) = await SeedAssetWithSpecAsync("ASL_SD", "SD");

            await using var ctx = _fixture.CreateFreshDbContext();
            var spec = await ctx.AssetSpecifications.FirstAsync(s => s.AssetId == assetId);
            spec.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateRepo().GetAllAssetSpecificationAsync();

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAssetSpecificationAsync_Should_Return_Empty_When_No_Data()
        {
            await _fixture.ClearAllTablesAsync();

            var results = await CreateRepo().GetAllAssetSpecificationAsync();

            results.Should().BeEmpty();
        }

        // --- GetByAssetIdAsync ---

        [Fact]
        public async Task GetByAssetIdAsync_Should_Return_Matching_Specs()
        {
            await _fixture.ClearAllTablesAsync();
            var (assetId, _) = await SeedAssetWithSpecAsync("ASL_BID", "ByIdVal");

            var results = await CreateRepo().GetByAssetIdAsync(assetId);

            results.Should().HaveCount(1);
            results[0].AssetId.Should().Be(assetId);
            results[0].SpecificationValue.Should().Be("ByIdVal");
        }

        [Fact]
        public async Task GetByAssetIdAsync_Should_Return_Empty_For_Unknown_Asset()
        {
            await _fixture.ClearAllTablesAsync();

            var results = await CreateRepo().GetByAssetIdAsync(999999);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByAssetIdAsync_Should_Exclude_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var (assetId, _) = await SeedAssetWithSpecAsync("ASL_BD", "X");

            await using var ctx = _fixture.CreateFreshDbContext();
            var spec = await ctx.AssetSpecifications.FirstAsync(s => s.AssetId == assetId);
            spec.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var results = await CreateRepo().GetByAssetIdAsync(assetId);

            results.Should().BeEmpty();
        }
    }
}
