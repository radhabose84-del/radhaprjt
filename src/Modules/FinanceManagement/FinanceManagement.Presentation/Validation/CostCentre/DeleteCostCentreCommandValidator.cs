using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.DeleteCostCentre;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CostCentre
{
    public class DeleteCostCentreCommandValidator : AbstractValidator<DeleteCostCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCentreQueryRepository _queryRepository;

        public DeleteCostCentreCommandValidator(ICostCentreQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteCostCentreCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Cost Centre {rule.Error}");
                        break;

                    case "SoftDelete":
                        // Blocks delete when the node has children or open transactions (AC#3).
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
