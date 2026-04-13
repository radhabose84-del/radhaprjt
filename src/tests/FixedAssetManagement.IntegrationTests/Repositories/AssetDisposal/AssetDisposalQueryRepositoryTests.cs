using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetDisposal;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetDisposal
{
    [Collection("DatabaseCollection")]
    public sealed class AssetDisposalQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetDisposalQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetDisposalQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetDisposalQueryRepository(conn);
        }

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
                AssetId = asset.Id, AssetSourceId = src.Id,
                PoDate = DateTimeOffset.UtcNow, PoNo = 1, PoSno = 1,
                GrnNo = 1, GrnSno = 1, GrnDate = DateTimeOffset.UtcNow,
                VendorCode = "V001", VendorName = "Test Vendor",
                QcCompleted = 'Y', AcceptedQty = 1m, PurchaseValue = 1000m, GrnValue = 1000m,
                ItemCode = "ITM001", ItemName = "Test Item",
                BillNo = "BILL001", BillDate = DateTimeOffset.UtcNow,
                OldUnitId = "U1", PjYear = "2025", PjDocId = "PJ001", PjDocNo = 1
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

        private async Task<int> SeedDisposalAsync(int assetId, int purchaseId, int disposalTypeId, decimal? amount = 500m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetDisposalCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                AssetId = assetId,
                AssetPurchaseId = purchaseId,
                DisposalDate = new DateOnly(2025, 1, 1),
                DisposalType = disposalTypeId,
                DisposalReason = "Test",
                DisposalAmount = amount,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetDisposalAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var (assetId, purchaseId) = await SeedAssetAndPurchaseAsync("ADQ_1");
            var dispTypeId = await SeedMiscMasterAsync("DQ_DT1");
            await SeedDisposalAsync(assetId, purchaseId, dispTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAssetDisposalAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetDisposalAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAssetDisposalAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match_When_AssetId_Found()
        {
            await ClearTablesAsync();
            var (assetId, purchaseId) = await SeedAssetAndPurchaseAsync("ADQ_2");
            var dispTypeId = await SeedMiscMasterAsync("DQ_DT2");
            await SeedDisposalAsync(assetId, purchaseId, dispTypeId, amount: 999m);

            var result = await CreateQueryRepo().GetByIdAsync(assetId);

            result.Should().NotBeNull();
            result!.DisposalAmount.Should().Be(999m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDisposalType_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetDisposalType();

            result.Should().BeEmpty();
        }
    }
}
