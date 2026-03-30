using FluentValidation.TestHelper;
using PurchaseManagement.Presentation.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PortMaster
{
    public sealed class DeletePortMasterValidatorTests
    {
        private static DeletePortMasterValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = PortMasterBuilders.ValidDeleteCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidDeleteCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidDeleteCommand(-1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
