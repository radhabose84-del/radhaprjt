using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using FluentValidation;

namespace PartyManagement.API.Validation.BankMaster;

public class CreateBankMasterValidator : AbstractValidator<CreateBankMasterCommand>
{
    public CreateBankMasterValidator(IBankMasterQueryRepository repo)
    {
        RuleFor(x => x.Dto.BankName)
            .NotEmpty().MaximumLength(20)
            .MustAsync(async (code, ct) => !await repo.ExistsByBankCodeAsync(code, null, ct))
            .WithMessage("BankName already exists.");

        RuleFor(x => x.Dto.BankName).NotEmpty().MaximumLength(100);
    }
}
