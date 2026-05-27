using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Commands.PurchaseReturn;

public sealed class ProcessPurchaseReturnApprovalDecisionCommandHandlerTests
{
    private readonly Mock<IPurchaseReturnCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
    private readonly Mock<IPurchaseReturnQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private ProcessPurchaseReturnApprovalDecisionCommandHandler CreateSut() =>
        new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_Approved_WritesStockLedger()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("PendingApproval");
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync("Approved")).ReturnsAsync(3);
        _mockCommandRepo.Setup(r => r.SetStatusAsync(1, 3, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidProcessApprovalCommand(1, isApproved: true), CancellationToken.None);

        result.Should().BeTrue();
        _mockCommandRepo.Verify(r => r.WriteStockLedgerOnApprovalAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Rejected_DoesNotWriteStockLedger()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("PendingApproval");
        _mockQueryRepo.Setup(r => r.GetStatusIdByCodeAsync("Rejected")).ReturnsAsync(4);
        _mockCommandRepo.Setup(r => r.SetStatusAsync(1, 4, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidProcessApprovalCommand(1, isApproved: false), CancellationToken.None);

        result.Should().BeTrue();
        _mockCommandRepo.Verify(r => r.WriteStockLedgerOnApprovalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StatusNotPendingApproval_ReturnsFalseIdempotent()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(1)).ReturnsAsync("Approved"); // already done

        var result = await CreateSut().Handle(PurchaseReturnBuilders.ValidProcessApprovalCommand(1, isApproved: true), CancellationToken.None);

        result.Should().BeFalse();
        _mockCommandRepo.Verify(r => r.WriteStockLedgerOnApprovalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsExceptionRules()
    {
        _mockQueryRepo.Setup(r => r.GetCurrentStatusCodeAsync(99)).ReturnsAsync((string?)null);

        Func<Task> act = async () => await CreateSut().Handle(PurchaseReturnBuilders.ValidProcessApprovalCommand(99, isApproved: true), CancellationToken.None);
        await act.Should().ThrowAsync<ExceptionRules>();
    }
}
