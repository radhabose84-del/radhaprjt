using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Application.DispatchAddressMapping.Queries.GetDispatchAddressMappingById;

namespace SalesManagement.UnitTests.Application.DispatchAddressMapping.Queries;

public class GetDispatchAddressMappingByIdQueryHandlerTests
{
    private readonly Mock<IDispatchAddressMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMediator> _mockMediator = new();

    private GetDispatchAddressMappingByIdQueryHandler CreateSut()
    {
        _mockMapper.Setup(m => m.Map<DispatchAddressMappingDto>(It.IsAny<object>()))
            .Returns<object>(o => (o as DispatchAddressMappingDto)!);
        _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetDispatchAddressMappingByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsDto()
    {
        var dto = new DispatchAddressMappingDto { Id = 1, PartyId = 10 };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetDispatchAddressMappingByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_EntityExists_ReturnsCorrectDto()
    {
        var dto = new DispatchAddressMappingDto { Id = 1, PartyId = 10, DispatchAddressId = 20 };
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

        var result = await CreateSut().Handle(new GetDispatchAddressMappingByIdQuery { Id = 1 }, CancellationToken.None);

        result!.Id.Should().Be(1);
        result!.PartyId.Should().Be(10);
    }

    [Fact]
    public async Task Handle_EntityNotFound_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DispatchAddressMappingDto?)null);

        var result = await CreateSut().Handle(new GetDispatchAddressMappingByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }
}
