using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;
using UserManagement.Presentation.Validation.Admin;

namespace UserManagement.UnitTests.Validators.Admin
{
    public sealed class SetAdminPasswordCommandValidatorTests
    {
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private SetAdminPasswordCommandValidator CreateValidator() =>
            new(_mockTimeZone.Object);

        private static ResetPasswordCommand ValidCommand() =>
            new ResetPasswordCommand
            {
                Email = "admin@test.com",
                VerificationCode = "123456",
                Password = "NewPassword123"
            };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Email_Empty_FailsValidation(string? email)
        {
            var cmd = ValidCommand();
            cmd.Email = email;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task VerificationCode_Empty_FailsValidation(string? code)
        {
            var cmd = ValidCommand();
            cmd.VerificationCode = code;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VerificationCode);
        }
    }
}
