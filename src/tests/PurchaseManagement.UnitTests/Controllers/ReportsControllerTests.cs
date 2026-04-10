using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Reports.StockReport;
using PurchaseManagement.Application.Reports.SubStoresStock;
using PurchaseManagement.Presentation.Controllers.Reports;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ReportsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ReportsController CreateSut() => new(_mockMediator.Object);

        // --- GetCurrentStockSummary ---

        [Fact]
        public async Task GetCurrentStockSummary_WhenDataExists_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockSummaryDto> { new() });

            var result = await CreateSut().GetCurrentStockSummary();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStockSummary_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<StockSummaryDto>?)null);

            var result = await CreateSut().GetCurrentStockSummary();

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStockSummary_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockSummaryDto>());

            await CreateSut().GetCurrentStockSummary();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetStockReportSummaryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- GetCurrentSubStoresStockSummary ---

        [Fact]
        public async Task GetCurrentSubStoresStockSummary_WhenDataExists_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubStockSummaryDto> { new() });

            var result = await CreateSut().GetCurrentSubStoresStockSummary();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentSubStoresStockSummary_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<SubStockSummaryDto>?)null);

            var result = await CreateSut().GetCurrentSubStoresStockSummary();

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetCurrentSubStoresStockSummary_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetSubStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubStockSummaryDto>());

            await CreateSut().GetCurrentSubStoresStockSummary();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetSubStockReportSummaryQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
