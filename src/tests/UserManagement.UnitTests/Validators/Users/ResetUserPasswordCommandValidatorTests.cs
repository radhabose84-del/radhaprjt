#nullable disable
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities;
using UserManagement.Application.Users.Commands.ResetUserPassword;
using UserManagement.Domain.Entities; // User
using FluentValidation.TestHelper;
using UserManagement.Presentation.Validation.Users;

namespace UserManagement.UnitTests.Validators.Users
{
    public sealed class ResetUserPasswordCommandValidatorTests
    {
        private readonly Mock<IChangePassword> _changePassword = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _userRepo   = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _tz            = new(MockBehavior.Strict);

        private static ResetUserPasswordCommand ValidCommand() => new()
        {
            UserName = "neo",
            Password = "Str0ng#Pass",
            VerificationCode = "123456"
        };

        private ResetUserPasswordCommandValidator CreateValidator()
            => new(_changePassword.Object, _userRepo.Object, _tz.Object);

        private static void ClearForgotCache() => ForgotPasswordCache.CodeStorage.Clear();

        private static void PutCode(string userName, string code, DateTime expiryUtc)
        {
            ForgotPasswordCache.CodeStorage[userName] = new VerificationCodeDetails
            {
                Code = code,
                ExpiryTime = expiryUtc
            };
        }

        private void SetupClockUtc(DateTime nowUtc)
        {
            _tz.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            _tz.Setup(x => x.GetCurrentTime("UTC")).Returns(nowUtc);
        }

        private static User MakeUser(string currentPasswordPlaintext) => new()
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPasswordPlaintext)
        };

        [Fact]
        public async Task Valid_payload_passes()
        {
            ClearForgotCache();
            var cmd = ValidCommand();

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync(MakeUser("Old#Pass1"));

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(false);

            var now = DateTime.UtcNow;
            SetupClockUtc(now);
            PutCode(cmd.UserName, cmd.VerificationCode!, now.AddMinutes(10));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();

            // cleanup
            ClearForgotCache();
        }

        [Fact]
        public async Task NotEmpty_rules_trigger_when_fields_missing()
        {
            ClearForgotCache();

            _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null!);
            _changePassword.Setup(c => c.ValidatePasswordbyUserName(It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(false);

            var now = DateTime.UtcNow;
            SetupClockUtc(now);

            var cmd = new ResetUserPasswordCommand { UserName = "", Password = "" };

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
            result.ShouldHaveValidationErrorFor(x => x.Password);

            ClearForgotCache();
        }

        [Fact]
        public async Task NotFound_triggers_when_user_does_not_exist()
        {
            ClearForgotCache();
            var cmd = ValidCommand();

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync((User)null!);

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(false);

            var now = DateTime.UtcNow;
            SetupClockUtc(now);
            PutCode(cmd.UserName, cmd.VerificationCode!, now.AddMinutes(5));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserName);

            ClearForgotCache();
        }

        [Fact]
        public async Task ExpiredVerificationCode_triggers_when_code_is_expired()
        {
            ClearForgotCache();
            var cmd = ValidCommand();

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync(MakeUser("Old#Pass1"));

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(false);

            var now = DateTime.UtcNow;
            SetupClockUtc(now);
            PutCode(cmd.UserName, cmd.VerificationCode!, now.AddMinutes(-1));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserName);

            ClearForgotCache();
        }

        [Fact]
        public async Task InvalidVerificationCode_triggers_when_code_does_not_match()
        {
            ClearForgotCache();
            var cmd = ValidCommand();
            var now = DateTime.UtcNow;

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync(MakeUser("Old#Pass1"));

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(false);

            SetupClockUtc(now);
            PutCode(cmd.UserName, "999999", now.AddMinutes(10)); // mismatched

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.VerificationCode);

            ClearForgotCache();
        }

        [Fact]
        public async Task PasswordHistory_triggers_when_password_was_recently_used()
        {
            ClearForgotCache();
            var cmd = ValidCommand();

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync(MakeUser("Old#Pass1"));

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(true); // indicates reuse

            var now = DateTime.UtcNow;
            SetupClockUtc(now);
            PutCode(cmd.UserName, cmd.VerificationCode!, now.AddMinutes(10));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Password);

            ClearForgotCache();
        }

        [Fact]
        public async Task PasswordCompare_triggers_when_verificationCode_equals_current_password()
        {
            ClearForgotCache();

            var currentPassword = "Secr3t#Pass";
            var cmd = new ResetUserPasswordCommand
            {
                UserName = "neo",
                Password = "New#Pass2",
                VerificationCode = currentPassword // rule compares verification code to old password (by design)
            };

            _userRepo.Setup(r => r.GetByUsernameAsync(cmd.UserName))
                     .ReturnsAsync(MakeUser(currentPassword));

            _changePassword.Setup(c => c.ValidatePasswordbyUserName(cmd.UserName, cmd.Password))
                           .ReturnsAsync(false);

            var now = DateTime.UtcNow;
            SetupClockUtc(now);
            PutCode(cmd.UserName, cmd.VerificationCode!, now.AddMinutes(10));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.VerificationCode);

            ClearForgotCache();
        }
    }
}
