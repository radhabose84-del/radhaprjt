using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.UserLogin.Commands.DeactivateUserSession;

namespace UserManagement.UnitTests.Application.UserLogin.Commands
{
    public sealed class DeactivateUserSessionCommandHandlerTests
    {
        private readonly Mock<IUserSessionRepository> _mockSessionRepo = new(MockBehavior.Strict);

        private DeactivateUserSessionCommandHandler CreateSut() =>
            new(_mockSessionRepo.Object);

        [Fact]
        public async Task Handle_ActiveSession_ReturnsTrue()
        {
            _mockSessionRepo.Setup(r => r.DeactivateUserSessionsByUsername("user1")).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeactivateUserSessionCommand { Username = "user1" }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoSession_ReturnsFalse()
        {
            _mockSessionRepo.Setup(r => r.DeactivateUserSessionsByUsername("unknown")).ReturnsAsync(false);

            var result = await CreateSut().Handle(new DeactivateUserSessionCommand { Username = "unknown" }, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
