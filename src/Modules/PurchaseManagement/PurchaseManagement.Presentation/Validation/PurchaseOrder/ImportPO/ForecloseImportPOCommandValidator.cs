using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Foreclose;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ImportPO;

public class ForecloseImportPOCommandValidator : AbstractValidator<ForecloseImportPOCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IImportPOQueryRepository _queryRepository;

    public ForecloseImportPOCommandValidator(IImportPOQueryRepository queryRepository)
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
                        .WithMessage($"{nameof(ForecloseImportPOCommand.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _queryRepository.ExistsAsync(id, ct))
                        .WithMessage($"Import PO {rule.Error}");
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
