using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesAgreement.Queries;

public class GetSalesAgreementAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesAgreementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesAgreementAutoCompleteQueryHandler CreateSut()
    {
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesAgreementAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var list = new List<SalesAgreementLookupDto> { new() { Id = 1, AgreementNo = "SA001" } };
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await CreateSut().Handle(new GetSalesAgreementAutoCompleteQuery("SA"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NoMatch_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesAgreementLookupDto>());

        var result = await CreateSut().Handle(new GetSalesAgreementAutoCompleteQuery("ZZ"), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
