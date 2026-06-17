using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class MapScheduleIIILineCommandValidator : AbstractValidator<MapScheduleIIILineCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _accountGroupQueryRepository;
        private readonly IScheduleIIIQueryRepository _scheduleIIIQueryRepository;

        public MapScheduleIIILineCommandValidator(
            IAccountGroupQueryRepository accountGroupQueryRepository,
            IScheduleIIIQueryRepository scheduleIIIQueryRepository)
        {
            _accountGroupQueryRepository = accountGroupQueryRepository;
            _scheduleIIIQueryRepository = scheduleIIIQueryRepository;

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
                        RuleFor(x => x.AccountGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(MapScheduleIIILineCommand.AccountGroupId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.AccountGroupId)
                            .MustAsync(async (id, ct) => !await _accountGroupQueryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Group {rule.Error}")
                            .When(x => x.AccountGroupId > 0);
                        break;

                    default:
                        break;
                }
            }

            // The mapped Schedule III line must exist (only checked when a line is supplied — null = unmap).
            RuleFor(x => x.ScheduleIIILineItemId)
                .MustAsync(async (lineId, ct) => !await _scheduleIIIQueryRepository.LineItemNotFoundAsync(lineId!.Value))
                .WithMessage("Schedule III line item not found.")
                .When(x => x.ScheduleIIILineItemId.HasValue);
        }
    }
}
