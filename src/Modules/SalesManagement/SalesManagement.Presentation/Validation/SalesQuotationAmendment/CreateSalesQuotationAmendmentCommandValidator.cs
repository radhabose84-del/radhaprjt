using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesQuotationAmendment
{
    public class CreateSalesQuotationAmendmentCommandValidator
        : AbstractValidator<CreateSalesQuotationAmendmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesQuotationAmendmentQueryRepository _queryRepo;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;

        public CreateSalesQuotationAmendmentCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ISalesQuotationAmendmentQueryRepository queryRepo,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService)
        {
            _queryRepo = queryRepo;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;

            var maxLengthReason = maxLengthProvider.GetMaxLength<SalesQuotationAmendmentHeader>("Reason") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.SalesQuotationHeaderId)
                            .GreaterThan(0)
                            .WithMessage($"SalesQuotationHeaderId {rule.Error}");

                        RuleFor(x => x.Reason)
                            .NotNull().WithMessage($"Reason {rule.Error}")
                            .NotEmpty().WithMessage($"Reason {rule.Error}");

                        RuleFor(x => x.AmendmentDetails)
                            .NotNull().WithMessage($"AmendmentDetails {rule.Error}")
                            .Must(d => d != null && d.Count > 0)
                            .WithMessage("At least one amendment detail line is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Reason)
                            .MaximumLength(maxLengthReason)
                            .WithMessage($"Reason {rule.Error} {maxLengthReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Reason));
                        break;

                    case "FKColumnDelete":
                        // Quotation must exist and be Approved
                        RuleFor(x => x.SalesQuotationHeaderId)
                            .MustAsync(async (id, ct) =>
                                await _queryRepo.SalesQuotationExistsAndApprovedAsync(id))
                            .WithMessage("Only approved sales quotations can be amended.")
                            .When(x => x.SalesQuotationHeaderId > 0);

                        // No pending amendment
                        RuleFor(x => x.SalesQuotationHeaderId)
                            .MustAsync(async (id, ct) =>
                                !await _queryRepo.HasPendingAmendmentAsync(id))
                            .WithMessage("An amendment is already pending approval for this quotation.")
                            .When(x => x.SalesQuotationHeaderId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.AmendmentDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.SalesQuotationDetailId)
                                    .GreaterThan(0)
                                    .WithMessage($"SalesQuotationDetailId {rule.Error}");

                                detail.RuleFor(d => d.NewQuantity)
                                    .GreaterThan(0)
                                    .WithMessage($"NewQuantity {rule.Error}")
                                    .When(d => d.NewQuantity.HasValue);

                                detail.RuleFor(d => d.NewExMillRate)
                                    .GreaterThan(0)
                                    .WithMessage($"NewExMillRate {rule.Error}")
                                    .When(d => d.NewExMillRate.HasValue);

                                detail.RuleFor(d => d.NewItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"NewItemId {rule.Error}")
                                    .When(d => d.NewItemId.HasValue);

                                detail.RuleFor(d => d.NewHSNId)
                                    .GreaterThan(0)
                                    .WithMessage($"NewHSNId {rule.Error}")
                                    .When(d => d.NewHSNId.HasValue);
                            })
                            .When(x => x.AmendmentDetails != null && x.AmendmentDetails.Count > 0);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.AmendmentDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.NewDiscount)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"NewDiscount {rule.Error}")
                                    .When(d => d.NewDiscount.HasValue);

                                detail.RuleFor(d => d.NewTaxPercentage)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"NewTaxPercentage {rule.Error}")
                                    .When(d => d.NewTaxPercentage.HasValue);
                            })
                            .When(x => x.AmendmentDetails != null && x.AmendmentDetails.Count > 0);

                        RuleFor(x => x.FreightCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"FreightCharges {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"OtherCharges {rule.Error}");
                        break;

                    case "Workflow":
                        RuleFor(x => x.AmendmentDetails)
                            .MustAsync(async (details, cancellation) =>
                            {
                                var unitId = _ipAddressService.GetUnitId() ?? 0;
                                if (unitId <= 0) return false;
                                return await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                    MiscEnumEntity.TransactionTypeSalesQuotationAmendment,
                                    unitId,
                                    0);
                            })
                            .WithMessage(rule.Error)
                            .When(x => x.AmendmentDetails != null);
                        break;

                    default:
                        break;
                }
            }

            // No duplicate SalesQuotationDetailId within the same amendment
            RuleFor(x => x.AmendmentDetails)
                .Must(details =>
                {
                    if (details == null) return true;
                    var ids = details.Select(d => d.SalesQuotationDetailId).ToList();
                    return ids.Distinct().Count() == ids.Count;
                })
                .WithMessage("Duplicate SalesQuotationDetailId found in amendment details.")
                .When(x => x.AmendmentDetails != null && x.AmendmentDetails.Count > 0);
        }
    }
}
