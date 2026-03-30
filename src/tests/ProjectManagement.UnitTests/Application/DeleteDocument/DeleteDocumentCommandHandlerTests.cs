using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.DeleteDocument;

namespace ProjectManagement.UnitTests.Application.DeleteDocument
{
    public sealed class DeleteDocumentCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUploadDocumentQueryRepository> _mockDocRepo = new(MockBehavior.Strict);

        private DeleteDocumentCommandHandler CreateSut() =>
            new(_mockFileUpload.Object, _mockIpService.Object, _mockDocRepo.Object);

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsException()
        {
            _mockDocRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync(string.Empty);

            var command = new DeleteDocumentCommand
            {
                Id = 1,
                ProjectId = 1,
                ProjectDocumentPath = "test/doc.pdf",
                FileName = "doc.pdf"
            };

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Base directory not configured.");
        }

        [Fact]
        public async Task Handle_WhitespaceBaseDirectory_ThrowsException()
        {
            _mockDocRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync("   ");

            var command = new DeleteDocumentCommand
            {
                Id = 1,
                ProjectId = 1,
                ProjectDocumentPath = "test/doc.pdf",
                FileName = "doc.pdf"
            };

            var act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Base directory not configured.");
        }

        [Fact]
        public async Task Handle_CallsGetDocumentDirectoryOnce()
        {
            _mockDocRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync(string.Empty);

            try
            {
                await CreateSut().Handle(
                    new DeleteDocumentCommand { Id = 1, ProjectId = 1, ProjectDocumentPath = "p.pdf" },
                    CancellationToken.None);
            }
            catch { /* expected */ }

            _mockDocRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Once);
        }
    }
}
