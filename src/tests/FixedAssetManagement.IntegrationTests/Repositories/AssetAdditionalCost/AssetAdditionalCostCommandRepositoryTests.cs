using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.Infrastructure.Repositories.AssetMaster.AssetAdditionalCost;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetAdditionalCost
{
    [Collection("DatabaseCollection")]
    public sealed class AssetAdditionalCostCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetAdditionalCostCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetAdditionalCostCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private async Task<(int assetId, int sourceId)> SeedAssetAndSourceAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new FAM.Domain.Entities.AssetGroup
            {
                Code = "AAC_G", GroupName = "G", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetGroup.Add(group);
            await ctx.SaveChangesAsync();

            var cat = new FAM.Domain.Entities.AssetCategories
            {
                Code = "AAC_C", CategoryName = "C", AssetGroupId = group.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetCategories.Add(cat);
            await ctx.SaveChangesAsync();

            var subCat = new FAM.Domain.Entities.AssetSubCategories
            {
                Code = "AAC_SC", SubCategoryName = "SC", AssetCategoriesId = cat.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSubCategories.Add(subCat);
            await ctx.SaveChangesAsync();

            var asset = new AssetMasterGenerals
            {
                CompanyId = 1, UnitId = 1,
                AssetCode = "AAC_AM", AssetName = "Test Asset",
                AssetGroupId = group.Id,
                AssetCategoryId = cat.Id,
                AssetSubCategoryId = subCat.Id,
                Quantity = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetMasterGenerals.Add(asset);
            await ctx.SaveChangesAsync();

            var src = new AssetSource
            {
                SourceCode = "SRC_AAC", SourceName = "Source",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSource.Add(src);
            await ctx.SaveChangesAsync();

            return (asset.Id, src.Id);
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var costTypeId = await SeedMiscMasterAsync("AAC_CT_C1");
            var (assetId, sourceId) = await SeedAssetAndSourceAsync();

            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                CostType = costTypeId,
                Amount = 1500m,
                JournalNo = "JN001"
            });

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var costTypeId = await SeedMiscMasterAsync("AAC_CT_C2");
            var (assetId, sourceId) = await SeedAssetAndSourceAsync();

            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                CostType = costTypeId,
                Amount = 2500m,
                JournalNo = "JN002"
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetAdditionalCost.FirstAsync(x => x.Id == newId);
            saved.Amount.Should().Be(2500m);
            saved.JournalNo.Should().Be("JN002");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var costTypeId = await SeedMiscMasterAsync("AAC_CT_U1");
            var (assetId, sourceId) = await SeedAssetAndSourceAsync();
            var newId = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                CostType = costTypeId,
                Amount = 100m,
                JournalNo = "ORIG"
            });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                CostType = costTypeId,
                Amount = 999m,
                JournalNo = "UPDATED"
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var updated = await ctx.AssetAdditionalCost.FirstAsync(x => x.Id == newId);
            updated.Amount.Should().Be(999m);
            updated.JournalNo.Should().Be("UPDATED");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                Amount = 100m,
                JournalNo = "X"
            });

            result.Should().Be(-1);
        }
    }
}
