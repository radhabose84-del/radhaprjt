using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetAmc;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetAmc
{
    [Collection("DatabaseCollection")]
    public sealed class AssetAmcCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetAmcCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetAmcCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

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

        private async Task<int> SeedAssetAsync(string codePrefix = "AMC")
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

        private static FAM.Domain.Entities.AssetMaster.AssetAmc BuildEntity(int assetId, int coverageTypeId, int renewalStatusId, string vendorName = "Test Vendor") =>
            new FAM.Domain.Entities.AssetMaster.AssetAmc
            {
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2026, 1, 1),
                Period = 12,
                VendorCode = "VC001",
                VendorName = vendorName,
                VendorPhone = "1234567890",
                VendorEmail = "vendor@example.com",
                CoverageType = coverageTypeId,
                RenewalStatus = renewalStatusId,
                FreeServiceCount = 2,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var covId = await SeedMiscMasterAsync("AMC_COV_C1");
            var renId = await SeedMiscMasterAsync("AMC_REN_C1");
            var assetId = await SeedAssetAsync("AMC_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, covId, renId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var covId = await SeedMiscMasterAsync("AMC_COV_C2");
            var renId = await SeedMiscMasterAsync("AMC_REN_C2");
            var assetId = await SeedAssetAsync("AMC_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, covId, renId, "Acme Vendor"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetAmc.FirstAsync(x => x.Id == newId);
            saved.AssetId.Should().Be(assetId);
            saved.VendorName.Should().Be("Acme Vendor");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var covId = await SeedMiscMasterAsync("AMC_COV_U1");
            var renId = await SeedMiscMasterAsync("AMC_REN_U1");
            var assetId = await SeedAssetAsync("AMC_U1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, covId, renId, "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetMaster.AssetAmc
            {
                StartDate = new DateOnly(2025, 6, 1),
                EndDate = new DateOnly(2026, 6, 1),
                Period = 12,
                CoverageType = covId,
                RenewalStatus = renId,
                VendorCode = "VC002",
                VendorName = "Renamed",
                VendorPhone = "9999999999",
                VendorEmail = "new@example.com",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            (await ctx.AssetAmc.FirstAsync(x => x.Id == newId)).VendorName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetAmc { VendorName = "X" });

            result.Should().Be(-1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var covId = await SeedMiscMasterAsync("AMC_COV_D1");
            var renId = await SeedMiscMasterAsync("AMC_REN_D1");
            var assetId = await SeedAssetAsync("AMC_D1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, covId, renId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId, new FAM.Domain.Entities.AssetMaster.AssetAmc { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetAmc.IgnoreQueryFilters().FirstAsync(x => x.Id == newId);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetAmc { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }

        [Fact]
        public async Task GetAlreadyAsync_Should_Return_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var covId = await SeedMiscMasterAsync("AMC_COV_G1");
            var renId = await SeedMiscMasterAsync("AMC_REN_G1");
            var assetId = await SeedAssetAsync("AMC_G1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, covId, renId, "FindMe"));

            var result = await CreateRepository(ctx).GetAlreadyAsync(x => x.AssetId == assetId);

            result.Should().NotBeNull();
            result!.VendorName.Should().Be("FindMe");
        }

        [Fact]
        public async Task GetAlreadyAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetAlreadyAsync(x => x.AssetId == 9999);

            result.Should().BeNull();
        }
    }
}
