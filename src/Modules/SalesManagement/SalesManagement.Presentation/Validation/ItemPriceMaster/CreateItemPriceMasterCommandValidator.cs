
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ItemPriceMaster
{
    public class CreateItemPriceMasterCommandValidator : AbstractValidator<CreateItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemPriceMasterQueryRepository _queryRepository;

        public CreateItemPriceMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IItemPriceMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthPriceCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.ItemPriceMaster>("PriceCode") ?? 20;

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
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PriceCode)} {rule.Error}");

                        // ItemId required
                        RuleFor(x => x.ItemId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ItemId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId required
                        RuleFor(x => x.PaymentTermsId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // ExMillPrice must be greater than zero
                        RuleFor(x => x.ExMillPrice)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ExMillPrice)} must be greater than zero.");

                        // CurrencyId required
                        RuleFor(x => x.CurrencyId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.CurrencyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.CurrencyId)} {rule.Error}");

                        // ValidFrom required
                        RuleFor(x => x.ValidFrom)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ValidFrom)} {rule.Error}");

                        // ValidTo required + must be after ValidFrom
                        RuleFor(x => x.ValidTo)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ValidTo)} {rule.Error}")
                            .GreaterThan(x => x.ValidFrom)
                            .WithMessage("Valid To must be after Valid From.");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.PriceCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PriceCode)
                            .MaximumLength(maxLengthPriceCode)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PriceCode)} {rule.Error} {maxLengthPriceCode} characters.");
                        break;

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId FK exists
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId FK exists
                        RuleFor(x => x.PaymentTermsId)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermExistsAsync(id))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.CurrencyId)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        // PriceCode uniqueness
                        RuleFor(x => x.PriceCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.PriceCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceCode));

                        // Overlap check (same Item + Segment + PaymentTerms with overlapping dates)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.ItemId,
                                    cmd.SalesSegmentId,
                                    cmd.PaymentTermsId,
                                    cmd.ValidFrom,
                                    cmd.ValidTo!))
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
