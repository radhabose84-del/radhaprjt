using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetAutoComplete;

namespace SalesManagement.UnitTests.Application.TripSheet.Queries;

public class GetTripSheetAutoCompleteQueryHandlerTests
{
    private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetTripSheetAutoCompleteQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<TripSheetLookupDto>>(It.IsAny<object>()))
            .Returns<object>(o => o is IEnumerable<TripSheetLookupDto> e ? e.ToList() : new List<TripSheetLookupDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetTripSheetAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_WithTerm_ReturnsLookupList()
    {
        var lookupList = new List<TripSheetLookupDto> { new() { Id = 1, TripSheetNo = "TS001" } };
        _mockQueryRepo.Setup(r => r.AutocompleteAsync("TS", It.IsAny<CancellationToken>())).ReturnsAsync(lookupList);

        var result = await CreateSut().Handle(new GetTripSheetAutoCompleteQuery("TS"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTerm_ReturnsEmptyList()
    {
        _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TripSheetLookupDto>());

        var result = await CreateSut().Handle(new GetTripSheetAutoCompleteQuery(string.Empty), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
