using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.DeleteFreightRfq;
using PurchaseManagement.Domain.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.FreightRfq
{
    public class DeleteFreightRfqCommandValidator : AbstractValidator<DeleteFreightRfqCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFreightRfqQueryRepository _queryRepository;

        public DeleteFreightRfqCommandValidator(IFreightRfqQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty().WithMessage($"{nameof(DeleteFreightRfqCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => await _queryRepository.NotFoundAsync(id))
                            .WithName("Freight RFQ")
                            .WithMessage($"Freight RFQ {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // R4 — an Approved or Pending RFQ is locked; only Draft / Rejected can be deleted.
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) =>
                {
                    var status = await _queryRepository.GetStatusCodeAsync(id);
                    return status != MiscEnumEntity.Approved && status != MiscEnumEntity.Pending;
                })
                .WithMessage("An approved or pending Freight RFQ cannot be deleted.")
                .WithName("Freight RFQ")
                .When(x => x.Id > 0);
        }
    }
}
