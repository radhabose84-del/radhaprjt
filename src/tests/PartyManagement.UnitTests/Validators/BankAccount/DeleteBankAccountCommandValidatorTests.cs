using FluentValidation.TestHelper;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Presentation.Validation.BankAccount;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.BankAccount
{
    public sealed class DeleteBankAccountCommandValidatorTests
    {
        private DeleteBankAccountCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BankAccountBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteBankAccountCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = new DeleteBankAccountCommand(-5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
