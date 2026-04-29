using FluentValidation;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using static SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry.CreateSalesEnquiryDto;

namespace SalesManagement.Presentation.Validation.SalesEnquiry
{
    public class CreateSalesEnquiryCommandValidator : AbstractValidator<CreateSalesEnquiryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CreateSalesEnquiryCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesEnquiryQueryRepository queryRepository,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _queryRepository = queryRepository;
            _accessFilter = accessFilter;

            var maxLengthContactPerson = maxLengthProvider.GetMaxLength<Domain.Entities.SalesEnquiryHeader>("ContactPerson") ?? 200;
            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.SalesEnquiryHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.SalesEnquiryDetails.PartyId)
                            .NotNull()
                            .WithMessage($"PartyId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PartyId {rule.Error}");

                        RuleFor(x => x.SalesEnquiryDetails.EnquiryDate)
                            .NotNull()
                            .WithMessage($"EnquiryDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"EnquiryDate {rule.Error}");

                        RuleFor(x => x.SalesEnquiryDetails.EnquiryTypeId)
                            .GreaterThan(0)
                            .WithMessage($"EnquiryTypeId {rule.Error}");

                        RuleFor(x => x.SalesEnquiryDetails.SalesEnquiryDetails)
                            .NotNull()
                            .WithMessage("At least one product line item is required.")
                            .NotEmpty()
                            .WithMessage("At least one product line item is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.SalesEnquiryDetails.ContactPerson)
                            .MaximumLength(maxLengthContactPerson)
                            .WithMessage($"ContactPerson {rule.Error} {maxLengthContactPerson} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesEnquiryDetails?.ContactPerson));

                        RuleFor(x => x.SalesEnquiryDetails.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.SalesEnquiryDetails?.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesEnquiryDetails.PartyId)
                            .MustAsync(async (partyId, ct) =>
                                await _queryRepository.PartyExistsAsync(partyId))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.SalesEnquiryDetails?.PartyId > 0);

                        RuleFor(x => x.SalesEnquiryDetails.PaymentTermId)
                            .MustAsync(async (paymentTermId, ct) =>
                                await _queryRepository.PaymentTermExistsAsync(paymentTermId!.Value))
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.SalesEnquiryDetails?.PaymentTermId.HasValue == true && x.SalesEnquiryDetails.PaymentTermId > 0);

                        RuleFor(x => x.SalesEnquiryDetails.SalesLeadId)
                            .MustAsync(async (salesLeadId, ct) =>
                                await _queryRepository.SalesLeadExistsAsync(salesLeadId!.Value))
                            .WithMessage($"SalesLeadId {rule.Error}")
                            .When(x => x.SalesEnquiryDetails?.SalesLeadId.HasValue == true && x.SalesEnquiryDetails.SalesLeadId > 0);

                        RuleFor(x => x.SalesEnquiryDetails.EnquiryTypeId)
                            .MustAsync(async (enquiryTypeId, ct) =>
                                await _queryRepository.EnquiryTypeExistsAsync(enquiryTypeId))
                            .WithMessage($"EnquiryTypeId {rule.Error}")
                            .When(x => x.SalesEnquiryDetails?.EnquiryTypeId > 0);
                        break;

                    case "MarketingOfficerAccess":
                        RuleFor(x => x.SalesEnquiryDetails.PartyId)
                            .MustAsync(async (id, ct) => await _accessFilter.CanAccessCustomerAsync(id, ct))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.SalesEnquiryDetails?.PartyId > 0);
                        break;

                    default:
                        break;
                }
            }

            // Detail-level validation using RuleForEach
            RuleForEach(x => x.SalesEnquiryDetails.SalesEnquiryDetails)
                .ChildRules(detail =>
                {
                    detail.RuleFor(d => d.ItemId)
                        .GreaterThan(0)
                        .WithMessage("ItemId is required for each product line.");

                    detail.RuleFor(d => d.ItemId)
                        .MustAsync(async (itemId, ct) =>
                            await _queryRepository.ItemExistsAsync(itemId))
                        .WithMessage("Item does not exist in Item Master.")
                        .When(d => d.ItemId > 0);

                    detail.RuleFor(d => d.VariantId)
                        .MustAsync(async (variantId, ct) =>
                            await _queryRepository.ItemExistsAsync(variantId!.Value))
                        .WithMessage("Variant does not exist in Item Master.")
                        .When(d => d.VariantId.HasValue && d.VariantId > 0);

                    detail.RuleFor(d => d.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity must be greater than zero.");
                })
                .When(x => x.SalesEnquiryDetails?.SalesEnquiryDetails != null && x.SalesEnquiryDetails.SalesEnquiryDetails.Any());
        }
    }
}
