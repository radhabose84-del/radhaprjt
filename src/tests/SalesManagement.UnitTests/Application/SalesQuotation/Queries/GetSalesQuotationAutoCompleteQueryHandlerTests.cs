using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesQuotation.Queries;

public sealed class GetSalesQuotationAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesQuotationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesQuotationAutoCompleteQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsResults()
    {
        var list = new List<SalesQuotationLookupDto> { new() { Id = 1, CustomerName = "Test" } };
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateSut().Handle(
            new GetSalesQuotationAutoCompleteQuery("test"), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
