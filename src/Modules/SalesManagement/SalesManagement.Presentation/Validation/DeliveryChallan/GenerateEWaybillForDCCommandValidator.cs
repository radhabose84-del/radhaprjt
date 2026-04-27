using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DeliveryChallan
{
    /// <summary>
    /// Validator for POST /api/deliveryChallan/{id}/generate-ewaybill.
    /// Ensures the DC exists before the handler fires (handler still re-checks for race safety).
    /// </summary>
    public class GenerateEWaybillForDCCommandValidator : AbstractValidator<GenerateEWaybillForDCCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryChallanQueryRepository _queryRepository;

        public GenerateEWaybillForDCCommandValidator(IDeliveryChallanQueryRepository queryRepository)
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
                        RuleFor(x => x.DeliveryChallanId)
                            .NotEmpty()
                            .WithMessage($"{nameof(GenerateEWaybillForDCCommand.DeliveryChallanId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.DeliveryChallanId)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Delivery Challan {rule.Error}")
                            .When(x => x.DeliveryChallanId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
