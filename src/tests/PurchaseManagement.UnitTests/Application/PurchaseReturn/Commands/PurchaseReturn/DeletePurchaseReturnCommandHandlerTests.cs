using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class DeletePurchaseReturnCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private DeletePurchaseReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_DraftStatus_ReturnsTrue()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Draft");
        _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidDeleteCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotDraft_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Approved");

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidDeleteCommand(1), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Draft*");
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(99)).ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidDeleteCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>();
    }
}
