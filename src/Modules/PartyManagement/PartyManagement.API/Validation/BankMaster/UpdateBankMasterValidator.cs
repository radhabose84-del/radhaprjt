using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using FluentValidation;

namespace PartyManagement.API.Validation.BankMaster;

public class UpdateBankMasterValidator : AbstractValidator<UpdateBankMasterCommand>
{
    public UpdateBankMasterValidator(IBankMasterQueryRepository repo)
    {
        RuleFor(x => x.Dto.Id).GreaterThan(0);
        RuleFor(x => x.Dto.BankName)
            .NotEmpty().MaximumLength(20)
            .MustAsync(async (cmd, code, ct) => !await repo.ExistsByBankCodeAsync(code, cmd.Dto.Id, ct))
            .WithMessage("BankName already exists for another record.");
        RuleFor(x => x.Dto.BankName).NotEmpty().MaximumLength(100);
    }
}
