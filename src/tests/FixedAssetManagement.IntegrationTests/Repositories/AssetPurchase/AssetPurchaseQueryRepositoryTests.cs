using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetPurchase;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetPurchase
{
    [Collection("DatabaseCollection")]
    public sealed class AssetPurchaseQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetPurchaseQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetPurchaseQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetPurchaseQueryRepository(conn);
        }

        private async Task<(int assetId, int sourceId)> SeedAssetAndSourceAsync(string codePrefix)
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

            var src = new AssetSource
            {
                SourceCode = codePrefix + "_SRC", SourceName = "Source",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSource.Add(src);
            await ctx.SaveChangesAsync();

            return (asset.Id, src.Id);
        }

        private async Task<int> SeedPurchaseAsync(int assetId, int sourceId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetPurchaseCommandRepository(ctx).CreateAsync(new AssetPurchaseDetails
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                OldUnitId = "U1",
                VendorCode = "V001",
                VendorName = "Test Vendor",
                PoDate = DateTimeOffset.UtcNow,
                PoNo = 1, PoSno = 1,
                ItemCode = "ITM001",
                ItemName = "Test Item",
                GrnNo = 100, GrnSno = 1,
                GrnDate = DateTimeOffset.UtcNow,
                QcCompleted = 'Y',
                AcceptedQty = 1m,
                PurchaseValue = 1000m,
                GrnValue = 1000m,
                BillNo = "B001",
                BillDate = DateTimeOffset.UtcNow,
                PjYear = "2025",
                PjDocId = "DOC001",
                PjDocNo = 1
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match_When_AssetId_Found()
        {
            await ClearTablesAsync();
            var (assetId, sourceId) = await SeedAssetAndSourceAsync("APDQ_1");
            await SeedPurchaseAsync(assetId, sourceId);

            var result = await CreateQueryRepo().GetByIdAsync(assetId);

            result.Should().NotBeNull();
            result!.AssetId.Should().Be(assetId);
            result.PurchaseValue.Should().Be(1000m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetSources_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var (_, _) = await SeedAssetAndSourceAsync("APDQ_2");

            var result = await CreateQueryRepo().GetAssetSources("Source");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAssetSources_Should_Return_Empty_When_NoMatch()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAssetSources("ZZZNoSuch");

            result.Should().BeEmpty();
        }
    }
}
