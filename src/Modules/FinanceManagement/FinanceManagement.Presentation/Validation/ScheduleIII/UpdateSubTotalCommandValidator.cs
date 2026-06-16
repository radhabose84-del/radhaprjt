using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class UpdateSubTotalCommandValidator : AbstractValidator<UpdateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public UpdateSubTotalCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IScheduleIIIQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIISubTotal>("SubTotalName") ?? 120;

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
                        RuleFor(x => x.SubTotalName)
                            .NotNull().WithMessage($"{nameof(UpdateSubTotalCommand.SubTotalName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSubTotalCommand.SubTotalName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SubTotalName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSubTotalCommand.SubTotalName)} {rule.Error} {maxLengthName} characters.");
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

            // A sub-total cannot reference itself in its formula (basic cycle guard).
            // NOTE: transitive cycle detection across the whole sub-total graph is deferred.
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var subTotalOperandTypeId = await _queryRepository.GetSubTotalOperandTypeIdAsync();
                    return !cmd.Formulas.Any(f => f.OperandTypeId == subTotalOperandTypeId && f.OperandRefId == cmd.Id);
                })
                .WithMessage("A sub-total cannot reference itself in its formula.");
        }
    }
}
