using AutoMapper;
using Contracts;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetAllSalesOrderQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper>                   _mockMapper    = new(MockBehavior.Loose);
    private readonly Mock<IMediator>                 _mockMediator  = new(MockBehavior.Loose);
    private readonly Mock<IAccessPolicyService>      _mockPolicy    = new(MockBehavior.Loose);

    private GetAllSalesOrderQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockPolicy.Object);

    private void SetupUnrestricted()
    {
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<int>?)null);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        SetupUnrestricted();
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null, null))
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
        SetupUnrestricted();
        var dateFrom = new DateOnly(2026, 1, 1);
        var dateTo   = new DateOnly(2026, 12, 31);
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, "search", dateFrom, dateTo, "Party", "Approved", null, null))
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
        SetupUnrestricted();
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null, null))
            .ReturnsAsync((new List<SalesOrderHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AccessPolicy_EmptyAllowedList_ReturnsEmptyWithoutHittingRepo()
    {
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>().AsReadOnly());

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _mockQueryRepo.Verify(
            r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                               It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string?>(),
                               It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<int>?>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AccessPolicy_AllowedListPassedToRepo()
    {
        var allowedIds = new List<int> { 1, 2 }.AsReadOnly();
        _mockPolicy
            .Setup(s => s.GetAllowedValueIdsAsync(AccessPolicyCodes.SalesOrderType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowedIds);

        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null, allowedIds))
            .ReturnsAsync((new List<SalesOrderHeaderDto>(), 0));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _mockQueryRepo.Verify(
            r => r.GetAllAsync(1, 10, null, null, null, null, null, null, allowedIds),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AccessPolicy_NullAllowedList_QueryRunsUnrestricted()
    {
        SetupUnrestricted();
        _mockQueryRepo
            .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null, null))
            .ReturnsAsync((new List<SalesOrderHeaderDto> { new() { Id = 5 } }, 1));

        var result = await CreateSut().Handle(
            new GetAllSalesOrderQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.Data.Should().HaveCount(1);
        _mockQueryRepo.Verify(
            r => r.GetAllAsync(1, 10, null, null, null, null, null, null, null),
            Times.Once);
    }
}
