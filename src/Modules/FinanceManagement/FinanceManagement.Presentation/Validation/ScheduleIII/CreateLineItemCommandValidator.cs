using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateLineItemCommandValidator : AbstractValidator<CreateLineItemCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public CreateLineItemCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IScheduleIIIQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIILineItem>("LineCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIILineItem>("LineName") ?? 200;
            var maxLengthSub  = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIILineItem>("SubClassification") ?? 120;
            var maxLengthNote = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIILineItem>("NoteReference") ?? 30;

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
                        RuleFor(x => x.LineName)
                            .NotNull().WithMessage($"{nameof(CreateLineItemCommand.LineName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateLineItemCommand.LineName)} {rule.Error}");

                        RuleFor(x => x.StructureId)
                            .NotEmpty().WithMessage($"{nameof(CreateLineItemCommand.StructureId)} {rule.Error}");

                        RuleFor(x => x.SectionId)
                            .NotEmpty().WithMessage($"{nameof(CreateLineItemCommand.SectionId)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.LineCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateLineItemCommand.LineCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.LineCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.LineCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateLineItemCommand.LineCode)} {rule.Error} {maxLengthCode} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.LineCode));

                        RuleFor(x => x.LineName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateLineItemCommand.LineName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.SubClassification)
                            .MaximumLength(maxLengthSub)
                            .WithMessage($"{nameof(CreateLineItemCommand.SubClassification)} {rule.Error} {maxLengthSub} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SubClassification));

                        RuleFor(x => x.NoteReference)
                            .MaximumLength(maxLengthNote)
                            .WithMessage($"{nameof(CreateLineItemCommand.NoteReference)} {rule.Error} {maxLengthNote} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.NoteReference));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StructureId)
                            .MustAsync(async (id, ct) => await _queryRepository.StructureExistsAsync(id))
                            .WithMessage($"{nameof(CreateLineItemCommand.StructureId)} {rule.Error}")
                            .When(x => x.StructureId > 0);

                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => await _queryRepository.SectionExistsAsync(cmd.SectionId, cmd.StructureId))
                            .WithMessage($"{nameof(CreateLineItemCommand.SectionId)} {rule.Error}")
                            .When(x => x.SectionId > 0 && x.StructureId > 0);

                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) => await _queryRepository.ParentLineExistsAsync(cmd.ParentLineId!.Value, cmd.StructureId))
                            .WithMessage($"{nameof(CreateLineItemCommand.ParentLineId)} {rule.Error}")
                            .When(x => x.ParentLineId.HasValue && x.ParentLineId.Value > 0 && x.StructureId > 0);

                        // Structure must not be locked (post-lock edits go through FR-008 change control).
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
