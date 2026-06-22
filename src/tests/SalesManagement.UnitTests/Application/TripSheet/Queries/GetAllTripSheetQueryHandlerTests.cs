using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Application.TripSheet.Queries.GetAllTripSheet;

namespace SalesManagement.UnitTests.Application.TripSheet.Queries;

public class GetAllTripSheetQueryHandlerTests
{
    private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetAllTripSheetQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<List<TripSheetHeaderDto>>(It.IsAny<object>()))
            .Returns<object>(o => o as List<TripSheetHeaderDto> ?? new List<TripSheetHeaderDto>());
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetAllTripSheetQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
    {
        var data = new List<TripSheetHeaderDto> { new() { Id = 1, TripSheetNo = "TS001" } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((data, 1));

        var result = await CreateSut().Handle(new GetAllTripSheetQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var data = new List<TripSheetHeaderDto> { new() { Id = 1 } };
        _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "TS")).ReturnsAsync((data, 15));

        var result = await CreateSut().Handle(new GetAllTripSheetQuery { PageNumber = 2, PageSize = 5, SearchTerm = "TS" }, CancellationToken.None);

        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
    {
        _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((new List<TripSheetHeaderDto>(), 0));

        var result = await CreateSut().Handle(new GetAllTripSheetQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
