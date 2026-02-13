using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using FluentValidation;

namespace PartyManagement.Presentation.Validation.BankAccount
{
    public sealed class DeleteBankAccountCommandValidator : AbstractValidator<DeleteBankAccountCommand>
    {
        public DeleteBankAccountCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);         
        }
    }
}