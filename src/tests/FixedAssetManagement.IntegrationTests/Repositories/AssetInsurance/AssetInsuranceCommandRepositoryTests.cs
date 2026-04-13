using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetInsurance;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetInsurance
{
    [Collection("DatabaseCollection")]
    public sealed class AssetInsuranceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetInsuranceCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetInsuranceCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

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

        private static FAM.Domain.Entities.AssetMaster.AssetInsurance BuildEntity(int assetId, string policyNo = "POL001") =>
            new FAM.Domain.Entities.AssetMaster.AssetInsurance
            {
                AssetId = assetId,
                PolicyNo = policyNo,
                StartDate = new DateOnly(2025, 1, 1),
                Insuranceperiod = 12,
                EndDate = new DateOnly(2026, 1, 1),
                PolicyAmount = 50000m,
                VendorCode = "VC001",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 1, 1),
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
            var assetId = await SeedAssetAsync("AINS_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetId = await SeedAssetAsync("AINS_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, "POL_PERSIST"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetInsurance.FirstAsync(x => x.Id == result.Id);
            saved.PolicyNo.Should().Be("POL_PERSIST");
            saved.AssetId.Should().Be(assetId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetId = await SeedAssetAsync("AINS_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, "POL_ORIG"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(created.Id, new FAM.Domain.Entities.AssetMaster.AssetInsurance
            {
                PolicyNo = "POL_NEW",
                StartDate = new DateOnly(2025, 6, 1),
                Insuranceperiod = 12,
                EndDate = new DateOnly(2026, 6, 1),
                PolicyAmount = 99000m,
                VendorCode = "VC_NEW",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 6, 1),
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            (await ctx.AssetInsurance.FirstAsync(x => x.Id == created.Id)).PolicyNo.Should().Be("POL_NEW");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetInsurance { PolicyNo = "X" });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetId = await SeedAssetAsync("AINS_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new FAM.Domain.Entities.AssetMaster.AssetInsurance { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetInsurance.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetInsurance { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAlreadyAsync_Should_Return_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var assetId = await SeedAssetAsync("AINS_G1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, "POL_FIND"));

            var result = await CreateRepository(ctx).GetAlreadyAsync(x => x.PolicyNo == "POL_FIND");

            result.Should().NotBeNull();
        }
    }
}
