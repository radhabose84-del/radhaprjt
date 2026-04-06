using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Modules.Commands.UpdateModule;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Module;

namespace UserManagement.UnitTests.Validators.Module
{
    public sealed class UpdateModuleCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateModuleValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static UpdateModuleCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static UpdateModuleCommand ValidCommand() =>
            new UpdateModuleCommand
            {
                ModuleId = 1,
                ModuleName = "Updated Module"
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyModuleName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.ModuleName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModuleName);
        }

        [Fact]
        public async Task Validate_ModuleNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.ModuleName = new string('A', 51);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModuleName);
        }

        [Fact]
        public async Task Validate_ModuleNameWithinMaxLength_PassesValidation()
        {
            var command = ValidCommand();
            command.ModuleName = "Valid Module Name";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.ModuleName);
        }
    }
}
