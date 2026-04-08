
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ItemPriceMaster
{
    public class CreateItemPriceMasterCommandValidator : AbstractValidator<CreateItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemPriceMasterQueryRepository _queryRepository;

        public CreateItemPriceMasterCommandValidator(
            IItemPriceMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // VariantId FK exists (when provided)
                        RuleFor(x => x.VariantId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.VariantExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.VariantId)} {rule.Error}")
                            .When(x => x.VariantId.HasValue);

                        // VariantId must belong to the selected ItemId
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                await _queryRepository.VariantBelongsToItemAsync(cmd.VariantId!.Value, cmd.ItemId, ct))
                            .WithMessage("Selected Variant does not belong to the selected Item.")
                            .When(x => x.VariantId.HasValue && x.ItemId > 0);

                        // SalesSegmentId FK exists
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.CurrencyId)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TolerancePercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.TolerancePercentage)} {rule.Error}")
                            .When(x => x.TolerancePercentage.HasValue);

                        RuleFor(x => x.CharityValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.CharityValue)} {rule.Error}")
                            .When(x => x.CharityValue.HasValue);

                        RuleFor(x => x.HandlingCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateItemPriceMasterCommand.HandlingCharges)} {rule.Error}")
                            .When(x => x.HandlingCharges.HasValue);
                        break;

                    case "AlreadyExists":
                        // Overlap check (same Item + Variant + Segment with overlapping dates)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.ItemId,
                                    cmd.VariantId,
                                    cmd.SalesSegmentId,
                                    cmd.ValidFrom,
                                    cmd.ValidTo))
                            .WithMessage($"An active price record {rule.Error}")
                            .When(x => x.ItemId > 0 && x.SalesSegmentId > 0
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
