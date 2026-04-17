using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.UploadGateEntryDocument;

namespace PurchaseManagement.UnitTests.Application.GRN.GateEntry.Commands
{
    public sealed class UploadGateEntryDocumentCommandHandlerTests
    {
        private readonly Mock<IGateEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UploadGateEntryDocumentCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadGateEntryDocumentCommand { File = null };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*No file uploaded*");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            var command = new UploadGateEntryDocumentCommand { File = mockFile.Object };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyBaseDirectory_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns("doc.pdf");
            var command = new UploadGateEntryDocumentCommand { File = mockFile.Object };
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
