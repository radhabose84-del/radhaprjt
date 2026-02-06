using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using FluentValidation;


namespace PartyManagement.API.Validation.BankAccount;


public class CreateBankAccountCommandValidator : AbstractValidator<CreateBankAccountCommand>
{
    public CreateBankAccountCommandValidator()
    {
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.AccountNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AccountHolderName).NotEmpty().MaximumLength(250);        
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.IFSCCode)
        .Matches("^[A-Z]{4}0[0-9A-Z]{6}$").When(x => !string.IsNullOrWhiteSpace(x.IFSCCode))
        .WithMessage("Invalid IFSC format (e.g., HDFC0123456)");
        RuleFor(x => x.SWIFTCode)
        .Matches("^[A-Z0-9]{8}([A-Z0-9]{3})?$").When(x => !string.IsNullOrWhiteSpace(x.SWIFTCode))
        .WithMessage("SWIFT should be 8 or 11 characters");
        RuleFor(x => x.IBan)
        .Matches("^[A-Z0-9]{15,34}$").When(x => !string.IsNullOrWhiteSpace(x.IBan))
        .WithMessage("IBAN must be 15–34 alphanumeric characters");
        RuleFor(x => x.AccountTypeId).GreaterThan(0);
    }
}
