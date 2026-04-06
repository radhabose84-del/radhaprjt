using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompanySettings;
using UserManagement.Application.CompanySettings.Commands.CreateCompanySettings;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.CompanySettings;

namespace UserManagement.UnitTests.Validators.CompanySettings
{
    public sealed class CreateCompanySettingsCommandValidatorTests
    {
        private readonly Mock<ICompanyQuerySettings> _mockQuerySettings = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CompanySettingsDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateCompanySettingsCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQuerySettings.Object);

        private CreateCompanySettingsCommand ValidCommand() =>
            new CreateCompanySettingsCommand
            {
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
                FinancialYear = 1
            };

        private void SetupAllAsyncMocks(int companyId = 1)
        {
            _mockQuerySettings
                .Setup(r => r.AlreadyExistsAsync(companyId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.CompanyId);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroCompanyId_FailsValidation()
        {
            var command = ValidCommand();
            command.CompanyId = 0;
            SetupAllAsyncMocks(0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Validate_ZeroPasswordHistoryCount_FailsValidation()
        {
            var command = ValidCommand();
            command.PasswordHistoryCount = 0;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.PasswordHistoryCount);
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
        public async Task Validate_CompanySettingsAlreadyExist_FailsValidation()
        {
            var command = ValidCommand();
            _mockQuerySettings
                .Setup(r => r.AlreadyExistsAsync(command.CompanyId, null))
                .ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
