using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesEnquiry.Queries;

public sealed class GetSalesEnquiryAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesEnquiryAutoCompleteQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsResults()
    {
        var list = new List<SalesEnquiryLookupDto>
        {
            new() { Id = 1, PartyId = 1, PartyName = "Test Party" }
        };
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateSut().Handle(
            new GetSalesEnquiryAutoCompleteQuery("test"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NullTerm_UsesEmptyString()
    {
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesEnquiryLookupDto>());

        var result = await CreateSut().Handle(
            new GetSalesEnquiryAutoCompleteQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
