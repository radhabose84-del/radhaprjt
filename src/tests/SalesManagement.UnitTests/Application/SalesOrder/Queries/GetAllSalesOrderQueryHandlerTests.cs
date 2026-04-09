using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetAllSalesOrderQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetAllSalesOrderQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null))
            .ReturnsAsync((new List<SalesOrderHeaderDto> { new() { Id = 1 } }, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesParametersCorrectly()
    {
        var dateFrom = new DateOnly(2026, 1, 1);
        var dateTo = new DateOnly(2026, 12, 31);
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, "search", dateFrom, dateTo, "Party", "Approved"))
            .ReturnsAsync((new List<SalesOrderHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery
            {
                PageNumber = 1, PageSize = 10, SearchTerm = "search",
                OrderDateFrom = dateFrom, OrderDateTo = dateTo,
                PartyName = "Party", StatusName = "Approved"
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null))
            .ReturnsAsync((new List<SalesOrderHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
    }
}
