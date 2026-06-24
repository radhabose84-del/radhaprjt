using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.RecurringJournalTemplate
{
    public class DeleteRecurringJournalTemplateCommandValidator : AbstractValidator<DeleteRecurringJournalTemplateCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;

        public DeleteRecurringJournalTemplateCommandValidator(IRecurringJournalTemplateQueryRepository queryRepository)
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
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteRecurringJournalTemplateCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Recurring journal template {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
