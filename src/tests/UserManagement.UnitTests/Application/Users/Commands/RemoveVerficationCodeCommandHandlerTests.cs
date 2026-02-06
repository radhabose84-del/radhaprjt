using System.Threading;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.RemoveVerificationCode;
using FluentAssertions;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class RemoveVerficationCodeCommandHandlerTests
    {
        private readonly Mock<IUserCommandRepository> _repo = new(MockBehavior.Strict);

        private RemoveVerficationCodeCommandHandler CreateSut()
            => new RemoveVerficationCodeCommandHandler(_repo.Object);

        [Fact]
        public async Task Handle_WhenRepositoryReturnsTrue_StillReturnsSuccessTrue()
        {
            // Arrange
            var cmd = new RemoveVerficationCodeCommand { UserName = "neo" };
            _repo.Setup(r => r.RemoveVerficationCode("neo")).ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<bool> result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            _repo.Verify(r => r.RemoveVerficationCode("neo"), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenRepositoryReturnsFalse_StillReturnsSuccessTrue_PerCurrentLogic()
        {
            // Arrange
            var cmd = new RemoveVerficationCodeCommand { UserName = "trinity" };
            _repo.Setup(r => r.RemoveVerficationCode("trinity")).ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(cmd, CancellationToken.None);

            // Assert (matches the handler’s current behavior)
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            _repo.Verify(r => r.RemoveVerficationCode("trinity"), Times.Once);
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WithEmptyUsername_InvokesRepository_AndReturnsSuccess()
        {
            // Arrange
            var cmd = new RemoveVerficationCodeCommand { UserName = string.Empty };
            _repo.Setup(r => r.RemoveVerficationCode(string.Empty)).ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            _repo.Verify(r => r.RemoveVerficationCode(string.Empty), Times.Once);
            _repo.VerifyNoOtherCalls();
        }
    }
}
