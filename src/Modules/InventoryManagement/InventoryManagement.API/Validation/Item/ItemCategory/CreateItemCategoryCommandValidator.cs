using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using FluentValidation;
using InventoryManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.API.Validation.Item.ItemCategory
{
    public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;

        public CreateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository)
        {
            var maxLength = maxLengthProvider
                .GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>("ItemCategoryName") ?? 100;

            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
                throw new ArgumentException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ItemCategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ItemCategoryName)} {rule.Error}");

                        RuleFor(x => x.ItemGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ItemGroupId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ItemCategoryName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ItemCategoryName)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ItemCategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ItemCategoryName)} {rule.Error}")
                            .MustAsync(async (name, cancellation) =>
                                !await _itemCategoryCommandRepository.ExistsByNameAsync(name!))
                            .WithMessage("A Category Name already exists.");
                        break;
                }
            }
        }
    }
}
