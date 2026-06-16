using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class DeleteLineItemCommandValidator : AbstractValidator<DeleteLineItemCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public DeleteLineItemCommandValidator(IScheduleIIIQueryRepository queryRepository)
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
                            .NotEmpty().WithMessage($"{nameof(DeleteLineItemCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.LineItemNotFoundAsync(id))
                            .WithMessage($"Line item {rule.Error}");
                        break;

                    case "SoftDelete":
                        // AC5 — block delete/deactivate when account groups are mapped in US-GL02-03B.
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsLineMappedAsync(id))
                            .WithMessage("Cannot delete — account group(s) are mapped to this line. Re-map them first.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
