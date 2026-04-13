using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using FAM.Infrastructure.Repositories.Dashboard;

namespace FixedAssetManagement.IntegrationTests.Repositories.Dashboard
{
    [Collection("DatabaseCollection")]
    public sealed class DashboardQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DashboardQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DashboardQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            var deptLookup = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            return new DashboardQueryRepository(conn, ipMock.Object, deptLookup.Object);
        }

        [Fact]
        public async Task GetAssetChartViewAsync_Should_Return_Empty_Categories_When_No_Data()
        {
            var result = await CreateQueryRepo().GetAssetChartViewAsync(null,
                new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            result.Should().NotBeNull();
            result.Categories.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetChartViewAsync_Should_Build_Two_Series()
        {
            var result = await CreateQueryRepo().GetAssetChartViewAsync(null,
                new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            result.Series.Should().HaveCount(2);
            result.Series[0].Name.Should().Be("Asset Count");
            result.Series[1].Name.Should().Be("Total Purchase Value");
        }

        [Fact]
        public async Task GetAssetExpiredDashBoardDataAsync_Should_Return_Empty_When_No_Data()
        {
            var result = await CreateQueryRepo().GetAssetExpiredDashBoardDataAsync(
                new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            result.Should().NotBeNull();
            result.Categories.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetExpiredDashBoardDataAsync_Should_Build_Two_Series()
        {
            var result = await CreateQueryRepo().GetAssetExpiredDashBoardDataAsync(
                new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            result.Series.Should().HaveCount(2);
            result.Series[0].Name.Should().Be("Expired Assets");
            result.Series[1].Name.Should().Be("Residual Value");
        }

        [Fact]
        public async Task GetDashboardDataAsync_Should_Return_Empty_CardView_When_No_Data()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateQueryRepo().GetDashboardDataAsync(
                new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            result.Should().NotBeNull();
            result.TotalAssets.Should().Be(0);
        }
    }
}
