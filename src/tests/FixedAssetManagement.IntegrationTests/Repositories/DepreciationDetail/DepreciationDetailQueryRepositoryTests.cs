using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;
using FAM.Infrastructure.Repositories.DepreciationDetail;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.DepreciationDetail
{
    [Collection("DatabaseCollection")]
    public sealed class DepreciationDetailQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepreciationDetailQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepreciationDetailQueryRepository CreateQueryRepo(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            return new DepreciationDetailQueryRepository(conn, ctx, ipMock.Object);
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

        private async Task<int> SeedAssetAsync(int assetGroupId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var catId = await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = code + "C", CategoryName = "Cat", AssetGroupId = assetGroupId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            var subId = await new AssetSubCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubCategories
            {
                Code = code + "SC", SubCategoryName = "Sub", AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
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

        private async Task SeedDepreciationDetailAsync(int companyId, int unitId, int finYear, int depType, int depPeriod, int assetGroupId, int assetId, byte isLocked = 0)
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
                IsLocked = isLocked,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
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

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task ExistDataAsync_Should_Return_True_When_Records_Match()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DDQ_E1");
            var assetId = await SeedAssetAsync(groupId, "DQE1");
            var depTypeId = await SeedMiscMasterAsync("DQ_DT1");
            var depPeriodId = await SeedMiscMasterAsync("DQ_DP1");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataAsync(2025, depTypeId, depPeriodId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistDataAsync_Should_Return_False_When_NoMatch()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataAsync(2025, 1, 1)).Should().BeFalse();
        }

        [Fact]
        public async Task ExistDataLockedAsync_Should_Return_True_When_Locked_Records_Exist()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DDQ_E2");
            var assetId = await SeedAssetAsync(groupId, "DQE2");
            var depTypeId = await SeedMiscMasterAsync("DQ_DT2");
            var depPeriodId = await SeedMiscMasterAsync("DQ_DP2");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId, isLocked: 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataLockedAsync(2025, depTypeId, depPeriodId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistDataLockedAsync_Should_Return_False_When_Records_Unlocked()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DDQ_E3");
            var assetId = await SeedAssetAsync(groupId, "DQE3");
            var depTypeId = await SeedMiscMasterAsync("DQ_DT3");
            var depPeriodId = await SeedMiscMasterAsync("DQ_DP3");
            await SeedDepreciationDetailAsync(1, 1, 2025, depTypeId, depPeriodId, groupId, assetId, isLocked: 0);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataLockedAsync(2025, depTypeId, depPeriodId)).Should().BeFalse();
        }

        [Fact]
        public async Task GetDepreciationMethodAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateQueryRepo(ctx).GetDepreciationMethodAsync();

            result.Should().BeEmpty();
        }
    }
}
