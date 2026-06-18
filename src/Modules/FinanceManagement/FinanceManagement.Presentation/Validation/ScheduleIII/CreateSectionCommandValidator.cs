using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSection;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IScheduleIIIQueryRepository _queryRepository;

        public CreateSectionCommandValidator(
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
                            .NotNull().WithMessage($"{nameof(CreateSectionCommand.SectionName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateSectionCommand.SectionName)} {rule.Error}");

                        RuleFor(x => x.StatementTypeId)
                            .NotEmpty().WithMessage($"{nameof(CreateSectionCommand.StatementTypeId)} {rule.Error}");

                        RuleFor(x => x.NatureId)
                            .NotEmpty().WithMessage($"{nameof(CreateSectionCommand.NatureId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SectionName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSectionCommand.SectionName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.SectionName)
                            .MustAsync(async (name, ct) => !await _queryRepository.SectionNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateSectionCommand.SectionName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.SectionName));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
