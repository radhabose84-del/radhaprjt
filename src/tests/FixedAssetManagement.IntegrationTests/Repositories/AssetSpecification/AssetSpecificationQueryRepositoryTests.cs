using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetSpecification;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSpecification
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSpecificationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSpecificationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSpecificationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetSpecificationQueryRepository(conn);
        }

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

        private async Task SeedSpecAsync(int assetId, int specId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetSpecificationCommandRepository(ctx).CreateAsync(new AssetSpecifications
            {
                AssetId = assetId,
                SpecificationId = specId,
                SpecificationValue = "Val",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetSpecificationAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASPQ_1");
            await SeedSpecAsync(assetId, specId);

            var (items, total) = await CreateQueryRepo().GetAllAssetSpecificationAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAssetSpecificationAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAssetSpecificationAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASPQ_2");
            await SeedSpecAsync(assetId, specId);

            var result = await CreateQueryRepo().GetByIdAsync(assetId);

            result.AssetId.Should().Be(assetId);
            result.Specifications.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_AssetEntry_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().NotBeNull();
            result.Specifications.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByAssetSpecificationNameAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var (assetId, specId) = await SeedAssetAndSpecMasterAsync("ASPQ_3");
            await SeedSpecAsync(assetId, specId);

            var result = await CreateQueryRepo().GetByAssetSpecificationNameAsync("Asset");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAssetSpecBasedOnMachineNos_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var (items, _) = await CreateQueryRepo().GetAssetSpecBasedOnMachineNos(1, 10, null);

            items.Should().BeEmpty();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().SoftDeleteValidation(9999)).Should().BeFalse();
        }
    }
}
