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

        [Fact]
        public async Task Email_Null_ThrowsDueToNullCacheKey()
        {
            var cmd = ValidCommand();
            cmd.Email = null;
            // Dictionary.ContainsKey(null) throws ArgumentNullException inside the validator
            Func<Task> act = async () => await CreateValidator().TestValidateAsync(cmd);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Email_Empty_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Email = "";
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
