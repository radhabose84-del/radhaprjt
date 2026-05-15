using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Cancel;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

public class CancelPurchaseOrderCommandValidator : AbstractValidator<CancelPurchaseOrderCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IPurchaseOrderQueryRepository _queryRepository;

    public CancelPurchaseOrderCommandValidator(IPurchaseOrderQueryRepository queryRepository)
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
                        .WithMessage($"{nameof(CancelPurchaseOrderCommand.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id, ct))
                        .WithMessage($"PurchaseOrder {rule.Error}");
                    break;

                case "GrnExists":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => !await _queryRepository.HasAnyGrnAsync(id, ct))
                        .WithMessage(rule.Error)
                        .When(x => x.Id > 0);
                    break;

                default:
                    break;
            }
        }
    }
}
