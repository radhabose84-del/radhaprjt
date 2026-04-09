using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Commands
{
    public class UploadCustomerVisitImageCommandHandlerTests
    {
        private readonly Mock<ILogger<UploadCustomerVisitImageCommandHandler>> _mockLogger = new();

        private UploadCustomerVisitImageCommandHandler CreateSut() =>
            new(_mockLogger.Object);

        [Fact]
        public async Task Handle_NullFile_ThrowsValidationException()
        {
            var command = new UploadCustomerVisitImageCommand { File = null };

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*File is required*");
        }

        [Fact]
        public async Task Handle_EmptyFile_ThrowsValidationException()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var command = new UploadCustomerVisitImageCommand { File = mockFile.Object };

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*File is required*");
        }
    }
}
