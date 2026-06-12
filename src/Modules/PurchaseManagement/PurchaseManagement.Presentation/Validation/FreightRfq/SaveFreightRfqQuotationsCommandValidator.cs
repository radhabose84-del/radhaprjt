using Contracts.Interfaces.Lookups.Party;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations;
using PurchaseManagement.Domain.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class SaveFreightRfqQuotationsCommandValidator : AbstractValidator<SaveFreightRfqQuotationsCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly ITransporterLookup _transporterLookup;

        public SaveFreightRfqQuotationsCommandValidator(
            IFreightRfqQueryRepository queryRepository,
            ITransporterLookup transporterLookup)
        {
            _queryRepository = queryRepository;
            _transporterLookup = transporterLookup;

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

            // R4 — quotations can only be saved while the RFQ is still in Quotation Pending.
            RuleFor(x => x.FreightRfqId)
                .MustAsync(async (id, ct) => (await _queryRepository.GetStatusCodeAsync(id)) == MiscEnumEntity.FreightRfqQuotationPending)
                .WithMessage("Quotations can only be saved while the Freight RFQ is in Quotation Pending.")
                .WithName("Freight RFQ")
                .When(x => x.FreightRfqId > 0);

            // R1 — at least one transporter row.
            RuleFor(x => x.Quotations)
                .NotEmpty().WithMessage("At least one transporter is required.");

            RuleForEach(x => x.Quotations).ChildRules(row =>
            {
                // Quoted rate is optional until the transporter replies; when present it must be positive.
                row.RuleFor(q => q.QuotedRate)
                    .GreaterThan(0).WithMessage("Quoted Rate must be greater than zero.")
                    .When(q => q.QuotedRate.HasValue);

                // R2 / E3 — only active transporters can be quoted.
                row.RuleFor(q => q.TransporterId)
                    .GreaterThan(0).WithMessage("Transporter is required.")
                    .MustAsync(async (transporterId, ct) =>
                        await _transporterLookup.GetActiveTransporterByIdAsync(transporterId, ct) != null)
                    .WithMessage("Transporter is inactive or does not exist.")
                    .When(q => q.TransporterId > 0);

                // Rate basis is optional until a quote is entered; when present it must exist.
                row.RuleFor(q => q.RateBasisId)
                    .MustAsync(async (rateBasisId, ct) => await _queryRepository.MiscExistsAsync(rateBasisId!.Value))
                    .WithMessage("Rate Basis is inactive or does not exist.")
                    .When(q => q.RateBasisId.HasValue && q.RateBasisId.Value > 0);
            });
        }
    }
}
