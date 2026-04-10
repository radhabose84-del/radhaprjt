using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.PriceGroupMaster
{
    public class DeletePriceGroupMasterCommandValidator : AbstractValidator<DeletePriceGroupMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPriceGroupMasterQueryRepository _queryRepository;

        public DeletePriceGroupMasterCommandValidator(IPriceGroupMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeletePriceGroupMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Price Group {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
