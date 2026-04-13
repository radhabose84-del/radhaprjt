using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Infrastructure.Repositories.AssetTransferIssueApproval;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetTransferIssueApproval
{
    [Collection("DatabaseCollection")]
    public sealed class AssetTransferIssueQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetTransferIssueQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetTransferIssueQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            return new AssetTransferIssueQueryRepository(conn, ipMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllPendingAssetTransferAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllPendingAssetTransferAsync(
                1, 10, null,
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero));

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllPendingAssetTransferAsync_With_Null_Dates_Should_Return_Empty()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllPendingAssetTransferAsync(
                1, 10, null, null, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByAssetTransferIdAsync_Should_Return_Empty_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByAssetTransferIdAsync(9999);

            result.Should().BeEmpty();
        }
    }
}
