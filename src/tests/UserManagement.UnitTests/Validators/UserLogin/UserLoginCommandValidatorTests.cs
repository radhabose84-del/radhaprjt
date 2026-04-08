using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.UserLogin.Commands.UserLogin;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.UserLogin;

namespace UserManagement.UnitTests.Validators.UserLogin
{
    public sealed class UserLoginCommandValidatorTests
    {
        private readonly Mock<IUserQueryRepository> _mockUserRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyQuerySettings> _mockCompanySettings = new(MockBehavior.Loose);
        private readonly Mock<IUserSessionRepository> _mockSessionRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private UserLoginCommandValidator CreateValidator()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserLoginValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            var maxProvider = new MaxLengthProvider(ctx);
            return new UserLoginCommandValidator(maxProvider, _mockUserRepo.Object,
                _mockCompanySettings.Object, _mockSessionRepo.Object, _mockIpService.Object);
        }

        private static UserLoginCommand ValidCommand() =>
            new UserLoginCommand { Username = "testuser", Password = "Password123" };

        private void SetupHappyPath(string username = "testuser")
        {
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync(username, null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.UserLockedAsync(username)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesBasicValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            // UserSession rule uses a RuleSet so it's not run in basic TestValidateAsync
            result.ShouldNotHaveValidationErrorFor(x => x.Username);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Username_Empty_FailsValidation(string? username)
        {
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockUserRepo.Setup(r => r.UserLockedAsync(It.IsAny<string>())).ReturnsAsync(false);
            var cmd = ValidCommand();
            cmd.Username = username!;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Password_Empty_FailsValidation(string? password)
        {
            var cmd = ValidCommand();
            cmd.Password = password!;
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.UserLockedAsync(It.IsAny<string>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Username_ExceedsMaxLength_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Username = new string('a', 51);
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.UserLockedAsync(It.IsAny<string>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Username);
        }

        [Fact]
        public async Task UserNotFound_FailsValidation()
        {
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync("testuser", null)).ReturnsAsync(false);
            _mockUserRepo.Setup(r => r.UserLockedAsync("testuser")).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task UserLocked_FailsValidation()
        {
            _mockUserRepo.Setup(r => r.AlreadyExistsAsync("testuser", null)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.UserLockedAsync("testuser")).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
