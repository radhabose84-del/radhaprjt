using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod
{
    public class UpdateAccountingPeriodCommandValidator : AbstractValidator<UpdateAccountingPeriodCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountingPeriodQueryRepository _queryRepository;

        public UpdateAccountingPeriodCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountingPeriodQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.AccountingPeriod>("PeriodName") ?? 20;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.PeriodName)
                            .NotNull().WithMessage($"{nameof(UpdateAccountingPeriodCommand.PeriodName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateAccountingPeriodCommand.PeriodName)} {rule.Error}");

                        RuleFor(x => x.StatusId)
                            .GreaterThan(0).WithMessage($"{nameof(UpdateAccountingPeriodCommand.StatusId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PeriodName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateAccountingPeriodCommand.PeriodName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Accounting Period {rule.Error}");
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EndDate)
                            .GreaterThanOrEqualTo(x => x.StartDate)
                            .WithMessage($"{nameof(UpdateAccountingPeriodCommand.EndDate)} {rule.Error} {nameof(UpdateAccountingPeriodCommand.StartDate)}.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StatusId)
                            .MustAsync(async (statusId, ct) => await _queryRepository.StatusExistsAsync(statusId))
                            .WithMessage($"{nameof(UpdateAccountingPeriodCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAccountingPeriodCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
