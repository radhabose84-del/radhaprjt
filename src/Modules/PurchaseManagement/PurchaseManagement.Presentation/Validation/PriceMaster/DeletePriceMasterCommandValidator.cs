using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.PriceMaster;

public class DeletePriceMasterCommandValidator : AbstractValidator<DeletePriceMasterCommand>
{
    private readonly List<ValidationRule> _validationRules;
    private readonly IPriceMasterQueryRepository _queryRepository;

    public DeletePriceMasterCommandValidator(IPriceMasterQueryRepository queryRepository)
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
                        .WithMessage($"{nameof(DeletePriceMasterCommand.Id)} {rule.Error}");
                    break;

                case "NotFound":
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) =>
                        {
                            var existing = await _queryRepository.GetByIdAsync(id, ct);
                            return existing is not null;
                        })
                        .WithMessage($"PriceMaster {rule.Error}");
                    break;

                default:
                    break;
            }
        }
    }
}
