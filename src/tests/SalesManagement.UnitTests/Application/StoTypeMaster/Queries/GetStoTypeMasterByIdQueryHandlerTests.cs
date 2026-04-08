using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterById;

namespace SalesManagement.UnitTests.Application.StoTypeMaster.Queries;

public class GetStoTypeMasterByIdQueryHandlerTests
{
    private readonly Mock<IStoTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetStoTypeMasterByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<StoTypeMasterDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as StoTypeMasterDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetStoTypeMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new StoTypeMasterDto { Id = 1, StoTypeCode = "STO001", StoTypeName = "Standard" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetStoTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.StoTypeCode.Should().Be("STO001");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StoTypeMasterDto?)null);

        var result = await CreateSut().Handle(new GetStoTypeMasterByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new StoTypeMasterDto { Id = 7 });

        await CreateSut().Handle(new GetStoTypeMasterByIdQuery { Id = 7 }, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
    }
}
