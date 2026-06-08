using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteDocument;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Commands
{
    public sealed class DeleteRawMaterialPODocumentCommandHandlerTests
    {
        private readonly Mock<IRawMaterialPOCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOFileStorage> _mockStorage = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRawMaterialPODocumentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockStorage.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_FileDeleted_ReturnsTrue()
        {
            _mockStorage.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new DeleteRawMaterialPODocumentCommand("RMPO-2026-0001.png"), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FileDeleted_ClearsDbReferenceAndPublishesAudit()
        {
            _mockStorage.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(
                new DeleteRawMaterialPODocumentCommand("RMPO-2026-0001.png"), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.ClearDocumentPathByFileNameAsync("RMPO-2026-0001.png", It.IsAny<CancellationToken>()),
                Times.Once);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FileNotFound_ReturnsFalse_NoAudit()
        {
            _mockStorage.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateSut().Handle(
                new DeleteRawMaterialPODocumentCommand("missing.png"), CancellationToken.None);

            result.Should().BeFalse();
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EmptyFileName_ReturnsFalse()
        {
            var result = await CreateSut().Handle(
                new DeleteRawMaterialPODocumentCommand(""), CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
