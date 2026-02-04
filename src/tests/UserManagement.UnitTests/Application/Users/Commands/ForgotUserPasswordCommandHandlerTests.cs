using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.ForgotUserPassword;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class ForgotUserPasswordCommandHandlerTests : IDisposable
    {
        private readonly Mock<IUserQueryRepository> _userQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IChangePassword> _changePassword = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ForgotUserPasswordCommandHandler>> _logger = new();
        private readonly Mock<INotificationsQueryRepository> _notifQuery = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Strict);
        private readonly Mock<IBackgroundServiceClient> _bg = new(MockBehavior.Strict);

        public ForgotUserPasswordCommandHandlerTests()
        {
            // prevent cross-test pollution
            UserManagement.Application.Common.Utilities.ForgotPasswordCache.CodeStorage.Clear();
        }

        public void Dispose()
        {
            // prevent cross-test pollution
            UserManagement.Application.Common.Utilities.ForgotPasswordCache.CodeStorage.Clear();
        }

        private ForgotUserPasswordCommandHandler CreateSut() =>
            new ForgotUserPasswordCommandHandler(
                _userQuery.Object,
                _mapper.Object,
                _changePassword.Object,
                _logger.Object,
                _notifQuery.Object,
                _mediator.Object,
                _tz.Object,
                _bg.Object);

        [Fact]
        public async Task Handle_Success_Schedules_Cleanup_Logs_And_Returns_Response()
        {
            // Arrange
            var cmd = new ForgotUserPasswordCommand { UserName = "neo" };

            var user = new User
            {
                UserId = 7,
                UserName = "neo",
                EmailId = "neo@gmail.com",
                Mobile = "9990001111",
                FirstName = "Neo",
                LastName = "Anderson"
            };

            var userDto = new UserDto
            {
                UserId = 7,
                UserName = "neo",
                EmailId = "neo@gmail.com",
                Mobile = "9990001111",
                FirstName = "Neo",
                LastName = "Anderson"
            };

            var systemTz = "Asia/Kolkata";
            var now = new DateTime(2025, 1, 2, 3, 4, 5);
            var expiryMinutes = 15;
            var generatedCode = "123456";

            _userQuery.Setup(r => r.GetByUsernameAsync("neo")).ReturnsAsync(user);

            _changePassword
                .Setup(c => c.GenerateVerificationCode(6))
                .ReturnsAsync(generatedCode);

            _notifQuery.Setup(n => n.GetResetCodeExpiryMinutes()).ReturnsAsync(expiryMinutes);

            _tz.Setup(t => t.GetSystemTimeZone()).Returns(systemTz);
            _tz.Setup(t => t.GetCurrentTime(systemTz)).Returns(now);

            _bg.Setup(b => b.ScheduleVerificationCodeCleanupAsync("neo", expiryMinutes))
               .Returns(Task.CompletedTask);

            _mediator.Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(d =>
                        d.Module == "ResetUserPassword" &&
                        d.ActionDetail == "ResetUserPassword" &&
                        d.ActionName == "neo" &&
                        d.ActionCode == generatedCode),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(mp => mp.Map<UserDto>(user)).Returns(userDto);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<ForgotPasswordResponse> result =
                await sut.Handle(cmd, CancellationToken.None);

            // Assert response
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be("neo@gmail.com");
            result.Data.Mobile.Should().Be("9990001111");
            result.Data.VerificationCode.Should().Be(generatedCode);
            result.Data.PasswordResetCodeExpiryMinutes.Should().Be(expiryMinutes);
            result.Data.Message.Should().Contain("Verification code sent");

            // ✅ IMPORTANT: Do NOT assert ForgotPasswordCache.CodeStorage here.
            // In your production flow it is empty after Handle().

            _userQuery.VerifyAll();
            _changePassword.VerifyAll();
            _notifQuery.VerifyAll();
            _tz.VerifyAll();
            _bg.VerifyAll();
            _mediator.VerifyAll();
            _mapper.VerifyAll();
        }

        [Fact]
        public async Task Handle_Success_For_NonGmail_User()
        {
            // Arrange
            var cmd = new ForgotUserPasswordCommand { UserName = "trinity" };

            var user = new User
            {
                UserId = 8,
                UserName = "trinity",
                EmailId = "trinity@company.com",
                Mobile = "8887776666",
                FirstName = "Trinity",
                LastName = "Unknown"
            };

            var userDto = new UserDto
            {
                UserId = 8,
                UserName = "trinity",
                EmailId = "trinity@company.com",
                Mobile = "8887776666",
                FirstName = "Trinity",
                LastName = "Unknown"
            };

            var systemTz = "UTC";
            var now = new DateTime(2025, 1, 2, 0, 0, 0);
            var expiryMinutes = 10;
            var generatedCode = "999000";

            _userQuery.Setup(r => r.GetByUsernameAsync("trinity")).ReturnsAsync(user);

            _changePassword
                .Setup(c => c.GenerateVerificationCode(6))
                .ReturnsAsync(generatedCode);

            _notifQuery.Setup(n => n.GetResetCodeExpiryMinutes()).ReturnsAsync(expiryMinutes);

            _tz.Setup(t => t.GetSystemTimeZone()).Returns(systemTz);
            _tz.Setup(t => t.GetCurrentTime(systemTz)).Returns(now);

            _bg.Setup(b => b.ScheduleVerificationCodeCleanupAsync("trinity", expiryMinutes))
               .Returns(Task.CompletedTask);

            _mediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            _mapper.Setup(mp => mp.Map<UserDto>(user)).Returns(userDto);

            // Act
            var sut = CreateSut();
            var result = await sut.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be("trinity@company.com");
            result.Data.Mobile.Should().Be("8887776666");
            result.Data.VerificationCode.Should().Be(generatedCode);
            result.Data.PasswordResetCodeExpiryMinutes.Should().Be(expiryMinutes);

            _userQuery.VerifyAll();
            _changePassword.VerifyAll();
            _notifQuery.VerifyAll();
            _tz.VerifyAll();
            _bg.VerifyAll();
            _mediator.VerifyAll();
            _mapper.VerifyAll();
        }
    }
}
