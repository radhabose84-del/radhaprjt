using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class SubmitPurchaseReturnCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private SubmitPurchaseReturnCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockOutbox.Object, _mockIp.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_PendingRtv_ReturnsTrue()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1, "Pending"));
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync(It.IsAny<string>())).ReturnsAsync(2);
        _mockQueryRepo.Setup(r => r.GetReturnTypeApprovalRoleCodeAsync(It.IsAny<int>())).ReturnsAsync("QcHead");
        _mockCommandRepo.Setup(r => r.SetStatusAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidSubmitCommand(1), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto.PurchaseReturnHeaderDto?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidSubmitCommand(99), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NotPending_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1, "Approved"));

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidSubmitCommand(1), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Pending*");
    }
}
