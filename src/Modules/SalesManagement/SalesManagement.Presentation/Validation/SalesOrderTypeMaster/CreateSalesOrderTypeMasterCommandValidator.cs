using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using SalesManagement.Application.Common.Constants;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrderTypeMaster
{
    public class CreateSalesOrderTypeMasterCommandValidator
        : AbstractValidator<CreateSalesOrderTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public CreateSalesOrderTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderTypeMasterQueryRepository queryRepository,
            ITransactionTypeLookup transactionTypeLookup,
            ICurrencyLookup currencyLookup)
        {
            _queryRepository = queryRepository;
            _transactionTypeLookup = transactionTypeLookup;
            _currencyLookup = currencyLookup;

            var maxLengthName = maxLengthProvider
                .GetMaxLength<SalesManagement.Domain.Entities.SalesOrderTypeMaster>("TypeName") ?? 100;
            var maxLengthDescription = maxLengthProvider
                .GetMaxLength<SalesManagement.Domain.Entities.SalesOrderTypeMaster>("Description") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // ── Static / numeric / cross-field rules (executed once, outside the JSON-rule loop) ──
            ConfigureNumericRules();
            ConfigureCrossFieldRules();

            // ── JSON-rule-driven rules ──
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.SoTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.SoTypeId)} {rule.Error}");

                        RuleFor(x => x.TaxTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.TaxTypeId)} {rule.Error}");

                        RuleFor(x => x.TypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.TypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.TypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.TypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "FKColumnDelete":
                        // SoTypeId must reference a MiscMaster row under MiscType=SOTM_TYPE
                        RuleFor(x => x.SoTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.IsValidSoTypeAsync(id))
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.SoTypeId)} {rule.Error}")
                            .When(x => x.SoTypeId > 0);

                        // TaxTypeId must exist in Finance.TransactionTypeMaster
                        RuleFor(x => x.TaxTypeId)
                            .MustAsync(async (id, ct) =>
                            {
                                var rows = await _transactionTypeLookup.GetByIdsAsync(new[] { id });
                                return rows.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.TaxTypeId)} {rule.Error}")
                            .When(x => x.TaxTypeId > 0);

                        // DefaultCurrencyId (when present) must exist in Currency master
                        RuleFor(x => x.DefaultCurrencyId)
                            .MustAsync(async (id, ct) =>
                            {
                                var rows = await _currencyLookup.GetByIdsAsync(new[] { id!.Value }, ct);
                                return rows.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.DefaultCurrencyId)} {rule.Error}")
                            .When(x => x.DefaultCurrencyId.HasValue && x.DefaultCurrencyId.Value > 0);
                        break;

                    case "AlreadyExists":
                        // Composite uniqueness on (SoTypeId, TaxTypeId)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(cmd.SoTypeId, cmd.TaxTypeId))
                            .WithMessage($"Configuration for this combination of SoTypeId and TaxTypeId {rule.Error}")
                            .When(x => x.SoTypeId > 0 && x.TaxTypeId > 0);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ConfigureNumericRules()
        {
            RuleFor(x => x.MinPrice!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.MinPrice)} must be zero or positive.")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.MaxPrice)} must be zero or positive.")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(cmd => !(cmd.MinPrice.HasValue && cmd.MaxPrice.HasValue) ||
                              cmd.MaxPrice!.Value >= cmd.MinPrice!.Value)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.MaxPrice)} must be greater than or equal to {nameof(CreateSalesOrderTypeMasterCommand.MinPrice)}.")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            RuleFor(x => x.MaxQty!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.MaxQty)} must be zero or positive.")
                .When(x => x.MaxQty.HasValue);

            RuleFor(x => x.OverrideLimitPercent)
                .NotNull()
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.OverrideLimitPercent)} is required when AllowPriceOverride is true.")
                .When(x => x.AllowPriceOverride);

            RuleFor(x => x.OverrideLimitPercent!.Value)
                .InclusiveBetween(0m, 100m)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.OverrideLimitPercent)} must be between 0 and 100.")
                .When(x => x.OverrideLimitPercent.HasValue);

            RuleFor(x => x.DefaultCurrencyId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage($"{nameof(CreateSalesOrderTypeMasterCommand.DefaultCurrencyId)} is required when CurrencyRequired is true.")
                .When(x => x.CurrencyRequired);
        }

        private void ConfigureCrossFieldRules()
        {
            // Rule 15: SO_RATE_AGR → RequiresValidity must be true
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var code = await _queryRepository.GetSoTypeCodeAsync(cmd.SoTypeId);
                    if (code != MiscMasterCodes.SO_RATE_AGR) return true;
                    return cmd.RequiresValidity;
                })
                .WithMessage("Rate Agreement requires RequiresValidity = true.")
                .When(x => x.SoTypeId > 0);

            // Rule 16: SO_SAMPLE → must have (AllowZeroPrice OR Min+Max) AND MaxQty
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var code = await _queryRepository.GetSoTypeCodeAsync(cmd.SoTypeId);
                    if (code != MiscMasterCodes.SO_SAMPLE) return true;

                    var pricingOk = cmd.AllowZeroPrice || (cmd.MinPrice.HasValue && cmd.MaxPrice.HasValue);
                    var qtyOk = cmd.MaxQty.HasValue;
                    return pricingOk && qtyOk;
                })
                .WithMessage("Sample requires either AllowZeroPrice = true or both MinPrice and MaxPrice, plus MaxQty must be set.")
                .When(x => x.SoTypeId > 0);
        }
    }
}
