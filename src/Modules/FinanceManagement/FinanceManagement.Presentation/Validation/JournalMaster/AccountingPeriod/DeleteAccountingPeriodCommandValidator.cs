using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod
{
    public class DeleteAccountingPeriodCommandValidator : AbstractValidator<DeleteAccountingPeriodCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountingPeriodQueryRepository _queryRepository;

        public DeleteAccountingPeriodCommandValidator(IAccountingPeriodQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteAccountingPeriodCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Accounting Period {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
