using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.CompanySettings.Commands.UpdateCompanySettings;
using UserManagement.Presentation.Validation.CompanySettings;

namespace UserManagement.UnitTests.Validators.CompanySettings
{
    public sealed class UpdateCompanySettingsCommandValidatorTests
    {
        private readonly Mock<ICompanyQuerySettings> _mockQuerySettings = new(MockBehavior.Strict);

        private UpdateCompanySettingsCommandValidator CreateValidator() =>
            new(_mockQuerySettings.Object);

        private UpdateCompanySettingsCommand ValidCommand() =>
            new UpdateCompanySettingsCommand
            {
                Id = 1,
                CompanyId = 1,
                PasswordHistoryCount = 5,
                SessionTimeout = 30,
                FailedLoginAttempts = 3,
                AutoReleaseTime = 10,
                PasswordExpiryDays = 90,
                PasswordExpiryAlert = 7,
                TwoFactorAuth = 0,
                MaxConcurrentLogins = 1,
                ForgotPasswordCodeExpiry = 15,
                CaptchaOnLogin = 0,
                Currency = 1,
                Language = 1,
                TimeZone = 1,
                FinancialYear = 1,
                IsActive = 1
            };

        private void SetupAllAsyncMocks(int companyId = 1, int id = 1)
        {
            _mockQuerySettings
                .Setup(r => r.AlreadyExistsAsync(companyId, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.CompanyId, command.Id);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroCompanyId_FailsValidation()
        {
            var command = ValidCommand();
            command.CompanyId = 0;
            SetupAllAsyncMocks(0, command.Id);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Validate_ZeroSessionTimeout_FailsValidation()
        {
            var command = ValidCommand();
            command.SessionTimeout = 0;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.SessionTimeout);
        }

        [Fact]
        public async Task Validate_InvalidTwoFactorAuth_FailsValidation()
        {
            var command = ValidCommand();
            command.TwoFactorAuth = 2;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.TwoFactorAuth);
        }

        [Fact]
        public async Task Validate_InvalidCaptchaOnLogin_FailsValidation()
        {
            var command = ValidCommand();
            command.CaptchaOnLogin = 2;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CaptchaOnLogin);
        }

        [Fact]
        public async Task Validate_CompanySettingsAlreadyExistForOtherRecord_FailsValidation()
        {
            var command = ValidCommand();
            _mockQuerySettings
                .Setup(r => r.AlreadyExistsAsync(command.CompanyId, command.Id))
                .ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
