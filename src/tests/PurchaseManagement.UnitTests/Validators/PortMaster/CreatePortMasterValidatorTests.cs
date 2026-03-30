using FluentValidation.TestHelper;
using PurchaseManagement.Presentation.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PortMaster
{
    public sealed class CreatePortMasterValidatorTests
    {
        private static CreatePortMasterValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortCode_FailsValidation(string? portCode)
        {
            var command = PortMasterBuilders.ValidCreateCommand(portCode: portCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortCode);
        }

        [Fact]
        public async Task Validate_InvalidPortCodeFormat_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(portCode: "port code!");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortName_FailsValidation(string? portName)
        {
            var command = PortMasterBuilders.ValidCreateCommand(portName: portName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortName);
        }

        [Fact]
        public async Task Validate_ZeroCountryId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(countryId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CountryId);
        }

        [Fact]
        public async Task Validate_ZeroPortTypeId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidCreateCommand(portTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortTypeId);
        }
    }
}
