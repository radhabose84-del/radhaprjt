using Contracts.Interfaces;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;
using FAM.Infrastructure.Repositories.DepreciationDetail;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetManagement.IntegrationTests.Repositories.DepreciationDetail
{
    [Collection("DatabaseCollection")]
    public sealed class DepreciationDetailCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepreciationDetailCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepreciationDetailCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            return new DepreciationDetailCommandRepository(ctx, ipMock.Object);
        }

        private async Task<int> SeedAssetGroupAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "G " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
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

        private async Task<int> SeedAssetAsync(int assetGroupId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var catId = (await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code + "C", CategoryName = "Cat", AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            }));
            var subId = (await new AssetSubCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code + "SC", SubCategoryName = "Sub", AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            }));
            var asset = new AssetMasterGenerals
            {
                CompanyId = 1, UnitId = 1,
                AssetCode = code + "A", AssetName = "Asset",
                AssetGroupId = assetGroupId, AssetCategoryId = catId, AssetSubCategoryId = subId,
                Quantity = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetMasterGenerals.Add(asset);
            await ctx.SaveChangesAsync();
            return asset.Id;
        }

        private async Task SeedDepreciationDetailAsync(int companyId, int unitId, int finYear, int depType, int depPeriod, int assetGroupId, int assetId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.DepreciationDetails.Add(new DepreciationDetails
            {
                CompanyId = companyId,
                UnitId = unitId,
                Finyear = finYear,
                StartDate = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2026, 3, 31, 0, 0, 0, TimeSpan.Zero),
                DepreciationType = depType,
                DepreciationPeriod = depPeriod,
                AssetGroupId = assetGroupId,
                AssetId = assetId,
                AssetValue = 1000m,
                CapitalizationDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ResidualValue = 100m,
                ExpiryDate = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero),
                UsefulLifeDays = 365,
                DaysOpening = 0,
                DaysUsed = 30,
                OpeningValue = 1000m,
                DepreciationValue = 80m,
                ClosingValue = 920m,
                NetValue = 920m,
                IsLocked = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task DeleteAsync_Should_Remove_Matching_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DD_C1");
            var assetId = await SeedAssetAsync(groupId, "DDC1");
            var depTypeId = await SeedMiscMasterAsync("DD_DT1");
            var depPeriodId = await SeedMiscMasterAsync("DD_DP1");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId);

            var result = await CreateRepository(ctx).DeleteAsync(2025, depTypeId, depPeriodId);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_No_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, 1, 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Set_IsLocked_To_One()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DD_U1");
            var assetId = await SeedAssetAsync(groupId, "DDU1");
            var depTypeId = await SeedMiscMasterAsync("DD_DT2");
            var depPeriodId = await SeedMiscMasterAsync("DD_DP2");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId);

            var result = await CreateRepository(ctx).UpdateAsync(2025, depTypeId, depPeriodId);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.DepreciationDetails.FirstAsync();
            saved.IsLocked.Should().Be((byte)1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_No_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, 1, 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Only_Affect_Current_Company()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DD_C2");
            var assetId = await SeedAssetAsync(groupId, "DDC2");
            var depTypeId = await SeedMiscMasterAsync("DD_DT3");
            var depPeriodId = await SeedMiscMasterAsync("DD_DP3");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId);
            await SeedDepreciationDetailAsync(2, 1, 2025, depTypeId, depPeriodId, groupId, assetId);

            await CreateRepository(ctx).DeleteAsync(2025, depTypeId, depPeriodId);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.DepreciationDetails.CountAsync();
            remaining.Should().Be(1);
        }
    }
}
