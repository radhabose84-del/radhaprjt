using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Cancel;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.BlanketPO.Commands;

public class CancelBlanketPOCommandHandlerTests
{
    private readonly Mock<IBlanketPOCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private CancelBlanketPOCommandHandler CreateSut() => new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue_AndPublishes()
    {
        _mockCommandRepo.Setup(r => r.CancelAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(new CancelBlanketPOCommand(1), CancellationToken.None);

        result.Should().BeTrue();
        _mockMediator.Verify(
            m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BLANKET_RELEASE_PO_CANCEL"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsException()
    {
        _mockCommandRepo.Setup(r => r.CancelAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = async () => await CreateSut().Handle(new CancelBlanketPOCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
