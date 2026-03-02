
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

                        // PaymentTermsId required
                        RuleFor(x => x.PaymentTermsId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // ExMillRate must be greater than zero
                        RuleFor(x => x.ExMillRate)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ExMillRate)} must be greater than zero.");

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
                            .WithMessage($"Item Price Master {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId FK exists
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId FK exists
                        RuleFor(x => x.PaymentTermsId)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermExistsAsync(id))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateItemPriceMasterCommand.CurrencyId)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        // Overlap check — exclude self (current Id)
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.OverlapExistsAsync(
                                    cmd.ItemId,
                                    cmd.SalesSegmentId,
                                    cmd.PaymentTermsId,
                                    cmd.ValidFrom,
                                    cmd.ValidTo,
                                    excludeId: cmd.Id))
                            .WithMessage($"An active price record {rule.Error}")
                            .When(x => x.Id > 0 && x.ItemId > 0 && x.SalesSegmentId > 0 && x.PaymentTermsId > 0
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
