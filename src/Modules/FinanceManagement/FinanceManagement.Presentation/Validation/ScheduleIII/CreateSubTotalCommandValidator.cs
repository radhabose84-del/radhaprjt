using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateSubTotalCommandValidator : AbstractValidator<CreateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateSubTotalCommandValidator()
        {
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
                        RuleFor(x => x.FormulaName)
                            .NotNull().WithMessage($"{nameof(CreateSubTotalCommand.FormulaName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateSubTotalCommand.FormulaName)} {rule.Error}");

                        RuleFor(x => x.Formulas)
                            .Must(f => f != null && f.Count > 0)
                            .WithMessage("A sub-total must have at least one operand in its formula.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
