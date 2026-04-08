using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.ForgotUserPassword;
using UserManagement.Presentation.Validation.Users;

namespace UserManagement.UnitTests.Validators.Users
{
    public sealed class ForgotPasswordCommandValidatorTests
    {
        private readonly Mock<IUserQueryRepository> _mockUserRepo = new(MockBehavior.Strict);

        private ForgotPasswordCommandValidator CreateValidator() =>
            new(_mockUserRepo.Object);

        private static ForgotUserPasswordCommand ValidCommand() =>
            new ForgotUserPasswordCommand { UserName = "testuser" };

        private void SetupHappyPath(string username = "testuser")
        {
            _mockUserRepo.Setup(r => r.ValidateUsernameAsync(username, null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.ValidateUserActiveAsync(username, null)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UserName_Empty_FailsValidation(string? username)
        {
            _mockUserRepo.Setup(r => r.ValidateUsernameAsync(It.IsAny<string?>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockUserRepo.Setup(r => r.ValidateUserActiveAsync(It.IsAny<string?>(), It.IsAny<int?>())).ReturnsAsync(false);
            var cmd = ValidCommand();
            cmd.UserName = username;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public async Task UserNotFound_FailsValidation()
        {
            _mockUserRepo.Setup(r => r.ValidateUsernameAsync("testuser", null)).ReturnsAsync(false);
            _mockUserRepo.Setup(r => r.ValidateUserActiveAsync("testuser", null)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task UserInactive_FailsValidation()
        {
            _mockUserRepo.Setup(r => r.ValidateUsernameAsync("testuser", null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.ValidateUserActiveAsync("testuser", null)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
