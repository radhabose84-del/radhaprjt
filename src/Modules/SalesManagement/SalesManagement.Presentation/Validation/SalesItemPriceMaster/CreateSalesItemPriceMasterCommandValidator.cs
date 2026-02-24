#nullable disable

using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesItemPriceMaster
{
    public class CreateSalesItemPriceMasterCommandValidator : AbstractValidator<CreateSalesItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;

        public CreateSalesItemPriceMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesItemPriceMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthPriceCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.SalesItemPriceMaster>("PriceCode") ?? 20;

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
                        // PriceCode required
                        RuleFor(x => x.PriceCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PriceCode)} {rule.Error}");

                        // ItemId required
                        RuleFor(x => x.ItemId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ItemId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId required
                        RuleFor(x => x.PaymentTermsId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // ExMillPrice must be greater than zero
                        RuleFor(x => x.ExMillPrice)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ExMillPrice)} must be greater than zero.");

                        // CurrencyId required
                        RuleFor(x => x.CurrencyId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}");

                        // ValidFrom required
                        RuleFor(x => x.ValidFrom)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ValidFrom)} {rule.Error}");

                        // ValidTo required + must be after ValidFrom
                        RuleFor(x => x.ValidTo)
                            .NotEqual(default(DateTimeOffset))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ValidTo)} {rule.Error}")
                            .GreaterThan(x => x.ValidFrom)
                            .WithMessage("Valid To must be after Valid From.");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.PriceCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PriceCode)
                            .MaximumLength(maxLengthPriceCode)
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PriceCode)} {rule.Error} {maxLengthPriceCode} characters.");
                        break;

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId FK exists
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId FK exists
                        RuleFor(x => x.PaymentTermsId)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermExistsAsync(id))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        // PriceCode uniqueness
                        RuleFor(x => x.PriceCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateSalesItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceCode));

                        // Overlap check (same Item + Segment + PaymentTerms with overlapping dates)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.ItemId,
                                    cmd.SalesSegmentId,
                                    cmd.PaymentTermsId,
                                    cmd.ValidFrom,
                                    cmd.ValidTo))
                            .WithMessage($"An active price record {rule.Error}")
                            .When(x => x.ItemId > 0 && x.SalesSegmentId > 0 && x.PaymentTermsId > 0
                                        && x.ValidFrom != default && x.ValidTo != default
                                        && x.ValidTo > x.ValidFrom);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
