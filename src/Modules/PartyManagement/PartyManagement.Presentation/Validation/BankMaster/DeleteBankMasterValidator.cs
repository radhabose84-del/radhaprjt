using PartyManagement.Application.BankMaster.Command.Delete;
using FluentValidation;

namespace PartyManagement.Presentation.Validation.BankMaster;

public class DeleteBankMasterValidator : AbstractValidator<DeleteBankMasterCommand>
{
    public DeleteBankMasterValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
