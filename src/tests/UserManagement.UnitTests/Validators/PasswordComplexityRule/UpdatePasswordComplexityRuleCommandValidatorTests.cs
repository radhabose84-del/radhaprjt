using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.PasswordComplexityrule;

namespace UserManagement.UnitTests.Validators.PasswordComplexityRule
{
    public sealed class UpdatePasswordComplexityRuleCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"PwdComplexityUpdateDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static UpdatePasswordComplexityRuleCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static UpdatePasswordComplexityRuleCommand ValidCommand() =>
            new UpdatePasswordComplexityRuleCommand
            {
                Id = 1,
                PwdComplexityRule = "At least 8 characters"
            };

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PwdComplexityRule_Empty_FailsValidation(string? value)
        {
            var cmd = ValidCommand();
            cmd.PwdComplexityRule = value;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PwdComplexityRule);
        }

        [Fact]
        public async Task PwdComplexityRule_ExceedsMaxLength_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.PwdComplexityRule = new string('A', 160);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.PwdComplexityRule);
        }
    }
}
