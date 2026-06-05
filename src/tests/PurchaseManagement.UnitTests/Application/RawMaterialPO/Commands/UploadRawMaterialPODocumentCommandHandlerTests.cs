using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UploadDocument;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Commands
{
    public sealed class UploadRawMaterialPODocumentCommandHandlerTests
    {
        private readonly Mock<IRawMaterialPOFileStorage> _mockStorage = new(MockBehavior.Strict);

        private UploadRawMaterialPODocumentCommandHandler CreateSut() => new(_mockStorage.Object);

        [Fact]
        public async Task Handle_ValidFile_ReturnsStagedMetadata()
        {
            var file = new Mock<IFormFile>().Object;
            _mockStorage
                .Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedRawMaterialPODocument("TEMP_abc.png", "screenshot.png", 1234, "image/png"));

            var result = await CreateSut().Handle(
                new UploadRawMaterialPODocumentCommand { File = file }, CancellationToken.None);

            result.FileName.Should().Be("TEMP_abc.png");
            result.OriginalFileName.Should().Be("screenshot.png");
            result.FileSize.Should().Be(1234);
            result.FileType.Should().Be("image/png");
        }

        [Fact]
        public async Task Handle_ValidFile_CallsSaveOnce()
        {
            var file = new Mock<IFormFile>().Object;
            _mockStorage
                .Setup(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SavedRawMaterialPODocument("TEMP_abc.png", "screenshot.png", 1234, "image/png"));

            await CreateSut().Handle(new UploadRawMaterialPODocumentCommand { File = file }, CancellationToken.None);

            _mockStorage.Verify(s => s.SaveAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
