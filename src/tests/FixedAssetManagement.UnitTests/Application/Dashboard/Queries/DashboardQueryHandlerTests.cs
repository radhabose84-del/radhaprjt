using FAM.Application.Common.Interfaces.IDashboard;
using FAM.Application.Dashboard;
using FAM.Application.Dashboard.Common;

namespace FixedAssetManagement.UnitTests.Application.Dashboard.Queries
{
    public sealed class DashboardQueryHandlerTests
    {
        private readonly Mock<IDashboardQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private DashboardQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_AssetSummaryType_ReturnsChartDto()
        {
            var expected = new ChartDto
            {
                Categories = new List<string> { "Group A" },
                Series = new List<ChartSeriesDto>()
            };

            _mockRepo
                .Setup(r => r.GetAssetChartViewAsync(It.IsAny<int?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expected);

            var result = await CreateSut().Handle(
                new DashboardQuery
                {
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow,
                    Type = "assetSummary"
                },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Categories.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_AssetExpirySummaryType_ReturnsChartDto()
        {
            var expected = new ChartDto
            {
                Categories = new List<string> { "Expired" },
                Series = new List<ChartSeriesDto>()
            };

            _mockRepo
                .Setup(r => r.GetAssetExpiredDashBoardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expected);

            var result = await CreateSut().Handle(
                new DashboardQuery
                {
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow,
                    Type = "assetexpirySummary"
                },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Categories.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullType_ThrowsArgumentException()
        {
            var query = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow,
                Type = null
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateSut().Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyType_ThrowsArgumentException()
        {
            var query = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow,
                Type = ""
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateSut().Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidType_ThrowsArgumentException()
        {
            var query = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow,
                Type = "invalidType"
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateSut().Handle(query, CancellationToken.None));
        }
    }
}
