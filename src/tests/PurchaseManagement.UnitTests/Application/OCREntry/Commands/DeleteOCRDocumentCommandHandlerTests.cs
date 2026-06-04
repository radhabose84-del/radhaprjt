using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Commands
{
    public sealed class DeleteOCRDocumentCommandHandlerTests
    {
        private const string FileName = "TEMP_905dddb8-9bc5-4667-a2a4-ac9274946078.png";

        private readonly Mock<IOCREntryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IOCREntryFileStorage> _mockStorage = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteOCRDocumentCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockStorage.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_FileDeleted_ReturnsTrue()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FileDeleted_DeletesPhysicalFileByName()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            _mockStorage.Verify(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FileDeleted_ClearsDbReference()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            _mockRepo.Verify(
                r => r.ClearDocumentPathByFileNameAsync(FileName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FileDeleted_PublishesAuditEvent()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.ActionCode == "OCR_DOCUMENT_DELETE" &&
                        e.Module == "OCREntry"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FullUrlInput_DeletesByUrl_ClearsByBareName()
        {
            const string fullUrl = "http://192.168.1.126/Resources/Purchase/OCRDocuments/Acme/Unit1/OCR-2026-0001.png";
            const string bareName = "OCR-2026-0001.png";
            _mockStorage
                .Setup(s => s.DeleteAsync(fullUrl, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteOCRDocumentCommand(fullUrl), CancellationToken.None);

            result.Should().BeTrue();
            // Storage receives the full URL (it maps it to the local path); DB clear uses the bare name.
            _mockStorage.Verify(s => s.DeleteAsync(fullUrl, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepo.Verify(
                r => r.ClearDocumentPathByFileNameAsync(bareName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FileNotFound_ReturnsFalse()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_FileNotFound_DoesNotPublishAuditEvent()
        {
            _mockStorage
                .Setup(s => s.DeleteAsync(FileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await CreateSut().Handle(new DeleteOCRDocumentCommand(FileName), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
