using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval;
using PurchaseManagement.Domain.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class SubmitFreightRfqForApprovalCommandValidator : AbstractValidator<SubmitFreightRfqForApprovalCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;

        public SubmitFreightRfqForApprovalCommandValidator(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // RFQ must exist.
            RuleFor(x => x.FreightRfqId)
                .GreaterThan(0).WithMessage("Valid Freight RFQ Id is required.")
                .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                .WithName("Freight RFQ")
                .WithMessage("Freight RFQ does not exist.");

            // E2 — only a Draft RFQ can be submitted (prevents duplicate submission).
            RuleFor(x => x.FreightRfqId)
                .MustAsync(async (id, ct) => (await _queryRepository.GetStatusCodeAsync(id)) == MiscEnumEntity.Draft)
                .WithMessage("Only a Draft Freight RFQ can be submitted for approval.")
                .WithName("Freight RFQ")
                .When(x => x.FreightRfqId > 0);

            // R1 — at least one quotation before submission.
            RuleFor(x => x.FreightRfqId)
                .MustAsync(async (id, ct) => await _queryRepository.GetQuotationCountAsync(id) > 0)
                .WithMessage("At least one transporter quotation is required before submission.")
                .WithName("Freight RFQ")
                .When(x => x.FreightRfqId > 0);

            // R3 / E1 — a selected transporter is mandatory. This required check must run even when
            // SelectedQuotationId == 0, so it is NOT gated on SelectedQuotationId > 0 (a trailing
            // .When() in FluentValidation applies to the whole RuleFor chain).
            RuleFor(x => x.SelectedQuotationId)
                .GreaterThan(0).WithMessage("A selected transporter is required before submission.")
                .When(x => x.FreightRfqId > 0);

            // ...and once a quotation is selected, it must belong to this RFQ.
            RuleFor(x => x.SelectedQuotationId)
                .MustAsync(async (cmd, quotationId, ct) =>
                    await _queryRepository.QuotationBelongsToRfqAsync(cmd.FreightRfqId, quotationId))
                .WithMessage("The selected transporter quotation does not belong to this Freight RFQ.")
                .When(x => x.FreightRfqId > 0 && x.SelectedQuotationId > 0);

            // Override selection requires a reason.
            RuleFor(x => x.ComparisonRemarks)
                .NotEmpty().WithMessage("A reason for selection is required when overriding the lowest quote.")
                .When(x => x.IsOverride);
        }
    }
}
