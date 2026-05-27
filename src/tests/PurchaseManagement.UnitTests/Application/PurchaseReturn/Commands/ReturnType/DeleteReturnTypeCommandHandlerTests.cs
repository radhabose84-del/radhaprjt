using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.ReturnType;

public sealed class DeleteReturnTypeCommandHandlerTests
{
    private readonly Mock<IReturnTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private DeleteReturnTypeCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await CreateSut().Handle(ReturnTypeBuilders.ValidDeleteCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Func<Task> act = async () => await CreateSut().Handle(ReturnTypeBuilders.ValidDeleteCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>();
    }

    [Fact]
    public async Task Handle_ValidId_PublishesAuditEvent()
    {
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await CreateSut().Handle(ReturnTypeBuilders.ValidDeleteCommand(1), CancellationToken.None);
        _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
