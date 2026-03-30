using FluentValidation.TestHelper;
using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Presentation.Validation.BankMaster;

namespace PartyManagement.UnitTests.Validators.BankMaster
{
    public sealed class DeleteBankMasterValidatorTests
    {
        private DeleteBankMasterValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteBankMasterCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteBankMasterCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = new DeleteBankMasterCommand(-1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
