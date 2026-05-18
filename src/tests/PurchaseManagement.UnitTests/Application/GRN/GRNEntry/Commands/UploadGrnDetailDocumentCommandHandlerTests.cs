using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument;

namespace PurchaseManagement.UnitTests.Application.GRN.GRNEntry.Commands
{
    public sealed class UploadGrnDetailDocumentCommandHandlerTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UploadGrnDetailDocumentCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadGrnDetailDocumentCommand { File = null };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*No file uploaded*");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var command = new UploadGrnDetailDocumentCommand { File = mockFile.Object };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("line.png");
            var command = new UploadGrnDetailDocumentCommand { File = mockFile.Object };
            _mockRepo.Setup(r => r.GetDocumentDirectoryAsync())
                .ReturnsAsync(string.Empty);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*Base directory not configured*");
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
