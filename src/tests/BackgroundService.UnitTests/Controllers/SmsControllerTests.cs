using Contracts.Events.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BackgroundService.Presentation.Controllers;

namespace BackgroundService.UnitTests.Controllers
{
    public sealed class SmsControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private SmsController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task SendSms_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendSmsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().SendSms(new SendSmsCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SendSms_WhenFailure_ReturnsBadRequestResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendSmsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().SendSms(new SendSmsCommand());

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task SendSms_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendSmsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().SendSms(new SendSmsCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<SendSmsCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
