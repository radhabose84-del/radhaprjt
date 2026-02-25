using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemGroup
{
    public class DeleteItemGroupCommandValidator : AbstractValidator<DeleteItemGroupCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;        

        public DeleteItemGroupCommandValidator(IItemGroupQueryRepository itemGroupQueryRepository)
        {
            _itemGroupQueryRepository = itemGroupQueryRepository;            
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
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
                            .WithMessage($"{nameof(DeleteItemGroupCommand.Id)} {rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _itemGroupQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                        .MustAsync(async (Id, cancellation) => !await _itemGroupQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}