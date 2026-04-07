using Contracts.Events.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers;

namespace BackgroundService.UnitTests.Controllers
{
    public sealed class EmailControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private EmailController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task SendEmail_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().SendEmail(new SendEmailCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SendEmail_WhenFailure_ReturnsBadRequestResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().SendEmail(new SendEmailCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SendEmail_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().SendEmail(new SendEmailCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
