using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveDocumentUpload;
using BackgroundService.Application.Workflow.Common;
using BackgroundService.Application.Workflow.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Commands
{
    public sealed class UploadFileCommandHandlerTests
    {
        private readonly Mock<IFileStorageService> _mockFileStorage = new(MockBehavior.Strict);

        private UploadFileCommandHandler CreateSut() =>
            new(_mockFileStorage.Object);

        [Fact]
        public async Task Handle_ValidFile_ReturnsFileUploadResult()
        {
            var expectedResult = new FileUploadResult
            {
                FileName = "test.pdf",
                RelativePath = "ApproveDocument/test.pdf",
                Url = "/uploads/ApproveDocument/test.pdf"
            };

            _mockFileStorage
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);

            var sut = CreateSut();
            var command = new UploadFileCommand { File = mockFile.Object };

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.FileName.Should().Be("test.pdf");
            result.RelativePath.Should().Be("ApproveDocument/test.pdf");
        }

        [Fact]
        public async Task Handle_CallsSaveFileOnce()
        {
            _mockFileStorage
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync(new FileUploadResult());

            var mockFile = new Mock<IFormFile>();
            var sut = CreateSut();

            await sut.Handle(new UploadFileCommand { File = mockFile.Object }, CancellationToken.None);

            _mockFileStorage.Verify(
                s => s.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
