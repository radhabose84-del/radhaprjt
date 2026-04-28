using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using SalesManagement.Application.Common.Constants;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrderTypeMaster
{
    public class UpdateSalesOrderTypeMasterCommandValidator
        : AbstractValidator<UpdateSalesOrderTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;

        public UpdateSalesOrderTypeMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesOrderTypeMasterQueryRepository queryRepository,
            ICurrencyLookup currencyLookup)
        {
            _queryRepository = queryRepository;
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

            ConfigureNumericRules();
            ConfigureCrossFieldRules();

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.TypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.TypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.TypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.TypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesOrderTypeMaster {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        // DefaultCurrencyId (when present) must exist in Currency master
                        RuleFor(x => x.DefaultCurrencyId)
                            .MustAsync(async (id, ct) =>
                            {
                                var rows = await _currencyLookup.GetByIdsAsync(new[] { id!.Value }, ct);
                                return rows.Count > 0;
                            })
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.DefaultCurrencyId)} {rule.Error}")
                            .When(x => x.DefaultCurrencyId.HasValue && x.DefaultCurrencyId.Value > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.IsActive)} {rule.Error}");
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
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.MinPrice)} must be zero or positive.")
                .When(x => x.MinPrice.HasValue);

            RuleFor(x => x.MaxPrice!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.MaxPrice)} must be zero or positive.")
                .When(x => x.MaxPrice.HasValue);

            RuleFor(x => x)
                .Must(cmd => !(cmd.MinPrice.HasValue && cmd.MaxPrice.HasValue) ||
                              cmd.MaxPrice!.Value >= cmd.MinPrice!.Value)
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.MaxPrice)} must be greater than or equal to {nameof(UpdateSalesOrderTypeMasterCommand.MinPrice)}.")
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

            RuleFor(x => x.MaxQty!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.MaxQty)} must be zero or positive.")
                .When(x => x.MaxQty.HasValue);

            RuleFor(x => x.OverrideLimitPercent)
                .NotNull()
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.OverrideLimitPercent)} is required when AllowPriceOverride is true.")
                .When(x => x.AllowPriceOverride);

            RuleFor(x => x.OverrideLimitPercent!.Value)
                .InclusiveBetween(0m, 100m)
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.OverrideLimitPercent)} must be between 0 and 100.")
                .When(x => x.OverrideLimitPercent.HasValue);

            RuleFor(x => x.DefaultCurrencyId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage($"{nameof(UpdateSalesOrderTypeMasterCommand.DefaultCurrencyId)} is required when CurrencyRequired is true.")
                .When(x => x.CurrencyRequired);
        }

        private void ConfigureCrossFieldRules()
        {
            // Resolve SoTypeId by Id (since Update payload doesn't carry SoTypeId — immutable)
            // Then enforce same cross-field rules as Create
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var soTypeId = await _queryRepository.GetSoTypeIdByRowIdAsync(cmd.Id);
                    if (soTypeId is null) return true;   // NotFound rule will catch this

                    var code = await _queryRepository.GetSoTypeCodeAsync(soTypeId.Value);
                    if (code != MiscMasterCodes.SO_RATE_AGR) return true;
                    return cmd.RequiresValidity;
                })
                .WithMessage("Rate Agreement requires RequiresValidity = true.")
                .When(x => x.Id > 0);

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var soTypeId = await _queryRepository.GetSoTypeIdByRowIdAsync(cmd.Id);
                    if (soTypeId is null) return true;

                    var code = await _queryRepository.GetSoTypeCodeAsync(soTypeId.Value);
                    if (code != MiscMasterCodes.SO_SAMPLE) return true;

                    var pricingOk = cmd.AllowZeroPrice || (cmd.MinPrice.HasValue && cmd.MaxPrice.HasValue);
                    var qtyOk = cmd.MaxQty.HasValue;
                    return pricingOk && qtyOk;
                })
                .WithMessage("Sample requires either AllowZeroPrice = true or both MinPrice and MaxPrice, plus MaxQty must be set.")
                .When(x => x.Id > 0);
        }
    }
}
