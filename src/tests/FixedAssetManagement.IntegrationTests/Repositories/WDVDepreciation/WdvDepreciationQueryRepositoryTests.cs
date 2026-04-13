using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.WDVDepreciation;

namespace FixedAssetManagement.IntegrationTests.Repositories.WDVDepreciation
{
    [Collection("DatabaseCollection")]
    public sealed class WdvDepreciationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WdvDepreciationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WdvDepreciationQueryRepository CreateQueryRepo(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            return new WdvDepreciationQueryRepository(conn, ctx, ipMock.Object);
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

        private async Task SeedWdvAsync(int companyId, int finYear, int groupId, byte isLocked = 0)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.WDVDepreciationDetail.Add(new WDVDepreciationDetail
            {
                CompanyId = companyId,
                FinYear = finYear,
                AssetGroupId = groupId,
                DepreciationPercentage = 10m,
                OpeningValue = 1000m,
                ClosingValue = 900m,
                StartDate = DateTimeOffset.UtcNow.AddYears(-1),
                EndDate = DateTimeOffset.UtcNow,
                IsLocked = isLocked,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task ExistDataAsync_Should_Return_True_When_Records_Match()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("WDVQ_E1");
            await SeedWdvAsync(1, 2025, groupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataAsync(2025)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistDataAsync_Should_Return_False_When_NoMatch()
        {
            await ClearTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataAsync(2025)).Should().BeFalse();
        }

        [Fact]
        public async Task ExistDataLockedAsync_Should_Return_True_When_Locked()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("WDVQ_E2");
            await SeedWdvAsync(1, 2025, groupId, isLocked: 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataLockedAsync(2025)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistDataLockedAsync_Should_Return_False_When_Unlocked()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("WDVQ_E3");
            await SeedWdvAsync(1, 2025, groupId, isLocked: 0);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataLockedAsync(2025)).Should().BeFalse();
        }

        [Fact]
        public async Task ExistDataAsync_Should_Be_Scoped_To_CompanyId()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("WDVQ_E4");
            await SeedWdvAsync(2, 2025, groupId); // different company

            await using var ctx = _fixture.CreateFreshDbContext();
            (await CreateQueryRepo(ctx).ExistDataAsync(2025)).Should().BeFalse();
        }
    }
}
