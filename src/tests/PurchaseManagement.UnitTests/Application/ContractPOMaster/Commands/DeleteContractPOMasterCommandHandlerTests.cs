using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Commands.Delete;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.ContractPOMaster.Commands;

public class DeleteContractPOMasterCommandHandlerTests
{
    private readonly Mock<IContractPOMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
    private readonly Mock<IContractPOMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

    private DeleteContractPOMasterCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeaderDto { Id = 1, ContractPONumber = "CPO001" });
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateSut().Handle(new DeleteContractPOMasterCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsSoftDeleteAndPublishes()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractPOHeaderDto { Id = 1, ContractPONumber = "CPO001" });
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(new DeleteContractPOMasterCommand(1), CancellationToken.None);

        _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsException()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractPOHeaderDto?)null);

        var act = async () => await CreateSut().Handle(new DeleteContractPOMasterCommand(99), CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }
}
