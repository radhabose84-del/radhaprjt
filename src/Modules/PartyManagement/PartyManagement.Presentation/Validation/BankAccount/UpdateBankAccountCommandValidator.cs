using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using FluentValidation;

namespace PartyManagement.Presentation.Validation.BankAccount;
public class UpdateBankAccountCommandValidator : AbstractValidator<UpdateBankAccountCommand>
{
    public UpdateBankAccountCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AccountHolderName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.IFSCCode)
        .Matches("^[A-Z]{4}0[0-9A-Z]{6}$").When(x => !string.IsNullOrWhiteSpace(x.IFSCCode));
        RuleFor(x => x.SWIFTCode)
        .Matches("^[A-Z0-9]{8}([A-Z0-9]{3})?$").When(x => !string.IsNullOrWhiteSpace(x.SWIFTCode));
        RuleFor(x => x.IBan)
        .Matches("^[A-Z0-9]{15,34}$").When(x => !string.IsNullOrWhiteSpace(x.IBan));
        RuleFor(x => x.AccountTypeId).GreaterThan(0);
    }
}