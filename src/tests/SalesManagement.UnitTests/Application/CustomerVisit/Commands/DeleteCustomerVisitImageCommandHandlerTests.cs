using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Commands
{
    public class DeleteCustomerVisitImageCommandHandlerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteCustomerVisitImageCommandHandler>> _mockLogger = new();

        private DeleteCustomerVisitImageCommandHandler CreateSut() =>
            new(_mockFileUpload.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NullImagePath_ReturnsFalse()
        {
            var command = new DeleteCustomerVisitImageCommand { ImagePath = null };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EmptyImagePath_ReturnsFalse()
        {
            var command = new DeleteCustomerVisitImageCommand { ImagePath = "" };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WhitespaceImagePath_ReturnsFalse()
        {
            var command = new DeleteCustomerVisitImageCommand { ImagePath = "   " };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidImagePath_CallsDeleteFileAsync()
        {
            _mockFileUpload
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var command = new DeleteCustomerVisitImageCommand { ImagePath = "test-image.jpg" };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            _mockFileUpload.Verify(f => f.DeleteFileAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FileServiceReturnsFalse_ReturnsFalse()
        {
            _mockFileUpload
                .Setup(f => f.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var command = new DeleteCustomerVisitImageCommand { ImagePath = "nonexistent.jpg" };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
