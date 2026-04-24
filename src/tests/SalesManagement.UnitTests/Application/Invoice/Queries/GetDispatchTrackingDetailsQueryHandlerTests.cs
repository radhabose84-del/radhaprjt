using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Queries.GetDispatchTrackingDetails;

namespace SalesManagement.UnitTests.Application.Invoice.Queries;

public sealed class GetDispatchTrackingDetailsQueryHandlerTests
{
    private readonly Mock<IInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetDispatchTrackingDetailsQueryHandler CreateSut()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetDispatchTrackingDetailsQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsDto_WhenFound()
    {
        var dto = new DispatchTrackingDetailsDto
        {
            SalesOrderId = 42,
            SalesOrderNo = "SO-2026-0042",
            Shipments = new List<DispatchTrackingShipmentDto>
            {
                new() { DispatchAdviceId = 1, DispatchNo = "DA-1", PartyId = 7, PartyName = "Acme" }
            }
        };
        _mockQueryRepo
            .Setup(r => r.GetDispatchTrackingDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await CreateSut().Handle(
            new GetDispatchTrackingDetailsQuery { SalesOrderId = 42 },
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.SalesOrderId.Should().Be(42);
        result.Shipments.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenRepositoryReturnsNull()
    {
        _mockQueryRepo
            .Setup(r => r.GetDispatchTrackingDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DispatchTrackingDetailsDto?)null);

        var result = await CreateSut().Handle(
            new GetDispatchTrackingDetailsQuery { SalesOrderId = 99 },
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent_WithSalesOrderIdInActionName()
    {
        _mockQueryRepo
            .Setup(r => r.GetDispatchTrackingDetailsAsync(77, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DispatchTrackingDetailsDto { SalesOrderId = 77 });

        await CreateSut().Handle(
            new GetDispatchTrackingDetailsQuery { SalesOrderId = 77 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<SalesManagement.Domain.Events.AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetDispatchTrackingDetails" &&
                    e.ActionName == "77"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
