using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.RejectFreightRfq;
using PurchaseManagement.Domain.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class RejectFreightRfqCommandValidator : AbstractValidator<RejectFreightRfqCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;

        public RejectFreightRfqCommandValidator(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            RuleFor(x => x.FreightRfqId)
                .GreaterThan(0).WithMessage("Valid Freight RFQ Id is required.")
                .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                .WithName("Freight RFQ")
                .WithMessage("Freight RFQ does not exist.");

            // E2 — only a Pending RFQ can be rejected.
            RuleFor(x => x.FreightRfqId)
                .MustAsync(async (id, ct) => (await _queryRepository.GetStatusCodeAsync(id)) == MiscEnumEntity.Pending)
                .WithMessage("Only a Freight RFQ pending approval can be rejected.")
                .WithName("Freight RFQ")
                .When(x => x.FreightRfqId > 0);

            RuleFor(x => x.ApprovalRemarks)
                .NotEmpty().WithMessage("Approval remarks are required when rejecting a Freight RFQ.");
        }
    }
}
