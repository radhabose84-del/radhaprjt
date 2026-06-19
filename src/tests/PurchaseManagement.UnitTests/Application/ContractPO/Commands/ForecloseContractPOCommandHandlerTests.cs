using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Foreclose;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.ContractPO.Commands;

public class ForecloseContractPOCommandHandlerTests
{
    private readonly Mock<IContractPOCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private ForecloseContractPOCommandHandler CreateSut() => new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue_AndPublishes()
    {
        _mockCommandRepo.Setup(r => r.ForecloseAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(new ForecloseContractPOCommand(1), CancellationToken.None);

        result.Should().BeTrue();
        _mockMediator.Verify(
            m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "CONTRACT_PO_FORECLOSE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsException()
    {
        _mockCommandRepo.Setup(r => r.ForecloseAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await CreateSut().Handle(new ForecloseContractPOCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
