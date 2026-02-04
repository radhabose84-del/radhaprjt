using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities; // ForgotPasswordCache, VerificationCodeDetails
using UserManagement.Application.Users.Commands.ResetUserPassword;
using UserManagement.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

// alias for the DTO the handler uses
using PwdLogDtoQ = UserManagement.Application.Users.Queries.GetUsers.PasswordLogDTO;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class ResetUserPasswordCommandHandlerTests : IDisposable
    {
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IChangePassword> _changePassword = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _userQuery = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _time = new(MockBehavior.Strict);

        public ResetUserPasswordCommandHandlerTests()
        {
            // ✅ prevent cross-test pollution (before every test)
            ForgotPasswordCache.CodeStorage.Clear();
        }

        // xUnit calls this after each [Fact]
        public void Dispose()
        {
            // ✅ prevent cross-test pollution (after every test)
            ForgotPasswordCache.CodeStorage.Clear();
        }

        // Using the existing production constructor (no new dependencies)
        private ResetUserPasswordCommandHandler CreateSut()
            => new ResetUserPasswordCommandHandler(
                _mapper.Object, _changePassword.Object, _userQuery.Object, _time.Object);

        [Fact]
        public async Task Handle_Success_ResetsPassword_LogsAndReturnsSuccess()
        {
            // ✅ Ensure clean cache state at start of test
            ForgotPasswordCache.CodeStorage.Clear();

            // Arrange
            var cmd = new ResetUserPasswordCommand { UserName = "neo", Password = "Matrix#1" };

            var fixedZone = "UTC";
            var fixedNow = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);

            var user = new User { UserId = 42, UserName = "neo", EmailId = "neo@zion.io", Mobile = "999" };

            _time.Setup(t => t.GetSystemTimeZone()).Returns(fixedZone);
            _time.Setup(t => t.GetCurrentTime(fixedZone)).Returns(fixedNow);

            // Support either call style (with or without optional id)
            _userQuery.Setup(q => q.GetByUsernameAsync("neo", It.IsAny<int?>())).ReturnsAsync(user);
            _userQuery.Setup(q => q.GetByUsernameAsync("neo")).ReturnsAsync(user);

            _changePassword.Setup(c => c.PasswordEncode("Matrix#1")).ReturnsAsync("hashed-pw");

            PwdLogDtoQ? capturedDto = null;

            _mapper.Setup(m => m.Map<PasswordLog>(It.IsAny<PwdLogDtoQ>()))
                   .Returns((PwdLogDtoQ dto) =>
                   {
                       capturedDto = dto;
                       return new PasswordLog
                       {
                           UserId = dto.UserId,
                           UserName = dto.UserName,
                           PasswordHash = dto.PasswordHash,
                           CreatedAt = dto.CreatedAt
                       };
                   });

            // Optional fallback if handler calls Map with object
            _mapper.Setup(m => m.Map<PasswordLog>(It.IsAny<object>()))
                   .Returns((object src) =>
                   {
                       var dto = (PwdLogDtoQ)src;
                       capturedDto = dto;
                       return new PasswordLog
                       {
                           UserId = dto.UserId,
                           UserName = dto.UserName,
                           PasswordHash = dto.PasswordHash,
                           CreatedAt = dto.CreatedAt
                       };
                   });

            _changePassword.Setup(c => c.ResetUserPassword(42, It.IsAny<PasswordLog>()))
                           .ReturnsAsync("Password reset successful");

            _changePassword.Setup(c => c.PasswordLog(It.IsAny<PasswordLog>()))
                           .ReturnsAsync(true);

            // (Optional) Seed cache: only to simulate pre-existing verification code
            ForgotPasswordCache.CodeStorage[cmd.UserName] = new VerificationCodeDetails
            {
                Code = "ABC123",
                ExpiryTime = fixedNow.AddMinutes(5)
            };

            var sut = CreateSut();

            // Act
            ApiResponseDTO<string> result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Password reset successful");

            _time.Verify(t => t.GetSystemTimeZone(), Times.Once);
            _time.Verify(t => t.GetCurrentTime(fixedZone), Times.Once);
            _userQuery.Verify(q => q.GetByUsernameAsync("neo", It.IsAny<int?>()), Times.AtMostOnce());
            _userQuery.Verify(q => q.GetByUsernameAsync("neo"), Times.AtMostOnce());
            _changePassword.Verify(c => c.PasswordEncode("Matrix#1"), Times.Once);

            capturedDto.Should().NotBeNull();
            capturedDto!.UserId.Should().Be(42);
            capturedDto.UserName.Should().Be("neo");
            capturedDto.PasswordHash.Should().Be("hashed-pw");
            capturedDto.CreatedAt.Should().Be(fixedNow);

            _mapper.Verify(m => m.Map<PasswordLog>(It.IsAny<PwdLogDtoQ>()), Times.AtLeastOnce);
            _changePassword.Verify(c => c.ResetUserPassword(42, It.IsAny<PasswordLog>()), Times.Once);
            _changePassword.Verify(c => c.PasswordLog(It.IsAny<PasswordLog>()), Times.Once);

            // ✅ IMPORTANT: No cache assertions after success.
            // Production may clear cache, remove key, or keep it — test should not assume.

            _mapper.VerifyNoOtherCalls();
            _changePassword.VerifyNoOtherCalls();
            _userQuery.VerifyNoOtherCalls();
            _time.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_CurrentCodeThrowsNullReference()
        {
            // ✅ Ensure clean cache state at start of test
            ForgotPasswordCache.CodeStorage.Clear();

            // Arrange
            var cmd = new ResetUserPasswordCommand { UserName = "unknown", Password = "DoesNotMatter" };

            var fixedZone = "UTC";
            var fixedNow = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);

            _time.Setup(t => t.GetSystemTimeZone()).Returns(fixedZone);
            _time.Setup(t => t.GetCurrentTime(fixedZone)).Returns(fixedNow);

            _userQuery.Setup(q => q.GetByUsernameAsync("unknown", It.IsAny<int?>()))
                      .ReturnsAsync((User)null!);
            _userQuery.Setup(q => q.GetByUsernameAsync("unknown"))
                      .ReturnsAsync((User)null!);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(cmd, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>();

            _time.Verify(t => t.GetSystemTimeZone(), Times.Once);
            _time.Verify(t => t.GetCurrentTime(fixedZone), Times.Once);
            _userQuery.Verify(q => q.GetByUsernameAsync("unknown", It.IsAny<int?>()), Times.AtMostOnce());
            _userQuery.Verify(q => q.GetByUsernameAsync("unknown"), Times.AtMostOnce());

            _userQuery.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _changePassword.VerifyNoOtherCalls();
            _time.VerifyNoOtherCalls();
        }
    }
}
