using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.AdminSecuritySettings.Commands.CreateAdminSecuritySettings;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.AdminSecuritySettings;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.UnitTests.Validators.AdminSecuritySettings
{
    public sealed class CreateAdminSecuritySettingsCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"AdminSecuritySettingsDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static CreateAdminSecuritySettingsCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static CreateAdminSecuritySettingsCommand ValidCommand() =>
            new CreateAdminSecuritySettingsCommand
            {
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
                IsCaptchaEnabledOnLogin = 0
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroPasswordHistoryCount_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordHistoryCount = 0;
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
        public async Task Validate_NonZeroPasswordHistoryCount_PassesValidation()
        {
            var command = ValidCommand();
            command.PasswordHistoryCount = 3;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.PasswordHistoryCount);
        }

        [Fact]
        public async Task Validate_NonZeroSessionTimeoutMinutes_PassesValidation()
        {
            var command = ValidCommand();
            command.SessionTimeoutMinutes = 60;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.SessionTimeoutMinutes);
        }
    }
}
