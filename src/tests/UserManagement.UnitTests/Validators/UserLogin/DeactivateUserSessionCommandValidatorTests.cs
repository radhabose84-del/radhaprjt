using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.UserLogin.Commands.DeactivateUserSession;
using UserManagement.Presentation.Validation.UserLogin;

namespace UserManagement.UnitTests.Validators.UserLogin
{
    public sealed class DeactivateUserSessionCommandValidatorTests
    {
        private readonly Mock<IUserQueryRepository> _mockUserRepo = new(MockBehavior.Strict);

        private DeactivateUserSessionCommandValidator CreateValidator() =>
            new(_mockUserRepo.Object);

        private static DeactivateUserSessionCommand ValidCommand() =>
            new DeactivateUserSessionCommand { Username = "testuser", Password = "Password123" };

        [Fact]
        public async Task ValidCommand_PassesBasicValidation()
        {
            // PasswordValidation case uses BCrypt internally so we skip it
            // but NotEmpty rules run unconditionally
            var cmd = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Username_Empty_FailsValidation(string? username)
        {
            var cmd = ValidCommand();
            cmd.Username = username;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Password_Empty_FailsValidation(string? password)
        {
            var cmd = ValidCommand();
            cmd.Password = password;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
