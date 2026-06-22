using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterAutoComplete;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Queries;

public class GetSalesOrderTypeMasterAutoCompleteQueryHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesOrderTypeMasterAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<SalesOrderTypeMasterLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<SalesOrderTypeMasterLookupDto> e ? e.ToList() : new List<SalesOrderTypeMasterLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesOrderTypeMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<SalesOrderTypeMasterLookupDto> { new() { Id = 1, TypeName = "Normal" } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("SO", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetSalesOrderTypeMasterAutoCompleteQuery("SO"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTerm_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesOrderTypeMasterLookupDto>());

        var result = await CreateSut().Handle(new GetSalesOrderTypeMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
