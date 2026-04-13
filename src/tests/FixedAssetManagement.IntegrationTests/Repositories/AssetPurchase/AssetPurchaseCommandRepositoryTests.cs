using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetPurchase;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetPurchase
{
    [Collection("DatabaseCollection")]
    public sealed class AssetPurchaseCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetPurchaseCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetPurchaseCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

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

        private static AssetPurchaseDetails BuildEntity(int assetId, int sourceId, string poNo = "PO_001") =>
            new AssetPurchaseDetails
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                BudgetType = "Capital",
                OldUnitId = "U1",
                VendorCode = "V001",
                VendorName = "Test Vendor",
                PoDate = DateTimeOffset.UtcNow,
                PoNo = 1,
                PoSno = 1,
                ItemCode = "ITM001",
                ItemName = "Test Item",
                GrnNo = 100,
                GrnSno = 1,
                GrnDate = DateTimeOffset.UtcNow,
                QcCompleted = 'Y',
                AcceptedQty = 1m,
                PurchaseValue = 1000m,
                GrnValue = 1000m,
                BillNo = "B001",
                BillDate = DateTimeOffset.UtcNow,
                Uom = "PCS",
                PjYear = "2025",
                PjDocId = "DOC001",
                PjDocNo = 1
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, sourceId) = await SeedAssetAndSourceAsync("APD_C1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, sourceId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, sourceId) = await SeedAssetAndSourceAsync("APD_C2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, sourceId, "PO_PERSIST"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetPurchaseDetails.FirstAsync(x => x.Id == newId);
            saved.AssetId.Should().Be(assetId);
            saved.PurchaseValue.Should().Be(1000m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, sourceId) = await SeedAssetAndSourceAsync("APD_U1");
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, sourceId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new AssetPurchaseDetails
            {
                BudgetType = "Operational",
                VendorCode = "V_NEW",
                VendorName = "New Vendor",
                PoDate = DateTimeOffset.UtcNow,
                PoNo = 2, PoSno = 2,
                GrnNo = 200, GrnSno = 1,
                GrnDate = DateTimeOffset.UtcNow,
                QcCompleted = 'Y',
                AcceptedQty = 5m,
                PurchaseValue = 5000m,
                GrnValue = 5000m,
                ItemCode = "ITM002",
                ItemName = "Updated Item",
                BillNo = "UPD-BILL",
                BillDate = DateTimeOffset.UtcNow,
                OldUnitId = "U1",
                PjYear = "2025", PjDocId = "PJ001", PjDocNo = 1
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var updated = await ctx.AssetPurchaseDetails.FirstAsync(x => x.Id == newId);
            updated.VendorName.Should().Be("New Vendor");
            updated.PurchaseValue.Should().Be(5000m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new AssetPurchaseDetails
            {
                PoDate = DateTimeOffset.UtcNow,
                GrnDate = DateTimeOffset.UtcNow,
                BillDate = DateTimeOffset.UtcNow
            });

            result.Should().Be(-1);
        }
    }
}
