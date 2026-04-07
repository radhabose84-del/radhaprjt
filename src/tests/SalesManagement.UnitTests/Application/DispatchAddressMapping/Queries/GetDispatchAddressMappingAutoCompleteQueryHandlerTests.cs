using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingAutoComplete;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Queries;

public class GetDispatchAddressMappingAutoCompleteQueryHandlerTests
{
    private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetDispatchAddressMappingAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<DispatchAddressMappingLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<DispatchAddressMappingLookupDto> e ? e.ToList() : new List<DispatchAddressMappingLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetDispatchAddressMappingAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<DispatchAddressMappingLookupDto> { new() { Id = 1, DispatchAddressId = 10 } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetDispatchAddressMappingAutoCompleteQuery("test"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTerm_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DispatchAddressMappingLookupDto>());

        var result = await CreateSut().Handle(new GetDispatchAddressMappingAutoCompleteQuery(null), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
