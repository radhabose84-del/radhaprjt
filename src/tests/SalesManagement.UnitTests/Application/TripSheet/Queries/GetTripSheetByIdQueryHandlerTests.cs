using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Application.TripSheet.Queries.GetTripSheetById;

namespace SalesManagement.UnitTests.Application.TripSheet.Queries;

public class GetTripSheetByIdQueryHandlerTests
{
    private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetTripSheetByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<TripSheetHeaderDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as TripSheetHeaderDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetTripSheetByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new TripSheetHeaderDto { Id = 1, TripSheetNo = "TS001" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetTripSheetByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TripSheetNo.Should().Be("TS001");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TripSheetHeaderDto?)null);

        var result = await CreateSut().Handle(new GetTripSheetByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new TripSheetHeaderDto { Id = 7 });

        await CreateSut().Handle(new GetTripSheetByIdQuery { Id = 7 }, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
    }
}
