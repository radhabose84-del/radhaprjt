using BackgroundService.Application.Email;
using BackgroundService.Application.Notification.Common.Interfaces;
using Contracts.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace BackgroundService.UnitTests.Application.Email
{
    public sealed class SendEmailCommandHandlerTests
    {
        private readonly Mock<IEmailService> _mockEmailService = new(MockBehavior.Strict);
        private readonly Mock<ILogger<SendEmailCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private SendEmailCommandHandler CreateSut() =>
            new(_mockEmailService.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ShouldReturn_True_WhenEmailServiceReturnsTrue()
        {
            var command = new SendEmailCommand
            {
                ToEmail = "to@example.com",
                Subject = "Subject",
                HtmlContent = "Body"
            };
            _mockEmailService.Setup(s => s.SendEmailAsync(command)).ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturn_False_WhenEmailServiceReturnsFalse()
        {
            var command = new SendEmailCommand { ToEmail = "x@x.com", Subject = "S", HtmlContent = "B" };
            _mockEmailService.Setup(s => s.SendEmailAsync(command)).ReturnsAsync(false);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldCall_EmailService_Once_With_SameCommand()
        {
            var command = new SendEmailCommand { ToEmail = "y@y.com", Subject = "S" };
            _mockEmailService.Setup(s => s.SendEmailAsync(command)).ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockEmailService.Verify(s => s.SendEmailAsync(command), Times.Once);
        }
    }
}
