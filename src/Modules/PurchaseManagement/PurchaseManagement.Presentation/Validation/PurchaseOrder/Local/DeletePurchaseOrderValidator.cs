using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

public class DeletePurchaseOrderValidator : AbstractValidator<DeletePurchaseOrderCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IPurchaseOrderQueryRepository _queryRepository;

    public DeletePurchaseOrderValidator(IPurchaseOrderQueryRepository queryRepository)
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
                        .WithMessage($"{nameof(DeletePurchaseOrderCommand.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _queryRepository.ExistsAsync(id, ct))
                        .WithMessage($"PurchaseOrder {rule.Error}");
                    break;

                case "SoftDelete":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) =>
                            !await _queryRepository.SoftDeleteValidationAsync(id))
                        .WithMessage(
                            "This master is linked with other records. You cannot delete this record.");
                    break;

                default:
                    break;
            }
        }
    }
}
