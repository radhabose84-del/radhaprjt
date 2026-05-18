using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetInsurance;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetInsurance
{
    [Collection("DatabaseCollection")]
    public sealed class AssetInsuranceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetInsuranceQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetInsuranceQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetInsuranceQueryRepository(conn);
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

        private async Task<int> SeedInsuranceAsync(int assetId, string policyNo = "POL_Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new AssetInsuranceCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetInsurance
            {
                AssetId = assetId,
                PolicyNo = policyNo,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                Insuranceperiod = 12,
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                PolicyAmount = 50000m,
                VendorCode = "VC_Q",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 1, 1),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetInsuranceAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_1");
            await SeedInsuranceAsync(assetId);

            var (items, total) = await CreateQueryRepo().GetAllAssetInsuranceAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetInsuranceAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_2");
            await SeedInsuranceAsync(assetId, "POL_ALPHA");

            var (items, _) = await CreateQueryRepo().GetAllAssetInsuranceAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByAssetIdAsync_Should_Return_Match_When_Found()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_3");
            var insId = await SeedInsuranceAsync(assetId, "POL_BYID");

            var result = await CreateQueryRepo().GetByAssetIdAsync(insId);

            result.PolicyNo.Should().Be("POL_BYID");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Found()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_4");
            await SeedInsuranceAsync(assetId, "POL_EX");

            (await CreateQueryRepo().AlreadyExistsAsync("POL_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Excluded()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_5");
            var insId = await SeedInsuranceAsync(assetId, "POL_EX2");

            (await CreateQueryRepo().AlreadyExistsAsync("POL_EX2", insId)).Should().BeFalse();
        }

        [Fact]
        public async Task ActiveInsuranceValidation_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AINSQ_6");
            await SeedInsuranceAsync(assetId);

            (await CreateQueryRepo().ActiveInsuranceValidation(assetId)).Should().BeTrue();
        }

        [Fact]
        public async Task ActiveInsuranceValidation_Should_Return_False_When_NoMatch()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().ActiveInsuranceValidation(9999)).Should().BeFalse();
        }
    }
}
