using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderById;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetSalesOrderByIdQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetSalesOrderByIdQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsDto()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new SalesOrderHeaderDto { Id = 1, SalesOrderNo = "SO001" });

        var result = await CreateSut().Handle(
            new GetSalesOrderByIdQuery { Id = 1 }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.SalesOrderNo.Should().Be("SO001");
    }

    [Fact]
    public async Task Handle_NonExistentId_ReturnsNull()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((SalesOrderHeaderDto?)null);

        var result = await CreateSut().Handle(
            new GetSalesOrderByIdQuery { Id = 99 }, CancellationToken.None);

        result.Should().BeNull();
    }
}
