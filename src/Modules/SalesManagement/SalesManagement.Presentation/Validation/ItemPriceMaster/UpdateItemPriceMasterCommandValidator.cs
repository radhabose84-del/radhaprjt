
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ItemPriceMaster
{
    public class UpdateItemPriceMasterCommandValidator : AbstractValidator<UpdateItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemPriceMasterQueryRepository _queryRepository;

        public UpdateItemPriceMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
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
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ItemId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // CurrencyId required
                        RuleFor(x => x.CurrencyId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.CurrencyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.CurrencyId)} {rule.Error}");

                        // ValidFrom required
                        RuleFor(x => x.ValidFrom)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ValidFrom)} {rule.Error}");

                        // ValidTo required + must be after ValidFrom
                        RuleFor(x => x.ValidTo)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ValidTo)} {rule.Error}")
                            .GreaterThan(x => x.ValidFrom)
                            .WithMessage("Valid To must be after Valid From.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Item Price Master Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Item Price Master {rule.Error}")
                            .MustAsync(async (id, ct) => await _queryRepository.IsItemPriceMasterPendingAsync(id))
                            .WithMessage("Only Item Price Master records with 'Pending' status can be updated.");
                        break;

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // VariantId FK exists (when provided)
                        RuleFor(x => x.VariantId!.Value)
                            .MustAsync(async (id, ct) => await _queryRepository.VariantExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.VariantId)} {rule.Error}")
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
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.CurrencyId)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TolerancePercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.TolerancePercentage)} {rule.Error}")
                            .When(x => x.TolerancePercentage.HasValue);

                        RuleFor(x => x.CharityValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.CharityValue)} {rule.Error}")
                            .When(x => x.CharityValue.HasValue);

                        RuleFor(x => x.HandlingCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.HandlingCharges)} {rule.Error}")
                            .When(x => x.HandlingCharges.HasValue);

                        RuleFor(x => x.AdditionalValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.AdditionalValue)} {rule.Error}")
                            .When(x => x.AdditionalValue.HasValue);
                        break;

                    case "AlreadyExists":
                        // Overlap check — exclude self (current Id)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.ItemId,
                                    cmd.VariantId,
                                    cmd.SalesSegmentId,
                                    cmd.ValidFrom,
                                    cmd.ValidTo,
                                    excludeId: cmd.Id))
                            .WithMessage($"An active price record {rule.Error}")
                            .When(x => x.Id > 0 && x.ItemId > 0 && x.SalesSegmentId > 0
                                        && x.ValidFrom != default && x.ValidTo != default
                                        && x.ValidTo > x.ValidFrom);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
