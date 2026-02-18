using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using UserManagement.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class FirstTimeUserPasswordCommandHandlerTests
    {
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IChangePassword> _change = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _query = new(MockBehavior.Strict);

        private FirstTimeUserPasswordCommandHandler CreateSut()
            => new FirstTimeUserPasswordCommandHandler(_mapper.Object, _change.Object, _query.Object);

        [Fact]
        public async Task Handle_WhenPasswordMatchesExisting_ReturnsFailure_MessageAboutDefaultPassword()
        {
            // Arrange
            var cmd = new FirstTimeUserPasswordCommand
            {
                UserId = 7,
                UserName = "john",
                Password = "Default#1"
            };

            // Generate a valid BCrypt hash for "Default#1" so BCrypt.Verify will be true.
            var existingUser = new User
            {
                UserId = 7,
                UserName = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Default#1")
            };

            _query.Setup(q => q.GetByIdAsync(7)).ReturnsAsync(existingUser);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Your input password should not match the default password. Please try a different password.");

            _query.Verify(q => q.GetByIdAsync(7), Times.Once);
            _query.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _change.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenPasswordDifferent_AndChangeSucceeds_ReturnsSuccess()
        {
            // Arrange
            var cmd = new FirstTimeUserPasswordCommand
            {
                UserId = 7,
                UserName = "john",
                Password = "NewStrong#2"
            };

            // Store a hash that is definitely not "NewStrong#2"
            var existingUser = new User
            {
                UserId = 7,
                UserName = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Default#1")
            };

            _query.Setup(q => q.GetByIdAsync(7)).ReturnsAsync(existingUser);

            // Map command -> PasswordLog (handler then overwrites PasswordHash on the mapped object)
            _mapper
                .Setup(m => m.Map<PasswordLog>(It.IsAny<FirstTimeUserPasswordCommand>()))
                .Returns((FirstTimeUserPasswordCommand c) => new PasswordLog { UserId = c.UserId, UserName = c.UserName });

            // Encode new password
            _change.Setup(c => c.PasswordEncode("NewStrong#2"))
                   .ReturnsAsync("hashed-new");

            // Expect to receive the password log with updated hash
            _change.Setup(c => c.FirstTimeUserChangePassword(
                    7,
                    It.Is<PasswordLog>(pl => pl.UserId == 7 && pl.UserName == "john" && pl.PasswordHash == "hashed-new")))
                   .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Password changed successfully.");

            _query.Verify(q => q.GetByIdAsync(7), Times.Once);
            _mapper.Verify(m => m.Map<PasswordLog>(It.IsAny<FirstTimeUserPasswordCommand>()), Times.Once);
            _change.Verify(c => c.PasswordEncode("NewStrong#2"), Times.Once);
            _change.Verify(c => c.FirstTimeUserChangePassword(7, It.IsAny<PasswordLog>()), Times.Once);

            _query.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _change.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenPasswordDifferent_AndChangeFails_ReturnsFailure()
        {
            // Arrange
            var cmd = new FirstTimeUserPasswordCommand
            {
                UserId = 7,
                UserName = "john",
                Password = "Another#3"
            };

            var existingUser = new User
            {
                UserId = 7,
                UserName = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Default#1")
            };

            _query.Setup(q => q.GetByIdAsync(7)).ReturnsAsync(existingUser);

            _mapper
                .Setup(m => m.Map<PasswordLog>(It.IsAny<FirstTimeUserPasswordCommand>()))
                .Returns((FirstTimeUserPasswordCommand c) => new PasswordLog { UserId = c.UserId, UserName = c.UserName });

            _change.Setup(c => c.PasswordEncode("Another#3"))
                   .ReturnsAsync("hashed-another");

            _change.Setup(c => c.FirstTimeUserChangePassword(
                    7,
                    It.Is<PasswordLog>(pl => pl.UserId == 7 && pl.UserName == "john" && pl.PasswordHash == "hashed-another")))
                   .ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Password change failed.");

            _query.Verify(q => q.GetByIdAsync(7), Times.Once);
            _mapper.Verify(m => m.Map<PasswordLog>(It.IsAny<FirstTimeUserPasswordCommand>()), Times.Once);
            _change.Verify(c => c.PasswordEncode("Another#3"), Times.Once);
            _change.Verify(c => c.FirstTimeUserChangePassword(7, It.IsAny<PasswordLog>()), Times.Once);

            _query.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _change.VerifyNoOtherCalls();
        }
    }
}
