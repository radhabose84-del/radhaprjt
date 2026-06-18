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

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>("LineCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>("LineName") ?? 200;
            var maxLengthNote = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIISectionItem>("NoteReference") ?? 30;

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

                        RuleFor(x => x.NoteReference)
                            .MaximumLength(maxLengthNote)
                            .WithMessage($"{nameof(CreateLineItemCommand.NoteReference)} {rule.Error} {maxLengthNote} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.NoteReference));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SectionId)
                            .MustAsync(async (id, ct) => await _queryRepository.SectionExistsAsync(id))
                            .WithMessage($"{nameof(CreateLineItemCommand.SectionId)} {rule.Error}")
                            .When(x => x.SectionId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
