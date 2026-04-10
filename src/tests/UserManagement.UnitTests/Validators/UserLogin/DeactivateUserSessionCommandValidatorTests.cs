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

        private void SetupGetByUsername()
        {
            _mockUserRepo
                .Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync((UserManagement.Domain.Entities.User?)null!);
        }

        [Fact]
        public async Task ValidCommand_PassesBasicValidation()
        {
            // PasswordValidation case uses BCrypt — return a user with matching hash
            var hash = BCrypt.Net.BCrypt.HashPassword("Password123");
            _mockUserRepo
                .Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserManagement.Domain.Entities.User { PasswordHash = hash });
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
            SetupGetByUsername();
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
            SetupGetByUsername();
            var cmd = ValidCommand();
            cmd.Password = password;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
