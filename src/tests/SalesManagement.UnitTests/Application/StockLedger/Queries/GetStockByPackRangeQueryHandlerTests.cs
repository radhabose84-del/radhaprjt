using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Application.StockLedger.Queries.GetStockByPackRange;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.StockLedger.Queries;

public sealed class GetStockByPackRangeQueryHandlerTests
{
    private readonly Mock<IStockLedgerReportRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetStockByPackRangeQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockMediator.Object);

    // ── Detail mode (all 4 params provided) ──────────────────────

    [Fact]
    public async Task Handle_DetailMode_ReturnsSuccess()
    {
        _mockRepo
            .Setup(r => r.GetByPackRangeAsync(1, 2, 100, 200, 2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockLedgerReportDto>());

        var result = await CreateSut().Handle(
            new GetStockByPackRangeQuery
            {
                ItemId = 1, PackTypeId = 2,
                StartPackNo = 100, EndPackNo = 200, ProductionYear = 2026
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("pack range");
    }

    [Fact]
    public async Task Handle_DetailMode_CallsGetByPackRangeAsync()
    {
        _mockRepo
            .Setup(r => r.GetByPackRangeAsync(1, 2, 100, 200, 2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockLedgerReportDto> { new() });

        await CreateSut().Handle(
            new GetStockByPackRangeQuery
            {
                ItemId = 1, PackTypeId = 2,
                StartPackNo = 100, EndPackNo = 200, ProductionYear = 2026
            },
            CancellationToken.None);

        _mockRepo.Verify(
            r => r.GetByPackRangeAsync(1, 2, 100, 200, 2026, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DetailMode_ReturnsTotalCount()
    {
        _mockRepo
            .Setup(r => r.GetByPackRangeAsync(1, 2, 100, 200, 2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockLedgerReportDto> { new(), new(), new() });

        var result = await CreateSut().Handle(
            new GetStockByPackRangeQuery
            {
                ItemId = 1, PackTypeId = 2,
                StartPackNo = 100, EndPackNo = 200, ProductionYear = 2026
            },
            CancellationToken.None);

        result.TotalCount.Should().Be(3);
    }

    // ── Summary mode (not all 4 params) ──────────────────────────

    [Fact]
    public async Task Handle_SummaryMode_ReturnsSuccess()
    {
        _mockRepo
            .Setup(r => r.GetPackRangeSummaryAsync(2026, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackRangeSummaryDto>());

        var result = await CreateSut().Handle(
            new GetStockByPackRangeQuery { ProductionYear = 2026 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("pack range");
    }

    [Fact]
    public async Task Handle_SummaryMode_CallsGetPackRangeSummaryAsync()
    {
        _mockRepo
            .Setup(r => r.GetPackRangeSummaryAsync(2026, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackRangeSummaryDto>());

        await CreateSut().Handle(
            new GetStockByPackRangeQuery { ProductionYear = 2026 },
            CancellationToken.None);

        _mockRepo.Verify(
            r => r.GetPackRangeSummaryAsync(2026, null, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SummaryMode_WithPartialFilters_CallsSummary()
    {
        // Only ItemId provided (not all 4), so summary mode
        _mockRepo
            .Setup(r => r.GetPackRangeSummaryAsync(2026, 1, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackRangeSummaryDto> { new() });

        var result = await CreateSut().Handle(
            new GetStockByPackRangeQuery { ProductionYear = 2026, ItemId = 1 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.TotalCount.Should().Be(1);
    }

    // ── Audit event ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetPackRangeSummaryAsync(2026, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PackRangeSummaryDto>());

        await CreateSut().Handle(
            new GetStockByPackRangeQuery { ProductionYear = 2026 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetStockByPackRange" &&
                    e.ActionCode == "Get"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
