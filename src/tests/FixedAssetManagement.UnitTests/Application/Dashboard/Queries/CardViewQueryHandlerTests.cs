using FAM.Application.Common.Interfaces.IDashboard;
using FAM.Application.Dashboard.CardView;

namespace FixedAssetManagement.UnitTests.Application.Dashboard.Queries
{
    public sealed class CardViewQueryHandlerTests
    {
        private readonly Mock<IDashboardQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private CardViewQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsCardViewDto()
        {
            var expected = new CardViewDto
            {
                TotalAssets = 100,
                TotalAssetValue = 5000000m,
                NewAssets = 10,
                NewAssetsValue = 500000m,
                AssetDisposed = 5
            };

            _mockRepo
                .Setup(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expected);

            var result = await CreateSut().Handle(
                new CardViewQuery
                {
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow
                },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.TotalAssets.Should().Be(100);
            result.TotalAssetValue.Should().Be(5000000m);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo
                .Setup(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new CardViewDto());

            await CreateSut().Handle(
                new CardViewQuery
                {
                    FromDate = DateTime.UtcNow.AddMonths(-1),
                    ToDate = DateTime.UtcNow
                },
                CancellationToken.None);

            _mockRepo.Verify(r => r.GetDashboardDataAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        }
    }
}
