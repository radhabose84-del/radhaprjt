using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DeliveryChallan
{
    public class DeleteDeliveryChallanCommandValidator : AbstractValidator<DeleteDeliveryChallanCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryChallanQueryRepository _queryRepository;

        public DeleteDeliveryChallanCommandValidator(IDeliveryChallanQueryRepository queryRepository)
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
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteDeliveryChallanCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Delivery Challan {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
