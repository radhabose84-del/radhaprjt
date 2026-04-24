using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderInvoices;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetSalesOrderInvoicesQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new();

    private GetSalesOrderInvoicesQueryHandler CreateSut()
    {
        _mockMediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new GetSalesOrderInvoicesQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ReturnsInvoices_ForSalesOrder()
    {
        var data = new List<SalesOrderInvoiceDto>
        {
            new() { InvoiceId = 101, InvoiceNo = "INV-101", PartyId = 7, PartyName = "Acme" },
            new() { InvoiceId = 102, InvoiceNo = "INV-102", PartyId = 7, PartyName = "Acme" }
        };
        _mockQueryRepo
            .Setup(r => r.GetSalesOrderInvoicesAsync(42))
            .ReturnsAsync(data);

        var result = await CreateSut().Handle(
            new GetSalesOrderInvoicesQuery { SalesOrderId = 42 },
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeSameAs(data);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockQueryRepo
            .Setup(r => r.GetSalesOrderInvoicesAsync(99))
            .ReturnsAsync(new List<SalesOrderInvoiceDto>());

        var result = await CreateSut().Handle(
            new GetSalesOrderInvoicesQuery { SalesOrderId = 99 },
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsRepository_WithCorrectSalesOrderId()
    {
        _mockQueryRepo
            .Setup(r => r.GetSalesOrderInvoicesAsync(123))
            .ReturnsAsync(new List<SalesOrderInvoiceDto>());

        await CreateSut().Handle(
            new GetSalesOrderInvoicesQuery { SalesOrderId = 123 },
            CancellationToken.None);

        _mockQueryRepo.Verify(r => r.GetSalesOrderInvoicesAsync(123), Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesAuditEvent()
    {
        _mockQueryRepo
            .Setup(r => r.GetSalesOrderInvoicesAsync(42))
            .ReturnsAsync(new List<SalesOrderInvoiceDto> { new() { InvoiceId = 1 } });

        await CreateSut().Handle(
            new GetSalesOrderInvoicesQuery { SalesOrderId = 42 },
            CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<SalesManagement.Domain.Events.AuditLogsDomainEvent>(e =>
                    e.ActionDetail == "GetSalesOrderInvoicesQuery" &&
                    e.Module == "SalesOrder"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
