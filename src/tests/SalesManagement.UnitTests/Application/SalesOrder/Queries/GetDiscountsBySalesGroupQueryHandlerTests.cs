using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetDiscountsBySalesGroupQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetDiscountsBySalesGroupQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsDiscounts()
    {
        var discounts = new List<DiscountsBySalesGroupDto>
        {
            new DiscountsBySalesGroupDto { Id = 1, DiscountCode = "D001", DiscountName = "Test Discount" }
        };

        _mockQueryRepo
            .Setup(r => r.GetDiscountsBySalesGroupAsync(1, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discounts);

        var query = new GetDiscountsBySalesGroupQuery
        {
            SalesGroupId = 1,
            SlabTypeId = 2,
            PaymentTermId = 3
        };

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].DiscountCode.Should().Be("D001");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetDiscountsBySalesGroupAsync(99, 99, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DiscountsBySalesGroupDto>());

        var query = new GetDiscountsBySalesGroupQuery
        {
            SalesGroupId = 99,
            SlabTypeId = 99,
            PaymentTermId = 99
        };

        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepositoryOnce()
    {
        _mockQueryRepo
            .Setup(r => r.GetDiscountsBySalesGroupAsync(1, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DiscountsBySalesGroupDto>());

        var query = new GetDiscountsBySalesGroupQuery
        {
            SalesGroupId = 1,
            SlabTypeId = 2,
            PaymentTermId = 3
        };

        await CreateSut().Handle(query, CancellationToken.None);

        _mockQueryRepo.Verify(
            r => r.GetDiscountsBySalesGroupAsync(1, 2, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetDiscountsBySalesGroupAsync(1, 2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DiscountsBySalesGroupDto>());

        var query = new GetDiscountsBySalesGroupQuery
        {
            SalesGroupId = 1,
            SlabTypeId = 2,
            PaymentTermId = 3
        };

        await CreateSut().Handle(query, CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetDiscountsBySalesGroupQuery"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
