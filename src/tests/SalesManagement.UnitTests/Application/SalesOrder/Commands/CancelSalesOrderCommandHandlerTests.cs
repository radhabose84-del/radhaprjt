using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CancelSalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Commands;

public sealed class CancelSalesOrderCommandHandlerTests
{
    private readonly Mock<ISalesOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CancelSalesOrderCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ExistingId_ReturnsTrue()
    {
        _mockCommandRepo
            .Setup(r => r.CancelAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().Handle(new CancelSalesOrderCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingId_PublishesAuditEvent()
    {
        _mockCommandRepo
            .Setup(r => r.CancelAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await CreateSut().Handle(new CancelSalesOrderCommand(1), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "SALESORDER_CANCEL"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentId_ThrowsExceptionRules()
    {
        _mockCommandRepo
            .Setup(r => r.CancelAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = async () =>
            await CreateSut().Handle(new CancelSalesOrderCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<ExceptionRules>()
            .WithMessage("*not found*");
    }
}
