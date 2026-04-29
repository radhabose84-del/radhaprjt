using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesQuotation
{
    public class CreateSalesQuotationCommandValidator : AbstractValidator<CreateSalesQuotationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CreateSalesQuotationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesQuotationQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesQuotationHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.CustomerId)
                            .NotNull()
                            .WithMessage($"CustomerId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"CustomerId {rule.Error}");

                        RuleFor(x => x.QuotationDate)
                            .NotNull()
                            .WithMessage($"QuotationDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"QuotationDate {rule.Error}");

                        RuleFor(x => x.ValidityDate)
                            .NotNull()
                            .WithMessage($"ValidityDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"ValidityDate {rule.Error}");

                        RuleFor(x => x.PaymentTermId)
                            .NotNull()
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PaymentTermId {rule.Error}");

                        RuleFor(x => x.DeliveryTermId)
                            .NotNull()
                            .WithMessage($"DeliveryTermId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"DeliveryTermId {rule.Error}");

                        RuleFor(x => x.SalesQuotationDetails)
                            .NotNull()
                            .WithMessage($"SalesQuotationDetails {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesQuotationDetails {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "DateCompare":
                        RuleFor(x => x.ValidityDate)
                            .GreaterThanOrEqualTo(x => x.QuotationDate)
                            .WithMessage($"ValidityDate {rule.Error} QuotationDate.");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.FreightCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"FreightCharges {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"OtherCharges {rule.Error}");

                        RuleForEach(x => x.SalesQuotationDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.Discount)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"Discount {rule.Error}");
                            })
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.Any());
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.SalesQuotationDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.Quantity)
                                    .GreaterThan(0)
                                    .WithMessage($"Quantity {rule.Error}");

                                detail.RuleFor(d => d.ExMillRate)
                                    .GreaterThan(0)
                                    .WithMessage($"ExMillRate {rule.Error}");

                                detail.RuleFor(d => d.HSNId)
                                    .GreaterThan(0)
                                    .WithMessage($"HSNId {rule.Error}");

                                detail.RuleFor(d => d.DiscountTypeId)
                                    .NotNull()
                                    .GreaterThan(0)
                                    .WithMessage($"DiscountTypeId {rule.Error}");
                            })
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.Any());
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (customerId, ct) =>
                                await _queryRepository.CustomerExistsAsync(customerId))
                            .WithMessage($"CustomerId {rule.Error}")
                            .When(x => x.CustomerId > 0);

                        RuleFor(x => x.PaymentTermId)
                            .MustAsync(async (paymentTermId, ct) =>
                                await _queryRepository.PaymentTermExistsAsync(paymentTermId))
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.PaymentTermId > 0);

                        RuleFor(x => x.DeliveryTermId)
                            .MustAsync(async (deliveryTermId, ct) =>
                                await _queryRepository.DeliveryTermExistsAsync(deliveryTermId))
                            .WithMessage($"DeliveryTermId {rule.Error}")
                            .When(x => x.DeliveryTermId > 0);

                        RuleFor(x => x.SalesEnquiryId)
                            .MustAsync(async (salesEnquiryId, ct) =>
                                await _queryRepository.SalesEnquiryExistsAsync(salesEnquiryId!.Value))
                            .WithMessage($"SalesEnquiryId {rule.Error}")
                            .When(x => x.SalesEnquiryId.HasValue && x.SalesEnquiryId > 0);

                      /*   RuleFor(x => x.ContactPersonId)
                            .MustAsync(async (contactPersonId, ct) =>
                                await _queryRepository.ContactPersonExistsAsync(contactPersonId!.Value))
                            .WithMessage($"ContactPersonId {rule.Error}")
                            .When(x => x.ContactPersonId.HasValue && x.ContactPersonId > 0); */

                        // Detail-level FK validation
                        RuleForEach(x => x.SalesQuotationDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .MustAsync(async (itemId, ct) =>
                                        await _queryRepository.ItemExistsAsync(itemId))
                                    .WithMessage($"ItemId {rule.Error}")
                                    .When(d => d.ItemId > 0);

                                detail.RuleFor(d => d.HSNId)
                                    .MustAsync(async (hsnId, ct) =>
                                        await _queryRepository.HSNExistsAsync(hsnId))
                                    .WithMessage($"HSNId {rule.Error}")
                                    .When(d => d.HSNId > 0);

                                detail.RuleFor(d => d.VariantId)
                                    .MustAsync(async (variantId, ct) =>
                                        await _queryRepository.VariantExistsAsync(variantId!.Value))
                                    .WithMessage($"VariantId {rule.Error}")
                                    .When(d => d.VariantId.HasValue && d.VariantId.Value > 0);

                                detail.RuleFor(d => d.UOMId)
                                    .MustAsync(async (uomId, ct) =>
                                        await _queryRepository.UOMExistsAsync(uomId!.Value))
                                    .WithMessage($"UOMId {rule.Error}")
                                    .When(d => d.UOMId.HasValue && d.UOMId.Value > 0);

                                detail.RuleFor(d => d.DiscountTypeId)
                                    .MustAsync(async (discountTypeId, ct) =>
                                        await _queryRepository.DiscountTypeExistsAsync(discountTypeId!.Value))
                                    .WithMessage($"DiscountTypeId {rule.Error}")
                                    .When(d => d.DiscountTypeId.HasValue && d.DiscountTypeId.Value > 0);
                            })
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.Any());
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id, ct))
                            .WithMessage($"CustomerId {rule.Error}")
                            .When(x => x.CustomerId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
