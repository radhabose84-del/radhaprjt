using BackgroundService.Infrastructure.Services;
using MediatR;
using UserManagement.Application.UserLogin.Commands.UnlockUser;

namespace BackgroundService.UnitTests.Application.UserUnlock
{
    public sealed class UserUnlockServiceTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private UserUnlockService CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task UnlockUser_DispatchesUnlockUserCommandViaMediator()
        {
            const string userName = "alice";
            UnlockUserCommand? captured = null;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((cmd, _) => captured = (UnlockUserCommand)cmd)
                .ReturnsAsync(true);

            await CreateSut().UnlockUser(userName);

            captured.Should().NotBeNull();
            captured!.userName.Should().Be(userName);
        }

        [Fact]
        public async Task UnlockUser_CallsMediatorSend_ExactlyOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().UnlockUser("bob");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

    }
}
