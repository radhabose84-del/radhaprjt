using BackgroundService.Application.Email;
using BackgroundService.Application.Notification.Common.Interfaces;
using Contracts.Events.Notifications;

namespace BackgroundService.UnitTests.Application.Email
{
    public sealed class SendSmsCommandHandlerTests
    {
        private readonly Mock<ISmsService> _mockSmsService = new(MockBehavior.Strict);

        private SendSmsCommandHandler CreateSut() =>
            new(_mockSmsService.Object);

        [Fact]
        public async Task Handle_ShouldReturn_True_WhenSmsServiceReturnsTrue()
        {
            var command = new SendSmsCommand { to = "9999999999", message = "Hello" };
            _mockSmsService.Setup(s => s.SendSmsAsync(command)).ReturnsAsync(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturn_False_WhenSmsServiceReturnsFalse()
        {
            var command = new SendSmsCommand { to = "1", message = "m" };
            _mockSmsService.Setup(s => s.SendSmsAsync(command)).ReturnsAsync(false);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldCall_SmsService_Once_With_SameCommand()
        {
            var command = new SendSmsCommand { to = "111", message = "hi" };
            _mockSmsService.Setup(s => s.SendSmsAsync(command)).ReturnsAsync(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockSmsService.Verify(s => s.SendSmsAsync(command), Times.Once);
        }
    }
}
