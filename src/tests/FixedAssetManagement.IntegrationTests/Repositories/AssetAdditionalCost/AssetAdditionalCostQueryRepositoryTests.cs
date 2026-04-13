using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetAdditionalCost;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetAdditionalCost
{
    [Collection("DatabaseCollection")]
    public sealed class AssetAdditionalCostQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetAdditionalCostQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetAdditionalCostQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetAdditionalCostQueryRepository(conn);
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

        private async Task<(int assetId, int sourceId)> SeedAssetAndSourceAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new FAM.Domain.Entities.AssetGroup
            {
                Code = "AACQ_G", GroupName = "G", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetGroup.Add(group);
            await ctx.SaveChangesAsync();

            var cat = new FAM.Domain.Entities.AssetCategories
            {
                Code = "AACQ_C", CategoryName = "C", AssetGroupId = group.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetCategories.Add(cat);
            await ctx.SaveChangesAsync();

            var subCat = new FAM.Domain.Entities.AssetSubCategories
            {
                Code = "AACQ_SC", SubCategoryName = "SC", AssetCategoriesId = cat.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSubCategories.Add(subCat);
            await ctx.SaveChangesAsync();

            var asset = new AssetMasterGenerals
            {
                CompanyId = 1, UnitId = 1,
                AssetCode = "AACQ_AM", AssetName = "Asset",
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
                SourceCode = "SRC_AACQ", SourceName = "Source",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSource.Add(src);
            await ctx.SaveChangesAsync();

            return (asset.Id, src.Id);
        }

        private async Task<int> SeedEntityAsync(int assetId, int sourceId, int costTypeId, decimal amount = 1500m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetAdditionalCostCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                AssetId = assetId,
                AssetSourceId = sourceId,
                CostType = costTypeId,
                Amount = amount,
                JournalNo = "JN_Q"
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAdditionalCostGroupAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var costTypeId = await SeedMiscMasterAsync("AACQ_CT1");
            var (assetId, sourceId) = await SeedAssetAndSourceAsync();
            await SeedEntityAsync(assetId, sourceId, costTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAdditionalCostGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAdditionalCostGroupAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAdditionalCostGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match_When_AssetId_Found()
        {
            await ClearTablesAsync();
            var costTypeId = await SeedMiscMasterAsync("AACQ_CT2");
            var (assetId, sourceId) = await SeedAssetAndSourceAsync();
            await SeedEntityAsync(assetId, sourceId, costTypeId, 999m);

            var result = await CreateQueryRepo().GetByIdAsync(assetId);

            result.Should().NotBeNull();
            result!.Amount.Should().Be(999m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCostTypeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetCostTypeAsync();

            result.Should().BeEmpty();
        }
    }
}
