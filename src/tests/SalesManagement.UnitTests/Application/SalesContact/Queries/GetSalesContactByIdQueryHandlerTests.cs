using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Application.SalesContact.Queries.GetSalesContactById;

namespace SalesManagement.UnitTests.Application.SalesContact.Queries;

public class GetSalesContactByIdQueryHandlerTests
{
    private readonly Mock<ISalesContactQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesContactByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<SalesContactDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as SalesContactDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesContactByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new SalesContactDto { Id = 1, ContactName = "John Doe" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetSalesContactByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ContactName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesContactDto?)null);

        var result = await CreateSut().Handle(new GetSalesContactByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new SalesContactDto { Id = 7 });

        await CreateSut().Handle(new GetSalesContactByIdQuery { Id = 7 }, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
    }
}
