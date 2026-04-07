using PartyManagement.Application.BankMaster.Command.Delete;
using PartyManagement.Application.Common.Interfaces.IBankMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace PartyManagement.Presentation.Validation.BankMaster;

public class DeleteBankMasterValidator : AbstractValidator<DeleteBankMasterCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IBankMasterQueryRepository _bankMasterQueryRepository;

    public DeleteBankMasterValidator(IBankMasterQueryRepository bankMasterQueryRepository)
    {
        _bankMasterQueryRepository = bankMasterQueryRepository;
        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || _validationRules.Count == 0)
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        foreach (var rule in _validationRules)
        {
            switch (rule.Rule)
            {
                case "NotEmpty":
                    RuleFor(x => x.Id)
                        .NotEmpty()
                        .WithMessage($"{nameof(DeleteBankMasterCommand.Id)} {rule.Error}");
                    break;
                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _bankMasterQueryRepository.NotFoundAsync(id))
                        .WithMessage($"BankMaster {rule.Error}");
                    break;
                case "SoftDelete":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _bankMasterQueryRepository.SoftDeleteValidationAsync(id))
                        .WithMessage("This master is linked with other records. You cannot delete this record.");
                    break;
                default:
                    break;
            }
        }
    }
}
