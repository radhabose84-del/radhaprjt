using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.Journal
{
    public class DeleteJournalCommandValidator : AbstractValidator<DeleteJournalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IJournalQueryRepository _queryRepository;

        public DeleteJournalCommandValidator(IJournalQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteJournalCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Journal voucher {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Only drafts can be deleted; posted journals are immutable (US-GL01-10).
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await _queryRepository.IsDraftAsync(id))
                .WithMessage("Only draft journals can be deleted.");
        }
    }
}
