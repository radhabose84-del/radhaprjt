using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.WDVDepreciation;

namespace FixedAssetManagement.IntegrationTests.Repositories.WDVDepreciation
{
    [Collection("DatabaseCollection")]
    public sealed class WdvDepreciationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WdvDepreciationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WdvDepreciationCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            return new WdvDepreciationCommandRepository(conn, ctx, ipMock.Object);
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

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task DeleteAsync_Should_Remove_Unlocked_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("WDV_C1");
            await SeedWdvAsync(1, 2025, groupId, isLocked: 0);

            var result = await CreateRepository(ctx).DeleteAsync(2025);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Delete_Locked_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("WDV_C2");
            await SeedWdvAsync(1, 2025, groupId, isLocked: 1);

            var result = await CreateRepository(ctx).DeleteAsync(2025);

            result.Should().Be(0);
            (await ctx.WDVDepreciationDetail.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999);

            result.Should().Be(0);
        }

        [Fact]
        public async Task LockWDVDepreciationAsync_Should_Set_IsLocked_To_One()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("WDV_L1");
            await SeedWdvAsync(1, 2025, groupId);

            var result = await CreateRepository(ctx).LockWDVDepreciationAsync(2025);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            (await ctx.WDVDepreciationDetail.FirstAsync()).IsLocked.Should().Be((byte)1);
        }

        [Fact]
        public async Task LockWDVDepreciationAsync_Should_Return_Zero_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).LockWDVDepreciationAsync(9999);

            result.Should().Be(0);
        }
    }
}
