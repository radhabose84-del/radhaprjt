using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.UploadDocument;

namespace PurchaseManagement.UnitTests.Application.OCREntry.Commands
{
    public sealed class UploadOCRDocumentCommandHandlerTests
    {
        private readonly Mock<IOCREntryFileStorage> _mockStorage = new(MockBehavior.Loose);

        private UploadOCRDocumentCommandHandler CreateSut() => new(_mockStorage.Object);

        private static UploadOCRDocumentCommand BuildCommand(
            string fileName = "ocr.png",
            long length = 1024,
            string contentType = "image/png")
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            return new UploadOCRDocumentCommand { File = fileMock.Object };
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSavedMetadata()
        {
            var command = BuildCommand(fileName: "ocr.png", length: 4096, contentType: "image/png");
            _mockStorage
                .Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedOCRDocument(
                    FileName: "TEMP_guid.png",
                    OriginalFileName: "ocr.png",
                    FileSize: 4096,
                    FileType: "image/png"));

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.FileName.Should().Be("TEMP_guid.png");
            result.OriginalFileName.Should().Be("ocr.png");
            result.FileSize.Should().Be(4096);
            result.FileType.Should().Be("image/png");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsStorageOnce()
        {
            var command = BuildCommand();
            _mockStorage
                .Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedOCRDocument("TEMP_x.png", "x.png", 1, null));

            await CreateSut().Handle(command, CancellationToken.None);

            _mockStorage.Verify(
                s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PassesIncomingFile_ToStorage()
        {
            var command = BuildCommand();
            IFormFile? captured = null;
            _mockStorage
                .Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .Callback<IFormFile, CancellationToken>((f, _) => captured = f)
                .ReturnsAsync(new SavedOCRDocument("TEMP_x.png", "x.png", 1, null));

            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            captured.Should().BeSameAs(command.File);
        }
    }
}
