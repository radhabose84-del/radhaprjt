using FluentValidation.TestHelper;
using PurchaseManagement.Presentation.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PortMaster
{
    public sealed class UpdatePortMasterValidatorTests
    {
        private static UpdatePortMasterValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = PortMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortCode_FailsValidation(string? portCode)
        {
            var command = PortMasterBuilders.ValidUpdateCommand(portCode: portCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPortName_FailsValidation(string? portName)
        {
            var command = PortMasterBuilders.ValidUpdateCommand(portName: portName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PortName);
        }

        [Fact]
        public async Task Validate_ZeroCountryId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidUpdateCommand(countryId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CountryId);
        }
    }
}
