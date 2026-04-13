using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetAmc;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetAmc
{
    [Collection("DatabaseCollection")]
    public sealed class AssetAmcQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetAmcQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetAmcQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetAmcQueryRepository(conn);
        }

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "T",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "M",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedAssetAsync(string codePrefix)
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
            return asset.Id;
        }

        private async Task<int> SeedAmcAsync(int assetId, int coverageTypeId, int renewalStatusId, string vendorName = "VendorQ")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetAmcCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetAmc
            {
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2026, 1, 1),
                Period = 12,
                VendorCode = "VC_Q",
                VendorName = vendorName,
                CoverageType = coverageTypeId,
                RenewalStatus = renewalStatusId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetAmcAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var covId = await SeedMiscMasterAsync("AMCQ_COV1");
            var renId = await SeedMiscMasterAsync("AMCQ_REN1");
            var assetId = await SeedAssetAsync("AMCQ_1");
            await SeedAmcAsync(assetId, covId, renId);

            var (items, total) = await CreateQueryRepo().GetAllAssetAmcAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetAmcAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var covId = await SeedMiscMasterAsync("AMCQ_COV2");
            var renId = await SeedMiscMasterAsync("AMCQ_REN2");
            var assetId = await SeedAssetAsync("AMCQ_2");
            await SeedAmcAsync(assetId, covId, renId, "Alpha");

            var (items, _) = await CreateQueryRepo().GetAllAssetAmcAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match_When_Found()
        {
            await ClearTablesAsync();
            var covId = await SeedMiscMasterAsync("AMCQ_COV3");
            var renId = await SeedMiscMasterAsync("AMCQ_REN3");
            var assetId = await SeedAssetAsync("AMCQ_3");
            var id = await SeedAmcAsync(assetId, covId, renId, "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.VendorName.Should().Be("ById");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCoverageScopeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetCoverageScopeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRenewStatusAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetRenewStatusAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ActiveAMCValidation_Should_Return_True_When_Active_Amc_Exists()
        {
            await ClearTablesAsync();
            var covId = await SeedMiscMasterAsync("AMCQ_COV4");
            var renId = await SeedMiscMasterAsync("AMCQ_REN4");
            var assetId = await SeedAssetAsync("AMCQ_4");
            await SeedAmcAsync(assetId, covId, renId);

            (await CreateQueryRepo().ActiveAMCValidation(assetId)).Should().BeTrue();
        }

        [Fact]
        public async Task ActiveAMCValidation_Should_Return_False_When_NoMatch()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().ActiveAMCValidation(9999)).Should().BeFalse();
        }
    }
}
