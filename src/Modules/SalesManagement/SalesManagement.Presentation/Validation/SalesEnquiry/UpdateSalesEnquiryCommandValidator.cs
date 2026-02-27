using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesEnquiry
{
    public class UpdateSalesEnquiryCommandValidator : AbstractValidator<UpdateSalesEnquiryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesEnquiryQueryRepository _queryRepository;

        public UpdateSalesEnquiryCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesEnquiryQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.PartyId)
                            .NotNull()
                            .WithMessage($"PartyId {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"PartyId {rule.Error}");

                        RuleFor(x => x.EnquiryDate)
                            .NotNull()
                            .WithMessage($"EnquiryDate {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"EnquiryDate {rule.Error}");

                        RuleFor(x => x.SalesEnquiryDetails)
                            .NotNull()
                            .WithMessage("At least one product line item is required.")
                            .NotEmpty()
                            .WithMessage("At least one product line item is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ContactPerson)
                            .MaximumLength(maxLengthContactPerson)
                            .WithMessage($"ContactPerson {rule.Error} {maxLengthContactPerson} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.ContactPerson));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"Remarks {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesEnquiry {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PartyId)
                            .MustAsync(async (partyId, ct) =>
                                await _queryRepository.PartyExistsAsync(partyId))
                            .WithMessage($"PartyId {rule.Error}")
                            .When(x => x.PartyId > 0);

                        RuleFor(x => x.PaymentTermId)
                            .MustAsync(async (paymentTermId, ct) =>
                                await _queryRepository.PaymentTermExistsAsync(paymentTermId!.Value))
                            .WithMessage($"PaymentTermId {rule.Error}")
                            .When(x => x.PaymentTermId.HasValue && x.PaymentTermId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"IsActive {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Detail-level validation
            RuleForEach(x => x.SalesEnquiryDetails)
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

                    detail.RuleFor(d => d.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity must be greater than zero.");
                })
                .When(x => x.SalesEnquiryDetails != null && x.SalesEnquiryDetails.Any());
        }
    }
}
