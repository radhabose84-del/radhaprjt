using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.ReturnReason;

public sealed class DeleteReturnReasonCommandHandlerTests
{
    private readonly Mock<IReturnReasonCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private DeleteReturnReasonCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ValidId_ReturnsTrue()
    {
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var result = await CreateSut().Handle(ReturnReasonBuilders.ValidDeleteCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Func<Task> act = async () => await CreateSut().Handle(ReturnReasonBuilders.ValidDeleteCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>();
    }
}
