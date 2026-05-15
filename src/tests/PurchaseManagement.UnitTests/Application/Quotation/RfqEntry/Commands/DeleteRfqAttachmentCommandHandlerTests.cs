using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Commands
{
    public sealed class DeleteRfqAttachmentCommandHandlerTests
    {
        private readonly Mock<IRfqCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IRfqAttachmentFileStorage> _mockStorage = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRfqAttachmentCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockStorage.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AttachmentExists_ReturnsTrue()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 7, It.IsAny<CancellationToken>()))
                .ReturnsAsync("/Resources/Purchase/RfqAttachments/abc.pdf");

            var result = await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 7), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AttachmentExists_DeletesPhysicalFile()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 7, It.IsAny<CancellationToken>()))
                .ReturnsAsync("/Resources/Purchase/RfqAttachments/abc.pdf");

            await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 7), CancellationToken.None);

            _mockStorage.Verify(
                s => s.DeleteAsync("/Resources/Purchase/RfqAttachments/abc.pdf", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AttachmentExists_PublishesAuditEvent()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 7, It.IsAny<CancellationToken>()))
                .ReturnsAsync("/Resources/Purchase/RfqAttachments/abc.pdf");

            await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "RFQ_ATTACHMENT_DELETE" &&
                        e.Module == "RFQ"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AttachmentNotFound_ReturnsFalse()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var result = await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 99), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AttachmentNotFound_DoesNotDeleteFile()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 99), CancellationToken.None);

            _mockStorage.Verify(
                s => s.DeleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_AttachmentNotFound_DoesNotPublishAuditEvent()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAttachmentAsync(1, 99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            await CreateSut().Handle(new DeleteRfqAttachmentCommand(1, 99), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
