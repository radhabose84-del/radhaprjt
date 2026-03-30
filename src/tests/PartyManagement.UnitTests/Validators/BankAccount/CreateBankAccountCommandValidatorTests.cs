using FluentValidation.TestHelper;
using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Presentation.Validation.BankAccount;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Validators.BankAccount
{
    public sealed class CreateBankAccountCommandValidatorTests
    {
        private CreateBankAccountCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BankAccountBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroBankId_FailsValidation()
        {
            var command = BankAccountBuilders.ValidCreateCommand(bankId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BankId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyAccountNumber_FailsValidation(string? accountNumber)
        {
            var command = new CreateBankAccountCommand(
                BankId: 1,
                AccountNumber: accountNumber!,
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: null,
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
        }

        [Fact]
        public async Task Validate_ZeroAccountTypeId_FailsValidation()
        {
            var command = new CreateBankAccountCommand(
                BankId: 1,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: null,
                SWIFTCode: null,
                AccountTypeId: 0,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountTypeId);
        }

        [Fact]
        public async Task Validate_InvalidIFSCFormat_FailsValidation()
        {
            var command = new CreateBankAccountCommand(
                BankId: 1,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: "INVALID",
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: true,
                IBan: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IFSCCode);
        }

        [Fact]
        public async Task Validate_NullIFSCCode_PassesIFSCValidation()
        {
            var command = new CreateBankAccountCommand(
                BankId: 1,
                AccountNumber: "1234567890",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: null,
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: false,
                IsPrimaryAccount: false,
                IBan: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IFSCCode);
        }
    }
}
