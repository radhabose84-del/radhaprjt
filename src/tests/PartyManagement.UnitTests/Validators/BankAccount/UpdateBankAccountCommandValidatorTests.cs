using FluentValidation.TestHelper;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Presentation.Validation.BankAccount;
using PartyManagement.UnitTests.TestData;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.Validators.BankAccount
{
    public sealed class UpdateBankAccountCommandValidatorTests
    {
        private UpdateBankAccountCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BankAccountBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = BankAccountBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroBankId_FailsValidation()
        {
            var command = new UpdateBankAccountCommand(
                Id: 1,
                BankId: 0,
                AccountNumber: "9876543210",
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: null,
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: false,
                IBan: null,
                IsActive: Status.Active);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BankId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyAccountNumber_FailsValidation(string? accountNumber)
        {
            var command = new UpdateBankAccountCommand(
                Id: 1,
                BankId: 1,
                AccountNumber: accountNumber!,
                AccountHolderName: "Test Holder",
                BranchId: 1,
                IFSCCode: null,
                SWIFTCode: null,
                AccountTypeId: 1,
                IsDefaultAccount: true,
                IsPrimaryAccount: false,
                IBan: null,
                IsActive: Status.Active);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
        }
    }
}
