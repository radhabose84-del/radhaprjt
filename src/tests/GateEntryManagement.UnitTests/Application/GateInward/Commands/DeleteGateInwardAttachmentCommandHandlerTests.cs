using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInwardAttachment;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GateInward.Commands
{
    public sealed class DeleteGateInwardAttachmentCommandHandlerTests
    {
        private readonly Mock<IGateInwardCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IGateInwardAttachmentFileStorage> _mockStorage = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteGateInwardAttachmentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockStorage.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AttachmentExists_DeletesFileAndReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.ClearAttachmentAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync("GateEntry/abc.pdf");
            _mockStorage
                .Setup(s => s.DeleteAsync("GateEntry/abc.pdf", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteGateInwardAttachmentCommand(1), CancellationToken.None);

            result.Should().BeTrue();
            _mockStorage.Verify(s => s.DeleteAsync("GateEntry/abc.pdf", It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GATEINWARD_ATTACHMENT_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoAttachment_ReturnsFalseAndSkipsDelete()
        {
            _mockCommandRepo
                .Setup(r => r.ClearAttachmentAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var result = await CreateSut().Handle(new DeleteGateInwardAttachmentCommand(9), CancellationToken.None);

            result.Should().BeFalse();
            _mockStorage.Verify(
                s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
