using FluentValidation;
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

        public CreateSalesQuotationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesQuotationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.SalesQuotationDetails)
                            .NotNull()
                            .WithMessage($"SalesQuotationDetails {rule.Error}");

                        RuleFor(x => x.SalesQuotationDetails!.CustomerId)
                            .NotNull()
                            .WithMessage($"CustomerId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"CustomerId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.QuotationDate)
                            .NotNull()
                            .WithMessage($"QuotationDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"QuotationDate {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.ValidityDate)
                            .NotNull()
                            .WithMessage($"ValidityDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"ValidityDate {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.PaymentTermId)
                            .NotNull()
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.DeliveryTermId)
                            .NotNull()
                            .WithMessage($"DeliveryTermId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"DeliveryTermId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.SalesQuotationDetails)
                            .NotNull()
                            .WithMessage($"SalesQuotationDetails {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"SalesQuotationDetails {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesQuotationDetails!.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => x.SalesQuotationDetails != null && !string.IsNullOrWhiteSpace(x.SalesQuotationDetails.Remarks));
                        break;

                    case "DateCompare":
                        RuleFor(x => x.SalesQuotationDetails!.ValidityDate)
                            .GreaterThanOrEqualTo(x => x.SalesQuotationDetails!.QuotationDate)
                            .WithMessage($"ValidityDate {rule.Error} QuotationDate.")
                            .When(x => x.SalesQuotationDetails != null);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.SalesQuotationDetails!.FreightCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"FreightCharges {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleFor(x => x.SalesQuotationDetails!.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"OtherCharges {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null);

                        RuleForEach(x => x.SalesQuotationDetails!.SalesQuotationDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.Discount)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"Discount {rule.Error}");
                            })
                            .When(x => x.SalesQuotationDetails?.SalesQuotationDetails != null && x.SalesQuotationDetails.SalesQuotationDetails.Any());
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.SalesQuotationDetails!.SalesQuotationDetails)
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
                            })
                            .When(x => x.SalesQuotationDetails?.SalesQuotationDetails != null && x.SalesQuotationDetails.SalesQuotationDetails.Any());
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesQuotationDetails!.CustomerId)
                            .MustAsync(async (customerId, ct) =>
                                await _queryRepository.CustomerExistsAsync(customerId))
                            .WithMessage($"CustomerId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.CustomerId > 0);

                        RuleFor(x => x.SalesQuotationDetails!.PaymentTermId)
                            .MustAsync(async (paymentTermId, ct) =>
                                await _queryRepository.PaymentTermExistsAsync(paymentTermId))
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.PaymentTermId > 0);

                        RuleFor(x => x.SalesQuotationDetails!.DeliveryTermId)
                            .MustAsync(async (deliveryTermId, ct) =>
                                await _queryRepository.DeliveryTermExistsAsync(deliveryTermId))
                            .WithMessage($"DeliveryTermId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.DeliveryTermId > 0);

                        RuleFor(x => x.SalesQuotationDetails!.SalesEnquiryId)
                            .MustAsync(async (salesEnquiryId, ct) =>
                                await _queryRepository.SalesEnquiryExistsAsync(salesEnquiryId!.Value))
                            .WithMessage($"SalesEnquiryId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.SalesEnquiryId.HasValue && x.SalesQuotationDetails.SalesEnquiryId > 0);

                        RuleFor(x => x.SalesQuotationDetails!.ContactPersonId)
                            .MustAsync(async (contactPersonId, ct) =>
                                await _queryRepository.ContactPersonExistsAsync(contactPersonId!.Value))
                            .WithMessage($"ContactPersonId {rule.Error}")
                            .When(x => x.SalesQuotationDetails != null && x.SalesQuotationDetails.ContactPersonId.HasValue && x.SalesQuotationDetails.ContactPersonId > 0);

                        // Detail-level FK validation
                        RuleForEach(x => x.SalesQuotationDetails!.SalesQuotationDetails)
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
                            })
                            .When(x => x.SalesQuotationDetails?.SalesQuotationDetails != null && x.SalesQuotationDetails.SalesQuotationDetails.Any());
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
