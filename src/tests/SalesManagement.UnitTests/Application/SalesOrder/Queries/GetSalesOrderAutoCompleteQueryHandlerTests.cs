using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetSalesOrderAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesOrderAutoCompleteQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsResults()
    {
        var list = new List<SalesOrderLookupDto>
        {
            new() { Id = 1, SalesOrderNo = "SO001" }
        } as IReadOnlyList<SalesOrderLookupDto>;

        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(list);
        _mockMapper
            .Setup(m => m.Map<List<SalesOrderLookupDto>>(It.IsAny<object>()))
            .Returns(new List<SalesOrderLookupDto> { new() { Id = 1, SalesOrderNo = "SO001" } });

        var result = await CreateSut().Handle(
            new GetSalesOrderAutoCompleteQuery("test"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NullTerm_UsesEmptyString()
    {
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>(), false))
            .ReturnsAsync(new List<SalesOrderLookupDto>() as IReadOnlyList<SalesOrderLookupDto>);
        _mockMapper
            .Setup(m => m.Map<List<SalesOrderLookupDto>>(It.IsAny<object>()))
            .Returns(new List<SalesOrderLookupDto>());

        var result = await CreateSut().Handle(
            new GetSalesOrderAutoCompleteQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
