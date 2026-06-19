using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.VoucherType
{
    public class ResetVoucherTypeSeriesCommandValidator : AbstractValidator<ResetVoucherTypeSeriesCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IFinancialYearLookup _financialYearLookup;

        public ResetVoucherTypeSeriesCommandValidator(
            IVoucherTypeMasterQueryRepository queryRepository,
            IFinancialYearLookup financialYearLookup)
        {
            _queryRepository = queryRepository;
            _financialYearLookup = financialYearLookup;

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
                        RuleFor(x => x.VoucherTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(ResetVoucherTypeSeriesCommand.VoucherTypeId)} {rule.Error}");

                        RuleFor(x => x.FinancialYearId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(ResetVoucherTypeSeriesCommand.FinancialYearId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.VoucherTypeId)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Voucher Type {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.FinancialYearId)
                            .MustAsync(async (financialYearId, ct) =>
                            {
                                var year = await _financialYearLookup.GetByIdAsync(financialYearId, ct);
                                return year != null;
                            })
                            .WithMessage($"{nameof(ResetVoucherTypeSeriesCommand.FinancialYearId)} {rule.Error}")
                            .When(x => x.FinancialYearId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
