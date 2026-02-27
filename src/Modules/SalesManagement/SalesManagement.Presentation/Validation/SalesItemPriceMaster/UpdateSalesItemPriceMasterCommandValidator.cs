
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesItemPriceMaster
{
    public class UpdateSalesItemPriceMasterCommandValidator : AbstractValidator<UpdateSalesItemPriceMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;

        public UpdateSalesItemPriceMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesItemPriceMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ItemId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId required
                        RuleFor(x => x.SalesSegmentId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId required
                        RuleFor(x => x.PaymentTermsId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // ExMillPrice must be greater than zero
                        RuleFor(x => x.ExMillPrice)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ExMillPrice)} must be greater than zero.");

                        // CurrencyId required
                        RuleFor(x => x.CurrencyId)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}");

                        // ValidFrom required
                        RuleFor(x => x.ValidFrom)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ValidFrom)} {rule.Error}");

                        // ValidTo required + must be after ValidFrom
                        RuleFor(x => x.ValidTo)
                            .NotEqual(default(DateOnly))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ValidTo)} {rule.Error}")
                            .GreaterThan(x => x.ValidFrom)
                            .WithMessage("Valid To must be after Valid From.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Sales Item Price Master Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Sales Item Price Master {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        // ItemId FK exists
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) => await _queryRepository.ItemExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.ItemId)} {rule.Error}");

                        // SalesSegmentId FK exists
                        RuleFor(x => x.SalesSegmentId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesSegmentExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.SalesSegmentId)} {rule.Error}");

                        // PaymentTermsId FK exists
                        RuleFor(x => x.PaymentTermsId)
                            .MustAsync(async (id, ct) => await _queryRepository.PaymentTermExistsAsync(id))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.PaymentTermsId)} {rule.Error}");

                        // CurrencyId FK exists
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepository.CurrencyExistsAsync(id, ct))
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.CurrencyId)} {rule.Error}");
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
                            .WithMessage($"{nameof(UpdateSalesItemPriceMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
