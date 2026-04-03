using Contracts.Dtos.Lookups.Users;
using InventoryManagement.Application.Reports.GetUnitsByDivision;
using InventoryManagement.Application.Reports.StockReport;
using InventoryManagement.Application.Reports.StockReportDivisionwise;
using InventoryManagement.Application.Reports.SubStoresStock;
using InventoryManagement.Presentation.Controllers.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class ReportsControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private ReportsController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetCurrentStockSummary_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockSummaryDto> { new() });

            var result = await CreateSut().GetCurrentStockSummary();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentSubStoresStockSummary_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetSubStockReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SubStockSummaryDto> { new() });

            var result = await CreateSut().GetCurrentSubStoresStockSummary();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStockUnitWiseSummary_WithValidUnitIds_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetStockReportDivsionwiseSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockSummaryDivsionwiseDto>());

            var result = await CreateSut().GetCurrentStockUnitWiseSummary("1,2,3");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStockUnitWiseSummary_WithEmptyUnitIds_ReturnsBadRequest()
        {
            var result = await CreateSut().GetCurrentStockUnitWiseSummary("");

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetUnitsByDivision_WithValidIds_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUnitsByDivisionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DivisionUnitLookupDto>());

            var result = await CreateSut().GetUnitsByDivision(1, 1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetUnitsByDivision_WithZeroIds_ReturnsBadRequest()
        {
            var result = await CreateSut().GetUnitsByDivision(0, 0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
