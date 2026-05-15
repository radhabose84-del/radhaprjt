using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UploadAttachment;

namespace PurchaseManagement.UnitTests.Application.Quotation.RfqEntry.Commands
{
    public sealed class UploadRfqAttachmentCommandHandlerTests
    {
        private readonly Mock<IRfqAttachmentFileStorage> _mockStorage = new(MockBehavior.Loose);

        private UploadRfqAttachmentCommandHandler CreateSut() => new(_mockStorage.Object);

        private static UploadRfqAttachmentCommand BuildCommand(
            string fileName = "specs.pdf",
            long length = 1024,
            string contentType = "application/pdf")
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            return new UploadRfqAttachmentCommand { File = fileMock.Object };
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsStagedMetadata()
        {
            var command = BuildCommand(fileName: "specs.pdf", length: 4096, contentType: "application/pdf");
            _mockStorage
                .Setup(s => s.SaveToStagingAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StagedRfqAttachment(
                    FileName: "TEMP_abc.pdf",
                    OriginalFileName: "specs.pdf",
                    FileSize: 4096,
                    FileType: "application/pdf"));

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.FileName.Should().Be("TEMP_abc.pdf");
            result.OriginalFileName.Should().Be("specs.pdf");
            result.FileSize.Should().Be(4096);
            result.FileType.Should().Be("application/pdf");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsStorageOnce()
        {
            var command = BuildCommand();
            _mockStorage
                .Setup(s => s.SaveToStagingAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StagedRfqAttachment("TEMP_x.pdf", "x.pdf", 1, null));

            await CreateSut().Handle(command, CancellationToken.None);

            _mockStorage.Verify(
                s => s.SaveToStagingAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PassesIncomingFile_ToStorage()
        {
            var command = BuildCommand();
            IFormFile? captured = null;
            _mockStorage
                .Setup(s => s.SaveToStagingAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .Callback<IFormFile, CancellationToken>((f, _) => captured = f)
                .ReturnsAsync(new StagedRfqAttachment("TEMP_x.pdf", "x.pdf", 1, null));

            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            captured.Should().BeSameAs(command.File);
        }
    }
}
