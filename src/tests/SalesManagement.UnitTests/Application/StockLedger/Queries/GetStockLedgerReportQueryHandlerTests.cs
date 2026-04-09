using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Application.StockLedger.Queries.GetStockLedgerReport;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StockLedger.Queries;

public sealed class GetStockLedgerReportQueryHandlerTests
{
    private readonly Mock<IStockLedgerReportRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetStockLedgerReportQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var dtoList = new List<StockLedgerReportDto>();
        _mockRepo
            .Setup(r => r.GetReportAsync(1, 10, null, null, null, null, null, null, null, null, null))
            .ReturnsAsync((dtoList, 0));

        var result = await CreateSut().Handle(
            new GetStockLedgerReportQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(dtoList);
    }

    [Fact]
    public async Task Handle_ReturnsPaginationMetadata()
    {
        var dtoList = new List<StockLedgerReportDto> { new() };
        _mockRepo
            .Setup(r => r.GetReportAsync(2, 5, 1, null, null, null, null, null, null, null, null))
            .ReturnsAsync((dtoList, 11));

        var result = await CreateSut().Handle(
            new GetStockLedgerReportQuery { PageNumber = 2, PageSize = 5, ItemId = 1 },
            CancellationToken.None);

        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(11);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetReportAsync(1, 10, null, null, null, null, null, null, null, null, null))
            .ReturnsAsync((new List<StockLedgerReportDto>(), 0));

        await CreateSut().Handle(
            new GetStockLedgerReportQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetStockLedgerReport" &&
                    e.ActionCode == "Get"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithAllFilters_ReturnsSuccess()
    {
        var dateFrom = new DateOnly(2026, 1, 1);
        var dateTo = new DateOnly(2026, 12, 31);

        _mockRepo
            .Setup(r => r.GetReportAsync(1, 10, 1, 2, 3, 4, 5, 100, dateFrom, dateTo, 2026))
            .ReturnsAsync((new List<StockLedgerReportDto> { new() }, 1));

        var result = await CreateSut().Handle(
            new GetStockLedgerReportQuery
            {
                PageNumber = 1, PageSize = 10,
                ItemId = 1, LotId = 2, WarehouseId = 3, BinId = 4,
                StatusId = 5, PackNo = 100,
                DateFrom = dateFrom, DateTo = dateTo, ProductionYear = 2026
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockRepo
            .Setup(r => r.GetReportAsync(1, 10, null, null, null, null, null, null, null, null, null))
            .ReturnsAsync((new List<StockLedgerReportDto>(), 0));

        var result = await CreateSut().Handle(
            new GetStockLedgerReportQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
