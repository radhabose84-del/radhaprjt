using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Foreclose;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ContractPO;

public class ForecloseContractPOCommandValidator : AbstractValidator<ForecloseContractPOCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IContractPOQueryRepository _queryRepository;

    public ForecloseContractPOCommandValidator(IContractPOQueryRepository queryRepository)
    {
        _queryRepository = queryRepository;
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
                        .WithMessage($"{nameof(ForecloseContractPOCommand.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id, ct))
                        .WithMessage($"Contract PO {rule.Error}");
                    break;

                case "GrnRequired":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _queryRepository.HasAnyGrnAsync(id, ct))
                        .WithMessage(rule.Error)
                        .When(x => x.Id > 0);
                    break;

                default:
                    break;
            }
        }
    }
}
