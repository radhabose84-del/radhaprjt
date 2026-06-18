using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSection;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public UpdateSectionCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IScheduleIIIQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.ScheduleIIISection>("SectionName") ?? 150;

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
                        RuleFor(x => x.SectionName)
                            .NotNull().WithMessage($"{nameof(UpdateSectionCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateSectionCommand.SectionName)} {rule.Error}");

                        RuleFor(x => x.StatementTypeId)
                            .NotEmpty().WithMessage($"{nameof(UpdateSectionCommand.StatementTypeId)} {rule.Error}");

                        RuleFor(x => x.NatureId)
                            .NotEmpty().WithMessage($"{nameof(UpdateSectionCommand.NatureId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSectionCommand.SectionName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.SectionNotFoundAsync(id))
                            .WithMessage($"Section {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSectionCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
