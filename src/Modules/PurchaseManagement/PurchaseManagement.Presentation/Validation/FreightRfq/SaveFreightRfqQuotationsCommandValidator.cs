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

            // R4 — quotations can only be saved on a Draft RFQ.
            RuleFor(x => x.FreightRfqId)
                .MustAsync(async (id, ct) => (await _queryRepository.GetStatusCodeAsync(id)) == MiscEnumEntity.Draft)
                .WithMessage("Quotations can only be saved on a Draft Freight RFQ.")
                .WithName("Freight RFQ")
                .When(x => x.FreightRfqId > 0);

            // R1 — at least one quotation row.
            RuleFor(x => x.Quotations)
                .NotEmpty().WithMessage("At least one transporter quotation is required.");

            RuleForEach(x => x.Quotations).ChildRules(row =>
            {
                row.RuleFor(q => q.QuotedRate)
                    .GreaterThan(0).WithMessage("Quoted Rate must be greater than zero.");

                // R2 / E3 — only active transporters can be quoted.
                row.RuleFor(q => q.TransporterId)
                    .GreaterThan(0).WithMessage("Transporter is required.")
                    .MustAsync(async (transporterId, ct) =>
                        await _transporterLookup.GetActiveTransporterByIdAsync(transporterId) != null)
                    .WithMessage("Transporter is inactive or does not exist.")
                    .When(q => q.TransporterId > 0);

                row.RuleFor(q => q.RateBasisId)
                    .GreaterThan(0).WithMessage("Rate Basis is required.")
                    .MustAsync(async (rateBasisId, ct) => await _queryRepository.MiscExistsAsync(rateBasisId))
                    .WithMessage("Rate Basis is inactive or does not exist.")
                    .When(q => q.RateBasisId > 0);
            });
        }
    }
}
