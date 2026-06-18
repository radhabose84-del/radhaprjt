using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class UpdateSubTotalCommandValidator : AbstractValidator<UpdateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public UpdateSubTotalCommandValidator(IScheduleIIIQueryRepository queryRepository)
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
                            .NotNull().WithMessage($"{nameof(UpdateSubTotalCommand.FormulaName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSubTotalCommand.FormulaName)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.SubTotalNotFoundAsync(id))
                            .WithMessage($"Sub-total {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
