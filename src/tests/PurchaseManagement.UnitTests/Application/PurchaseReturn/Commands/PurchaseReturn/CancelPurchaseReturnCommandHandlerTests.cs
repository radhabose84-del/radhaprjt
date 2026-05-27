using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class CancelPurchaseReturnCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private CancelPurchaseReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Theory]
    [InlineData("Draft")]
    [InlineData("PendingApproval")]
    public async Task Handle_DraftOrPending_ReturnsTrue(string currentStatus)
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync(currentStatus);
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync(It.IsAny<string>())).ReturnsAsync(5);
        _mockCommandRepo.Setup(r => r.SetStatusAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidCancelCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Approved_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Approved");

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidCancelCommand(1), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Cannot cancel*");
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(99)).ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidCancelCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
    }
}
