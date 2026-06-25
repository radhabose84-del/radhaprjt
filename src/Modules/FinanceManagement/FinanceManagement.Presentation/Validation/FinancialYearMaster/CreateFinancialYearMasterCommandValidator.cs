using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.FinancialYearMaster
{
    public class CreateFinancialYearMasterCommandValidator : AbstractValidator<CreateFinancialYearMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateFinancialYearMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.FinancialYearMaster>("FinancialYearCode") ?? 9;

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
                        RuleFor(x => x.FinancialYearCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.FinancialYearCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error} {maxLengthCode} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.FinancialYearCode)
                            .MustAsync(async (code, ct) =>
                            {
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByCodeAsync(code!, companyId);
                            })
                            .WithMessage($"{nameof(CreateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.FinancialYearCode));
                        break;

                    default:
                        break;
                }
            }

            // Strict Indian financial-year code format: YYYY-YY (e.g., "2024-25")
            RuleFor(x => x.FinancialYearCode)
                .Matches(@"^\d{4}-\d{2}$")
                .WithMessage("Financial Year Code must follow format YYYY-YY (e.g. 2024-25).")
                .When(x => !string.IsNullOrWhiteSpace(x.FinancialYearCode));

            RuleFor(x => x.StartDate)
                .NotEqual(default(DateOnly))
                .WithMessage("Start Date is required.")
                .Must(d => d.Day == 1)
                .WithMessage("Start Date must be the 1st of a month.")
                .When(x => x.StartDate != default);

            RuleFor(x => x.EndDate)
                .NotEqual(default(DateOnly))
                .WithMessage("End Date is required.");

            // FY length: strict 12 months unless IsTransitionYear flag is set
            RuleFor(x => x)
                .Must(cmd => cmd.EndDate == cmd.StartDate.AddMonths(12).AddDays(-1))
                .WithMessage("End Date must be exactly 12 months after Start Date (use IsTransitionYear for first-year / FY-change exceptions).")
                .When(x => !x.IsTransitionYear && x.StartDate != default && x.EndDate != default);

            RuleFor(x => x)
                .Must(cmd => cmd.EndDate > cmd.StartDate)
                .WithMessage("End Date must be after Start Date.")
                .When(x => x.IsTransitionYear && x.StartDate != default && x.EndDate != default);

            // Code suffix must equal (StartDate.Year + 1) % 100 — e.g. StartDate 2024-04-01 → code "2024-25"
            RuleFor(x => x)
                .Must(cmd =>
                {
                    if (string.IsNullOrWhiteSpace(cmd.FinancialYearCode) || cmd.StartDate == default) return true;
                    if (!int.TryParse(cmd.FinancialYearCode.Substring(0, 4), out var year)) return true;
                    if (!int.TryParse(cmd.FinancialYearCode.Substring(5, 2), out var suffix)) return true;
                    return year == cmd.StartDate.Year && suffix == (cmd.StartDate.Year + 1) % 100;
                })
                .WithMessage("Financial Year Code must match Start Date year (e.g. StartDate 2024-04-01 → code '2024-25').")
                .When(x => !string.IsNullOrWhiteSpace(x.FinancialYearCode) && x.StartDate != default);

            // Date-range overlap check against existing Financial Years
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var companyId = _ipAddressService.GetCompanyId() ?? 0;
                    return !await _queryRepository.OverlapsExistingRangeAsync(cmd.StartDate, cmd.EndDate, companyId);
                })
                .WithMessage("Financial Year date range overlaps an existing fiscal year.")
                .When(x => x.StartDate != default && x.EndDate != default);
        }
    }
}
