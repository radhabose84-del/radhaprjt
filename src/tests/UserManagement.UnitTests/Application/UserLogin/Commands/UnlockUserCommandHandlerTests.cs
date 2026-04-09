using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.UserLogin.Commands.UnlockUser;

namespace UserManagement.UnitTests.Application.UserLogin.Commands
{
    public sealed class UnlockUserCommandHandlerTests
    {
        private readonly Mock<IUserCommandRepository> _mockUserRepo = new(MockBehavior.Strict);

        private UnlockUserCommandHandler CreateSut() =>
            new(_mockUserRepo.Object);

        [Fact]
        public async Task Handle_LockedUser_ReturnsTrue()
        {
            _mockUserRepo.Setup(r => r.UnlockUser("lockeduser")).ReturnsAsync(true);

            var result = await CreateSut().Handle(new UnlockUserCommand { userName = "lockeduser" }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UnlockFails_ThrowsException()
        {
            _mockUserRepo.Setup(r => r.UnlockUser("unknown")).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new UnlockUserCommand { userName = "unknown" }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
