using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class UpdateLineItemCommandValidator : AbstractValidator<UpdateLineItemCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public UpdateLineItemCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IScheduleIIIQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                            .NotNull().WithMessage($"{nameof(UpdateLineItemCommand.LineName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateLineItemCommand.LineName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.LineName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateLineItemCommand.LineName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.SubClassification)
                            .MaximumLength(maxLengthSub)
                            .WithMessage($"{nameof(UpdateLineItemCommand.SubClassification)} {rule.Error} {maxLengthSub} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SubClassification));

                        RuleFor(x => x.NoteReference)
                            .MaximumLength(maxLengthNote)
                            .WithMessage($"{nameof(UpdateLineItemCommand.NoteReference)} {rule.Error} {maxLengthNote} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.NoteReference));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.LineItemNotFoundAsync(id))
                            .WithMessage($"Line item {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateLineItemCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
