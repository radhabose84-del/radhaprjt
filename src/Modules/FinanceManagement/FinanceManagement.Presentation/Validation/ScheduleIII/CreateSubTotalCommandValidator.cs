using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateSubTotalCommandValidator : AbstractValidator<CreateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public CreateSubTotalCommandValidator(IScheduleIIIQueryRepository queryRepository)
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
                        RuleFor(x => x.FormulaName)
                            .NotNull().WithMessage($"{nameof(CreateSubTotalCommand.FormulaName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateSubTotalCommand.FormulaName)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.FormulaName)
                            .MustAsync(async (name, ct) => !await _queryRepository.SubTotalNameExistsAsync(name))
                            .WithMessage($"{nameof(CreateSubTotalCommand.FormulaName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.FormulaName));

                        RuleFor(x => x.DisplayOrder)
                            .MustAsync(async (order, ct) => !await _queryRepository.SubTotalDisplayOrderExistsAsync(order))
                            .WithMessage($"{nameof(CreateSubTotalCommand.DisplayOrder)} {rule.Error}")
                            .When(x => x.DisplayOrder > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
