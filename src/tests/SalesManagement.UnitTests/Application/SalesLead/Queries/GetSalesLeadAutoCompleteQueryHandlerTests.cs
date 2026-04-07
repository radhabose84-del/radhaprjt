using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Application.SalesLead.Queries.GetSalesLeadAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesLead.Queries;

public class GetSalesLeadAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesLeadAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<SalesLeadLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<SalesLeadLookupDto> e ? e.ToList() : new List<SalesLeadLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesLeadAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<SalesLeadLookupDto> { new() { Id = 1, ContactName = "Jane" } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("Jane", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetSalesLeadAutoCompleteQuery("Jane"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_NullTerm_PassesEmptyStringToRepository()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesLeadLookupDto>());

        await CreateSut().Handle(new GetSalesLeadAutoCompleteQuery(null), CancellationToken.None);

        _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }
}
