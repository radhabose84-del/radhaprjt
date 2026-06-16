using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateSubTotalCommandValidator : AbstractValidator<CreateSubTotalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public CreateSubTotalCommandValidator(
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
                            .NotNull().WithMessage($"{nameof(CreateSubTotalCommand.SubTotalName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateSubTotalCommand.SubTotalName)} {rule.Error}");

                        RuleFor(x => x.StructureId)
                            .NotEmpty().WithMessage($"{nameof(CreateSubTotalCommand.StructureId)} {rule.Error}");

                        RuleFor(x => x.Formulas)
                            .Must(f => f != null && f.Count > 0)
                            .WithMessage("A sub-total must have at least one operand in its formula.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SubTotalName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSubTotalCommand.SubTotalName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StructureId)
                            .MustAsync(async (id, ct) => await _queryRepository.StructureExistsAsync(id))
                            .WithMessage($"{nameof(CreateSubTotalCommand.StructureId)} {rule.Error}")
                            .When(x => x.StructureId > 0);

                        RuleFor(x => x.StructureId)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsStructureLockedAsync(id))
                            .WithMessage("Structure is locked — edits must go through change control (FR-008).")
                            .When(x => x.StructureId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
