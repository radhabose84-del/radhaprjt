using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetDisposal;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetDisposal
{
    [Collection("DatabaseCollection")]
    public sealed class AssetDisposalCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetDisposalCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetDisposalCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<(int assetId, int purchaseId)> SeedAssetAndPurchaseAsync(string codePrefix)
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

            var purchase = new AssetPurchaseDetails
            {
                AssetId = asset.Id,
                AssetSourceId = src.Id,
                PoDate = DateTimeOffset.UtcNow,
                PoNo = 1,
                PoSno = 1,
                GrnNo = 1,
                GrnSno = 1,
                GrnDate = DateTimeOffset.UtcNow,
                QcCompleted = 'Y',
                AcceptedQty = 1m,
                PurchaseValue = 1000m,
                GrnValue = 1000m,
                VendorCode = "V001",
                VendorName = "Test Vendor",
                ItemCode = "ITM001",
                ItemName = "Test Item",
                BillNo = "BILL001",
                BillDate = DateTimeOffset.UtcNow,
                OldUnitId = "U1",
                PjYear = "2025", PjDocId = "PJ001", PjDocNo = 1
            };
            ctx.AssetPurchaseDetails.Add(purchase);
            await ctx.SaveChangesAsync();

            return (asset.Id, purchase.Id);
        }

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode, Description = "T",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = "MM_" + typeCode, Description = "M",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, purchaseId) = await SeedAssetAndPurchaseAsync("ADP_C1");
            var dispTypeId = await SeedMiscMasterAsync("DIS_DT_C1");

            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                AssetId = assetId,
                AssetPurchaseId = purchaseId,
                DisposalDate = new DateOnly(2025, 6, 15),
                DisposalType = dispTypeId,
                DisposalReason = "Test",
                DisposalAmount = 500m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, purchaseId) = await SeedAssetAndPurchaseAsync("ADP_C2");
            var dispTypeId = await SeedMiscMasterAsync("DIS_DT_C2");

            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                AssetId = assetId,
                AssetPurchaseId = purchaseId,
                DisposalDate = new DateOnly(2025, 6, 15),
                DisposalType = dispTypeId,
                DisposalReason = "Sold",
                DisposalAmount = 750m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetDisposal.FirstAsync(x => x.Id == newId);
            saved.AssetId.Should().Be(assetId);
            saved.DisposalReason.Should().Be("Sold");
            saved.DisposalAmount.Should().Be(750m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, purchaseId) = await SeedAssetAndPurchaseAsync("ADP_U1");
            var dispTypeId = await SeedMiscMasterAsync("DIS_DT_U1");

            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                AssetId = assetId,
                AssetPurchaseId = purchaseId,
                DisposalDate = new DateOnly(2025, 1, 1),
                DisposalType = dispTypeId,
                DisposalReason = "Original",
                DisposalAmount = 100m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                DisposalDate = new DateOnly(2025, 9, 9),
                DisposalType = dispTypeId,
                DisposalReason = "Updated",
                DisposalAmount = 999m
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var updated = await ctx.AssetDisposal.FirstAsync(x => x.Id == newId);
            updated.DisposalReason.Should().Be("Updated");
            updated.DisposalAmount.Should().Be(999m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                DisposalReason = "X"
            });

            result.Should().Be(-1);
        }
    }
}
