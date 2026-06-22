using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterById;

namespace SalesManagement.UnitTests.Application.SalesOrderTypeMaster.Queries;

public class GetSalesOrderTypeMasterByIdQueryHandlerTests
{
    private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesOrderTypeMasterByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<SalesOrderTypeMasterDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as SalesOrderTypeMasterDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesOrderTypeMasterByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new SalesOrderTypeMasterDto { Id = 1, TypeName = "Normal" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetSalesOrderTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.TypeName.Should().Be("Normal");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesOrderTypeMasterDto?)null);

        var result = await CreateSut().Handle(new GetSalesOrderTypeMasterByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new SalesOrderTypeMasterDto { Id = 7 });

        await CreateSut().Handle(new GetSalesOrderTypeMasterByIdQuery { Id = 7 }, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
    }
}
