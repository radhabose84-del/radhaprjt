using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.AccountingPeriod
{
    public class CreateAccountingPeriodCommandValidator : AbstractValidator<CreateAccountingPeriodCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountingPeriodQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateAccountingPeriodCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IAccountingPeriodQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

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
                            .NotNull().WithMessage($"{nameof(CreateAccountingPeriodCommand.PeriodName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccountingPeriodCommand.PeriodName)} {rule.Error}");

                        RuleFor(x => x.FinancialYearId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateAccountingPeriodCommand.FinancialYearId)} {rule.Error}");

                        RuleFor(x => x.PeriodNo)
                            .InclusiveBetween(1, 12)
                            .WithMessage($"{nameof(CreateAccountingPeriodCommand.PeriodNo)} must be between 1 and 12.");

                        RuleFor(x => x.StatusId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateAccountingPeriodCommand.StatusId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PeriodName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateAccountingPeriodCommand.PeriodName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EndDate)
                            .GreaterThanOrEqualTo(x => x.StartDate)
                            .WithMessage($"{nameof(CreateAccountingPeriodCommand.EndDate)} {rule.Error} {nameof(CreateAccountingPeriodCommand.StartDate)}.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StatusId)
                            .MustAsync(async (statusId, ct) => await _queryRepository.StatusExistsAsync(statusId))
                            .WithMessage($"{nameof(CreateAccountingPeriodCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PeriodNo)
                            .MustAsync(async (command, periodNo, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(
                                    _ipAddressService.GetCompanyId() ?? 0, command.FinancialYearId, periodNo))
                            .WithMessage($"{nameof(CreateAccountingPeriodCommand.PeriodNo)} {rule.Error}")
                            .When(x => x.FinancialYearId > 0 && x.PeriodNo > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
