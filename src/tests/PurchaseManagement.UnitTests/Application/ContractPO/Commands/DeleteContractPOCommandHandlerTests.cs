using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.ContractPO.Commands;

public class DeleteContractPOCommandHandlerTests
{
    private readonly Mock<IContractPOCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteContractPOCommandHandler CreateSut() => new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_Success_ReturnsTrue_AndPublishes()
    {
        _mockCommandRepo.Setup(r => r.DeleteContractPOAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(new DeleteContractPOCommand(1), CancellationToken.None);

        result.Should().BeTrue();
        _mockMediator.Verify(
            m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "CONTRACT_RELEASE_PO_DELETE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NothingDeleted_ReturnsFalse_DoesNotPublish()
    {
        _mockCommandRepo.Setup(r => r.DeleteContractPOAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await CreateSut().Handle(new DeleteContractPOCommand(99), CancellationToken.None);

        result.Should().BeFalse();
        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
