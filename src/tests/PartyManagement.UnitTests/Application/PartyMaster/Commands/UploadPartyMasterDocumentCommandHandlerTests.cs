using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.UploadPartyMasterDocument;

namespace PartyManagement.UnitTests.Application.PartyMaster.Commands
{
    public sealed class UploadPartyMasterDocumentCommandHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UploadPartyMasterDocumentCommnadHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadPartyMasterDocumentCommand { File = null };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No file uploaded*");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(0);

            var command = new UploadPartyMasterDocumentCommand { File = fileMock.Object };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No file uploaded*");
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsValidationException()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");

            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync(string.Empty);

            var command = new UploadPartyMasterDocumentCommand { File = fileMock.Object };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Base directory not configured*");
        }

        [Fact]
        public async Task Handle_NullBaseDirectory_ThrowsValidationException()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");

            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync((string?)null!);

            var command = new UploadPartyMasterDocumentCommand { File = fileMock.Object };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Base directory not configured*");
        }

        [Fact]
        public async Task Handle_ValidFile_CallsGetDocumentDirectory()
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Loose);
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockQueryRepo.Setup(r => r.GetDocumentDirectoryAsync()).ReturnsAsync("PartyDocs");

            var command = new UploadPartyMasterDocumentCommand { File = fileMock.Object };

            try
            {
                await CreateSut().Handle(command, CancellationToken.None);
            }
            catch
            {
                // File system operations may fail in test; we only verify the repo call
            }

            _mockQueryRepo.Verify(r => r.GetDocumentDirectoryAsync(), Times.Once);
        }
    }
}
