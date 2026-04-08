using FAM.Application.Dashboard;
using FAM.Application.Dashboard.CardView;
using FAM.Application.Dashboard.Common;
using FAM.Presentation.Controllers.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class DashboardControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DashboardController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetCardDashboard_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CardViewQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CardViewDto
                {
                    TotalAssets = 10,
                    TotalAssetValue = 100000m
                });

            var request = new CardViewQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            var result = await CreateSut().GetCardDashboard(request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCardDashboard_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CardViewQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CardViewDto());

            var request = new CardViewQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            await CreateSut().GetCardDashboard(request);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CardViewQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAssetSummary_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto
                {
                    Categories = new List<string>(),
                    Series = new List<ChartSeriesDto>()
                });

            var request = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            var result = await CreateSut().GetAssetSummary(request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAssetSummary_SetsTypeToAssetSummary()
        {
            DashboardQuery? capturedQuery = null;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ChartDto>, CancellationToken>((q, _) => capturedQuery = (DashboardQuery)q)
                .ReturnsAsync(new ChartDto());

            var request = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            await CreateSut().GetAssetSummary(request);

            capturedQuery.Should().NotBeNull();
            capturedQuery!.Type.Should().Be("assetSummary");
        }

        [Fact]
        public async Task GetAssertExpirySummary_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChartDto());

            var request = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            var result = await CreateSut().GetAssertExpirySummary(request);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAssertExpirySummary_SetsTypeToAssetExpirySummary()
        {
            DashboardQuery? capturedQuery = null;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DashboardQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ChartDto>, CancellationToken>((q, _) => capturedQuery = (DashboardQuery)q)
                .ReturnsAsync(new ChartDto());

            var request = new DashboardQuery
            {
                FromDate = DateTime.UtcNow.AddMonths(-1),
                ToDate = DateTime.UtcNow
            };
            await CreateSut().GetAssertExpirySummary(request);

            capturedQuery.Should().NotBeNull();
            capturedQuery!.Type.Should().Be("assetexpirySummary");
        }
    }
}
