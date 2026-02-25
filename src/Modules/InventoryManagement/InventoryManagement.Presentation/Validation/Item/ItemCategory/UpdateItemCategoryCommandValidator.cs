using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemCategory
{
    public class UpdateItemCategoryCommandValidator : AbstractValidator<UpdateItemCategoryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IItemCategoryQueryRepository _itemCategoryQueryRepository;

        public UpdateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository,
            IItemCategoryQueryRepository itemCategoryQueryRepository)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _itemCategoryQueryRepository = itemCategoryQueryRepository;

            var maxLength = maxLengthProvider
                .GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>("ItemCategoryName") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ItemCategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ItemCategoryName)} {rule.Error}");

                        RuleFor(x => x.ItemGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ItemGroupId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ItemCategoryName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ItemCategoryName)} {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.ItemCategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ItemCategoryName)} {rule.Error}")
                            .MustAsync(async (name, cancellation) =>
                                !await _itemCategoryCommandRepository.IsNameDuplicateAsync(name!, /* id from instance */ 0))
                            .WithMessage("A Category Name already exists.");
                        // ⛔ This rule needs command.Id. Best version is below in NOTE.
                        break;

                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                (await _itemCategoryQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                        break;

                    case "LinkedWithOtherRecords":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                !await _itemCategoryQueryRepository.IsLinkedWithActiveItemsAsync(id))
                            .WithName("Item Category")
                            .WithMessage("Cannot update Item Category as it is linked with active items");
                        break;
                }
            }

            // ✅ NOTE (Fix for AlreadyExists correctly using command.Id)
            // Replace the AlreadyExists case above with this better rule:
            RuleFor(x => x)
                .MustAsync(async (cmd, cancellation) =>
                    !await _itemCategoryCommandRepository.IsNameDuplicateAsync(cmd.ItemCategoryName!, cmd.Id))
                .WithName(nameof(UpdateItemCategoryCommand.ItemCategoryName))
                .WithMessage("A Category Name already exists.");
        }
    }
}
