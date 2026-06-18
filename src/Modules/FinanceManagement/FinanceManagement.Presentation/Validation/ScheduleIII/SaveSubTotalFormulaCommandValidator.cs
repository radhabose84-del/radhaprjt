using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class SaveSubTotalFormulaCommandValidator : AbstractValidator<SaveSubTotalFormulaCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public SaveSubTotalFormulaCommandValidator(IScheduleIIIQueryRepository queryRepository)
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
                    case "NotFound":
                        RuleFor(x => x.SubTotalId)
                            .GreaterThan(0).WithMessage("Valid SubTotalId is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.SubTotalNotFoundAsync(id))
                            .WithMessage($"Sub-total {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.Formulas)
                .Must(f => f != null && f.Count > 0)
                .WithMessage("At least one operand is required in the formula.");
        }
    }
}
