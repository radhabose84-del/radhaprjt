using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Commands
{
    public sealed class DeleteBarcodeAllocationCommandHandlerTests
    {
        private readonly Mock<IBarcodeAllocationCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteBarcodeAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteBarcodeAllocationCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteBarcodeAllocationCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteBarcodeAllocationCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            try { await CreateSut().Handle(new DeleteBarcodeAllocationCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
