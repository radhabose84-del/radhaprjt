using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Application.SalesLead.Queries.GetSalesLeadById;

namespace SalesManagement.UnitTests.Application.SalesLead.Queries;

public class GetSalesLeadByIdQueryHandlerTests
{
    private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesLeadByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<SalesLeadDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as SalesLeadDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesLeadByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new SalesLeadDto { Id = 1, ContactName = "Jane Doe" };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetSalesLeadByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ContactName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesLeadDto?)null);

        var result = await CreateSut().Handle(new GetSalesLeadByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new SalesLeadDto { Id = 7 });

        await CreateSut().Handle(new GetSalesLeadByIdQuery { Id = 7 }, CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
    }
}
