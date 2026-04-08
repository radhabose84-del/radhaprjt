using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesContact.Queries;

public class GetSalesContactAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesContactAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<SalesContactLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<SalesContactLookupDto> e ? e.ToList() : new List<SalesContactLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesContactAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<SalesContactLookupDto> { new() { Id = 1, ContactName = "John" } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("John", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetSalesContactAutoCompleteQuery("John"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NullTerm_PassesEmptyStringToRepository()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesContactLookupDto>());

        await CreateSut().Handle(new GetSalesContactAutoCompleteQuery(null), CancellationToken.None);

        _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }
}
