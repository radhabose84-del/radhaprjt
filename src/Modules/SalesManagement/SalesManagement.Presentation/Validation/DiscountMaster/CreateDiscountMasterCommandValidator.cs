using Contracts.Interfaces.Lookups.Purchase;
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

        public CreateDiscountMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDiscountMasterQueryRepository queryRepo,
            IPaymentTermLookup paymentTermLookup)
        {
            _queryRepo = queryRepo;
            _paymentTermLookup = paymentTermLookup;

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

                        RuleFor(x => x.DiscountTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountTypeId)} {rule.Error}");

                        RuleFor(x => x.ApplicableLevelId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ApplicableLevelId)} {rule.Error}");

                        RuleFor(x => x.TriggerEventId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.TriggerEventId)} {rule.Error}");

                        RuleFor(x => x.ValueTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ValueTypeId)} {rule.Error}");
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
                        RuleFor(x => x.DiscountTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.DiscountTypeId)} {rule.Error}")
                            .When(x => x.DiscountTypeId > 0);

                        RuleFor(x => x.ApplicableLevelId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ApplicableLevelId)} {rule.Error}")
                            .When(x => x.ApplicableLevelId > 0);

                        RuleFor(x => x.TriggerEventId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.TriggerEventId)} {rule.Error}")
                            .When(x => x.TriggerEventId > 0);

                        RuleFor(x => x.ValueTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.ValueTypeId)} {rule.Error}")
                            .When(x => x.ValueTypeId > 0);

                        RuleFor(x => x.MaxDiscountLimitTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.MaxDiscountLimitTypeId)} {rule.Error}")
                            .When(x => x.MaxDiscountLimitTypeId.HasValue && x.MaxDiscountLimitTypeId > 0);

                        RuleFor(x => x.SlabTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateDiscountMasterCommand.SlabTypeId)} {rule.Error}")
                            .When(x => x.SlabTypeId.HasValue && x.SlabTypeId > 0);

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
                        // DiscountValue must be > 0 for Standard type (validated via custom rule below)
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

            // Custom: DiscountValue required and > 0 for Standard type
            RuleFor(x => x.DiscountValue)
                .NotNull()
                .WithMessage("DiscountValue is required for Standard discount type.")
                .GreaterThan(0)
                .WithMessage("DiscountValue must be greater than zero.")
                .When(x => x.Slabs == null || x.Slabs.Count == 0);

            // Custom: SlabTypeId required for Slab type
            RuleFor(x => x.SlabTypeId)
                .NotNull()
                .WithMessage("SlabTypeId is required for Slab discount type.")
                .GreaterThan(0)
                .WithMessage("SlabTypeId is required for Slab discount type.")
                .When(x => x.Slabs != null && x.Slabs.Count > 0);

            // Custom: At least one slab required when SlabTypeId is provided
            RuleFor(x => x.Slabs)
                .Must(s => s != null && s.Count > 0)
                .WithMessage("At least one slab must be defined.")
                .When(x => x.SlabTypeId.HasValue && x.SlabTypeId > 0);

            // Custom: Slab ToValue >= FromValue when ToValue is provided
            RuleForEach(x => x.Slabs).ChildRules(slab =>
            {
                slab.RuleFor(s => s.ToValue)
                    .GreaterThanOrEqualTo(s => s.FromValue)
                    .WithMessage("ToValue must be greater than or equal to FromValue.")
                    .When(s => s.ToValue.HasValue);
            }).When(x => x.Slabs != null && x.Slabs.Count > 0);
        }
    }
}
