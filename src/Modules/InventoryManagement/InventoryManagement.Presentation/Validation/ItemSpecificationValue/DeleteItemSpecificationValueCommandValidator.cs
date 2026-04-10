using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.ItemSpecificationValue
{
    public class DeleteItemSpecificationValueCommandValidator : AbstractValidator<DeleteItemSpecificationValueCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemSpecificationValueQueryRepository _queryRepository;

        public DeleteItemSpecificationValueCommandValidator(IItemSpecificationValueQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteItemSpecificationValueCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ItemSpecificationValue {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
