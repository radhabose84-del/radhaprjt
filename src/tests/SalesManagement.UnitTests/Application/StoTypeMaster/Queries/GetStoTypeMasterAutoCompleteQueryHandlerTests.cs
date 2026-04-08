using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterAutoComplete;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Queries;

public class GetStoTypeMasterAutoCompleteQueryHandlerTests
{
    private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetStoTypeMasterAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<StoTypeMasterLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<StoTypeMasterLookupDto> e ? e.ToList() : new List<StoTypeMasterLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetStoTypeMasterAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<StoTypeMasterLookupDto> { new() { Id = 1, StoTypeCode = "STO001" } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("STO", It.IsAny<CancellationToken>()))
            .ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetStoTypeMasterAutoCompleteQuery("STO"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTerm_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoTypeMasterLookupDto>());

        var result = await CreateSut().Handle(new GetStoTypeMasterAutoCompleteQuery(string.Empty), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
