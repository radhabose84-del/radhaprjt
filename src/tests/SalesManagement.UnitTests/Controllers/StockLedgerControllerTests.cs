using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Application.StockLedger.Queries.GetStockByPackRange;
using SalesManagement.Application.StockLedger.Queries.GetStockLedgerReport;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class StockLedgerControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private StockLedgerController CreateSut() => new(_mockMediator.Object);

    // ── GetStockLedgerReport ─────────────────────────────────────

    [Fact]
    public async Task GetStockLedgerReport_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockLedgerReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<StockLedgerReportDto>>
            {
                IsSuccess = true,
                Data = new List<StockLedgerReportDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetStockLedgerReportAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStockLedgerReport_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockLedgerReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<StockLedgerReportDto>>
            {
                IsSuccess = true,
                Data = new List<StockLedgerReportDto>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            });

        await CreateSut().GetStockLedgerReportAsync();

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetStockLedgerReportQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStockLedgerReport_WithFilters_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockLedgerReportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<StockLedgerReportDto>>
            {
                IsSuccess = true,
                Data = new List<StockLedgerReportDto>(),
                TotalCount = 5,
                PageNumber = 1,
                PageSize = 10
            });

        var result = await CreateSut().GetStockLedgerReportAsync(
            PageNumber: 1,
            PageSize: 10,
            ItemId: 1,
            LotId: 2,
            WarehouseId: 3,
            BinId: 4,
            StatusId: 5,
            PackNo: 100,
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: new DateOnly(2026, 12, 31),
            ProductionYear: 2026);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── GetStockByPackRange ──────────────────────────────────────

    [Fact]
    public async Task GetStockByPackRange_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockByPackRangeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Data = new List<object>(),
                TotalCount = 0
            });

        var result = await CreateSut().GetStockByPackRangeAsync(ProductionYear: 2026);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStockByPackRange_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockByPackRangeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Data = new List<object>(),
                TotalCount = 0
            });

        await CreateSut().GetStockByPackRangeAsync(ProductionYear: 2026);

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetStockByPackRangeQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStockByPackRange_WithAllFilters_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetStockByPackRangeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Data = new List<object>(),
                TotalCount = 3
            });

        var result = await CreateSut().GetStockByPackRangeAsync(
            ProductionYear: 2026,
            ItemId: 1,
            StartPackNo: 100,
            EndPackNo: 200,
            PackTypeId: 5);

        result.Should().BeOfType<OkObjectResult>();
    }
}
