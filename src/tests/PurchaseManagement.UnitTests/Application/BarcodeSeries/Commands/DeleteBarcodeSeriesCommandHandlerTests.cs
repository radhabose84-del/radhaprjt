using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.BarcodeSeries.Commands
{
    public sealed class DeleteBarcodeSeriesCommandHandlerTests
    {
        private readonly Mock<IBarcodeSeriesCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteBarcodeSeriesCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteBarcodeSeriesCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteBarcodeSeriesCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteBarcodeSeriesCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            try { await CreateSut().Handle(new DeleteBarcodeSeriesCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
