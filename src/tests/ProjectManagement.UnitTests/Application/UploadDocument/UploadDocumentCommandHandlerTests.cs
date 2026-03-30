using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.UploadDocument;

namespace ProjectManagement.UnitTests.Application.UploadDocument
{
    public sealed class UploadDocumentCommandHandlerTests
    {
        private readonly Mock<IUploadDocumentQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UploadDocumentCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadDocumentCommand { File = null };

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            mockFile.Setup(f => f.FileName).Returns("empty.pdf");

            var command = new UploadDocumentCommand { File = mockFile.Object };

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task Handle_NullFile_DoesNotCallRepository()
        {
            var command = new UploadDocumentCommand { File = null };

            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockRepo.Verify(
                r => r.GetDocumentDirectoryAsync(),
                Times.Never);
        }
    }
}
