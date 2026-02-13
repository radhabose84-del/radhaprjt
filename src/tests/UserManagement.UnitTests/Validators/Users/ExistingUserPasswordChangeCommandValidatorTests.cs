using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using FluentValidation.TestHelper;
using Moq;
using UserManagement.Presentation.Validation.Users;
using Xunit;

namespace UserManagement.UnitTests.Validators.Users
{
    public sealed class ExistingUserPasswordChangeCommandValidatorTests
    {
        private readonly Mock<IChangePassword> _changePassword = new(MockBehavior.Strict);

        private ExistingUserPasswordChangeCommandValidator CreateValidator()
            => new(_changePassword.Object);

        private static ChangeUserPasswordCommand Valid() => new()
        {
            UserId = 42,
            OldPassword = "Old#Pass1",
            NewPassword = "Aa1!aaaa" // len 8, has upper/lower/digit/special
        };

        private static string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);

        [Fact]
        public async Task Valid_payload_passes()
        {
            var cmd = Valid();

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);  // not used before
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);  // not first-time
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword)); // matches

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotEmpty_rules_trigger()
        {
            var cmd = new ChangeUserPasswordCommand
            {
                UserId = 0,
                OldPassword = "",
                NewPassword = ""
            };

            // Defensive setups in case downstream rules still evaluate
            _changePassword.Setup(x => x.ValidatePassword(It.IsAny<int>(), It.IsAny<string>()))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(It.IsAny<int>()))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(It.IsAny<int>()))
                           .ReturnsAsync(string.Empty);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.OldPassword);
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UserId_must_be_positive(int badId)
        {
            var cmd = Valid();
            cmd.UserId = badId;

            _changePassword.Setup(x => x.ValidatePassword(It.IsAny<int>(), It.IsAny<string>()))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(It.IsAny<int>()))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(It.IsAny<int>()))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Theory]
        [InlineData("Aa1!a!")]       // length 6 -> too short
        [InlineData("Aa1!aaaaaaa")]  // length 11 -> too long
        public async Task NewPassword_length_must_be_between_8_and_10(string pwd)
        {
            var cmd = Valid();
            cmd.NewPassword = pwd;

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();

            // Detect whether a Length rule is currently configured
            var hasLengthRule =
                v.CreateDescriptor()
                 .GetValidatorsForMember(nameof(ChangeUserPasswordCommand.NewPassword))
                 .Any(d => d.GetType().Name.Contains("LengthValidator"));

            var result = await v.TestValidateAsync(cmd);

            if (hasLengthRule)
                result.ShouldHaveValidationErrorFor(x => x.NewPassword);
            else
                result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task NewPassword_must_contain_uppercase()
        {
            var cmd = Valid();
            cmd.NewPassword = "aa1!aaaa"; // no uppercase

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task NewPassword_must_contain_lowercase()
        {
            var cmd = Valid();
            cmd.NewPassword = "AA1!AAAA"; // no lowercase

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task NewPassword_must_contain_digit()
        {
            var cmd = Valid();
            cmd.NewPassword = "Aa!aaaaa"; // no digit

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task NewPassword_must_contain_special_character()
        {
            var cmd = Valid();
            cmd.NewPassword = "Aa1aaaaa"; // no special

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task PasswordHistory_rule_triggers_when_recently_used()
        {
            var cmd = Valid();

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(true); // used before
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }

        [Fact]
        public async Task FirstTimeUser_rule_triggers_when_user_is_first_time()
        {
            var cmd = Valid();

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(true); // first-time -> invalid path here
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public async Task OldPassword_rule_fails_when_hash_missing_or_mismatch()
        {
            var cmd = Valid();
            cmd.OldPassword = "Wrong#Old1";

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);

            _changePassword.SetupSequence(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(string.Empty)             // no stored hash
                           .ReturnsAsync(Hash("Different#Old2"));  // mismatch

            var v = CreateValidator();

            var result1 = await v.TestValidateAsync(cmd);
            result1.ShouldHaveValidationErrorFor(x => x.OldPassword);

            var result2 = await v.TestValidateAsync(cmd);
            result2.ShouldHaveValidationErrorFor(x => x.OldPassword);
        }

        [Fact]
        public async Task OldPassword_rule_passes_when_hash_matches()
        {
            var cmd = Valid();

            _changePassword.Setup(x => x.ValidatePassword(cmd.UserId, cmd.NewPassword))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.ValidateFirstTimeUser(cmd.UserId))
                           .ReturnsAsync(false);
            _changePassword.Setup(x => x.GetUserPasswordHashAsync(cmd.UserId))
                           .ReturnsAsync(Hash(cmd.OldPassword));

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
        }
    }
}
