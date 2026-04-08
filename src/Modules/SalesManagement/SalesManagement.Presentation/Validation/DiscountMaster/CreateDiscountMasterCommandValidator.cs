using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DiscountMaster
{
    public class CreateDiscountMasterCommandValidator : AbstractValidator<CreateDiscountMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDiscountMasterQueryRepository _queryRepo;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public CreateDiscountMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDiscountMasterQueryRepository queryRepo,
            IPaymentTermLookup paymentTermLookup,
            ICurrencyLookup currencyLookup)
        {
            _queryRepo = queryRepo;
            _paymentTermLookup = paymentTermLookup;
            _currencyLookup = currencyLookup;

            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.DiscountMaster>("DiscountName") ?? 100;

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
                        RuleFor(x => x.DiscountName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountName)} {rule.Error}");

                        RuleFor(x => x.TriggerEventId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.TriggerEventId)} {rule.Error}");

                        RuleFor(x => x.DiscountBasisId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountBasisId)} {rule.Error}");

                        RuleFor(x => x.ExecutionTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ExecutionTypeId)} {rule.Error}");

                        RuleFor(x => x.ValueTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ValueTypeId)} {rule.Error}");

                        RuleFor(x => x.SlabTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.SlabTypeId)} {rule.Error}");

                        RuleFor(x => x.Priority)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.Priority)} {rule.Error}");

                        RuleFor(x => x.Slabs)
                            .Must(s => s != null && s.Count > 0)
                            .WithMessage("At least one slab must be defined.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.DiscountName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.DiscountName)
                            .MustAsync(async (name, ct) => !await _queryRepo.AlreadyExistsAsync(name!))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.DiscountName));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.TriggerEventId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.TriggerEventId)} {rule.Error}")
                            .When(x => x.TriggerEventId > 0);

                        RuleFor(x => x.DiscountBasisId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountBasisId)} {rule.Error}")
                            .When(x => x.DiscountBasisId > 0);

                        RuleFor(x => x.ExecutionTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ExecutionTypeId)} {rule.Error}")
                            .When(x => x.ExecutionTypeId > 0);

                        RuleFor(x => x.ValueTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ValueTypeId)} {rule.Error}")
                            .When(x => x.ValueTypeId > 0);

                        RuleFor(x => x.SlabTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.SlabTypeId)} {rule.Error}")
                            .When(x => x.SlabTypeId > 0);

                        RuleFor(x => x.MaxDiscountLimitTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.MaxDiscountLimitTypeId)} {rule.Error}")
                            .When(x => x.MaxDiscountLimitTypeId.HasValue && x.MaxDiscountLimitTypeId > 0);

                        RuleFor(x => x.CustomerGroupId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.CustomerGroupId)} {rule.Error}")
                            .When(x => x.CustomerGroupId.HasValue && x.CustomerGroupId > 0);

                        RuleFor(x => x.ExclusionGroupId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ExclusionGroupId)} {rule.Error}")
                            .When(x => x.ExclusionGroupId.HasValue && x.ExclusionGroupId > 0);

                        // Cross-module: CurrencyId via ICurrencyLookup
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (currencyId, ct) =>
                            {
                                var currencies = await _currencyLookup.GetByIdsAsync(new[] { currencyId!.Value }, ct);
                                return currencies.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);

                        // Validate each SalesGroupId exists
                        RuleForEach(x => x.SalesGroupIds)
                            .MustAsync(async (id, ct) => await _queryRepo.SalesGroupExistsAsync(id))
                            .WithMessage($"SalesGroupId {rule.Error}")
                            .When(x => x.SalesGroupIds != null && x.SalesGroupIds.Count > 0);

                        // Validate each PaymentTermId exists (cross-module via lookup)
                        RuleFor(x => x.PaymentTermIds)
                            .MustAsync(async (ids, ct) =>
                            {
                                if (ids == null || ids.Count == 0) return true;
                                var allTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                                var validIds = allTerms.Select(pt => pt.Id).ToHashSet();
                                return ids.All(id => validIds.Contains(id));
                            })
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.PaymentTermIds != null && x.PaymentTermIds.Count > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Slabs).ChildRules(slab =>
                        {
                            slab.RuleFor(s => s.DiscountValue)
                                .GreaterThan(0)
                                .WithMessage($"Slab DiscountValue {rule.Error}");
                        }).When(x => x.Slabs != null && x.Slabs.Count > 0);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.Slabs).ChildRules(slab =>
                        {
                            slab.RuleFor(s => s.FromValue)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"FromValue {rule.Error}");
                        }).When(x => x.Slabs != null && x.Slabs.Count > 0);
                        break;

                    default:
                        break;
                }
            }

            // Custom: Slab ToValue >= FromValue when ToValue is provided
            RuleForEach(x => x.Slabs).ChildRules(slab =>
            {
                slab.RuleFor(s => s.ToValue)
                    .GreaterThanOrEqualTo(s => s.FromValue)
                    .WithMessage("ToValue must be greater than or equal to FromValue.")
                    .When(s => s.ToValue.HasValue);
            }).When(x => x.Slabs != null && x.Slabs.Count > 0);

            // Custom: MaxDiscountValue required when MaxDiscountLimitType is Max % or Max Amount
            RuleFor(x => x.MaxDiscountValue)
                .NotNull()
                .WithMessage("MaxDiscountValue is required when Max Discount Limit is Max % or Max Amount.")
                .GreaterThan(0)
                .WithMessage("MaxDiscountValue must be greater than zero.")
                .When(x => x.MaxDiscountLimitTypeId.HasValue && x.MaxDiscountLimitTypeId > 0);
        }
    }
}
