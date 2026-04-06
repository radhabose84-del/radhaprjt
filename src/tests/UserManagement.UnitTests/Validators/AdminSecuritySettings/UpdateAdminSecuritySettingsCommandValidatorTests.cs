using FluentValidation.TestHelper;
using UserManagement.Application.AdminSecuritySettings.Commands.UpdateAdminSecuritySettings;
using UserManagement.Presentation.Validation.AdminSecuritySettings;

namespace UserManagement.UnitTests.Validators.AdminSecuritySettings
{
    public sealed class UpdateAdminSecuritySettingsCommandValidatorTests
    {
        private static UpdateAdminSecuritySettingsCommandValidator CreateValidator() =>
            new UpdateAdminSecuritySettingsCommandValidator();

        private static UpdateAdminSecuritySettingsCommand ValidCommand() =>
            new UpdateAdminSecuritySettingsCommand
            {
                Id = 1,
                PasswordHistoryCount = 5,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                AccountAutoUnlockMinutes = 10,
                PasswordExpiryDays = 90,
                PasswordExpiryAlertDays = 7,
                IsTwoFactorAuthenticationEnabled = 0,
                MaxConcurrentLogins = 1,
                IsForcePasswordChangeOnFirstLogin = 0,
                PasswordResetCodeExpiryMinutes = 15,
                IsCaptchaEnabledOnLogin = 0,
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NegativePasswordHistoryCount_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordHistoryCount = -1;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PasswordHistoryCount);
        }

        [Fact]
        public async Task Validate_ZeroSessionTimeoutMinutes_FailsValidation()
        {
            var command = ValidCommand();
            command.SessionTimeoutMinutes = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.SessionTimeoutMinutes);
        }

        [Fact]
        public async Task Validate_MaxFailedLoginAttemptsOutOfRange_FailsValidation()
        {
            var command = ValidCommand();
            command.MaxFailedLoginAttempts = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MaxFailedLoginAttempts);
        }

        [Fact]
        public async Task Validate_MaxFailedLoginAttemptsAboveMax_FailsValidation()
        {
            var command = ValidCommand();
            command.MaxFailedLoginAttempts = 11;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MaxFailedLoginAttempts);
        }

        [Fact]
        public async Task Validate_ZeroPasswordExpiryDays_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordExpiryDays = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PasswordExpiryDays);
        }

        [Fact]
        public async Task Validate_PasswordExpiryAlertDaysGreaterThanExpiry_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordExpiryDays = 10;
            command.PasswordExpiryAlertDays = 15;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PasswordExpiryAlertDays);
        }

        [Fact]
        public async Task Validate_InvalidTwoFactorAuth_FailsValidation()
        {
            var command = ValidCommand();
            command.IsTwoFactorAuthenticationEnabled = 2;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.IsTwoFactorAuthenticationEnabled);
        }

        [Fact]
        public async Task Validate_ZeroMaxConcurrentLogins_FailsValidation()
        {
            var command = ValidCommand();
            command.MaxConcurrentLogins = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MaxConcurrentLogins);
        }

        [Fact]
        public async Task Validate_ZeroPasswordResetCodeExpiryMinutes_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordResetCodeExpiryMinutes = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PasswordResetCodeExpiryMinutes);
        }
    }
}
