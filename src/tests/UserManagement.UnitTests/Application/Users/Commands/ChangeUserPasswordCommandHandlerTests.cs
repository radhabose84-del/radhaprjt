using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using UserManagement.Application.Users.Commands.CreateFirstTimeUserPassword; // for PasswordLog if it's here; adjust if needed
using UserManagement.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class ChangeUserPasswordCommandHandlerTests
    {
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _userQuery = new(MockBehavior.Strict); // not used by handler, keep Strict to catch accidental calls
        private readonly Mock<IChangePassword> _changePwd = new(MockBehavior.Strict);

        private ChangeUserPasswordCommandHandler CreateSut()
            => new(_mapper.Object, _userQuery.Object, _changePwd.Object);

        private static ChangeUserPasswordCommand MakeRequest(int userId = 1, string? newPwd = "P@ssw0rd!")
            => new ChangeUserPasswordCommand { UserId = userId, NewPassword = newPwd };

        [Fact]
        public async Task Handle_When_NewPassword_Is_Null_Or_Empty_Returns_Failure()
        {
            var sut = CreateSut();

            // null
            var r1 = await sut.Handle(MakeRequest(newPwd: null), CancellationToken.None);
            r1.IsSuccess.Should().BeFalse();
            r1.Message.Should().Be("Invalid input parameters.");

            // empty
            var r2 = await sut.Handle(MakeRequest(newPwd: ""), CancellationToken.None);
            r2.IsSuccess.Should().BeFalse();
            r2.Message.Should().Be("Invalid input parameters.");

            // Ensure no calls were made
            _mapper.VerifyNoOtherCalls();
            _changePwd.VerifyNoOtherCalls();
            _userQuery.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_When_ChangePassword_Returns_False_Returns_Failure()
        {
            // Arrange
            var req = MakeRequest();
            var mappedLog = new PasswordLog(); // whatever concrete type is used

            _mapper.Setup(m => m.Map<PasswordLog>(req))
                   .Returns(mappedLog);

            _changePwd.Setup(c => c.PasswordEncode(req.NewPassword!))
                      .ReturnsAsync("ENCODED");

            // Verify the PasswordLog passed to ChangePassword contains the encoded hash
            _changePwd.Setup(c => c.ChangePassword(
                                req.UserId,
                                req.NewPassword!,
                                It.Is<PasswordLog>(pl => pl.PasswordHash == "ENCODED")))
                      .ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(req, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Try a different Password");

            _mapper.VerifyAll();
            _changePwd.VerifyAll();
            _userQuery.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_When_Success_Returns_Success_Response()
        {
            // Arrange
            var req = MakeRequest();
            var mappedLog = new PasswordLog();

            _mapper.Setup(m => m.Map<PasswordLog>(req))
                   .Returns(mappedLog);

            _changePwd.Setup(c => c.PasswordEncode(req.NewPassword!))
                      .ReturnsAsync("ENCODED");

            _changePwd.Setup(c => c.ChangePassword(
                                req.UserId,
                                req.NewPassword!,
                                It.Is<PasswordLog>(pl => pl.PasswordHash == "ENCODED")))
                      .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(req, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Password changed successfully.");

            _mapper.VerifyAll();
            _changePwd.VerifyAll();
            _userQuery.VerifyNoOtherCalls();
        }
    }
}
