using Contracts.Interfaces.Lookups.Users;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemCategory
{
    public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IModuleLookup _moduleLookup;

        public CreateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository,
            IModuleLookup moduleLookup)
        {
            var maxLength = maxLengthProvider
                .GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>("ItemCategoryName") ?? 100;

            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _moduleLookup = moduleLookup;
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

                        RuleFor(x => x.ModuleIds)
                            .NotNull()
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .Must(ids => ids != null && ids.Count > 0)
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ModuleIds)} {rule.Error}");
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

                    case "FKColumnDelete":
                        RuleFor(x => x.ModuleIds)
                            .MustAsync(async (ids, cancellation) =>
                            {
                                if (ids == null || ids.Count == 0) return true; // covered by NotEmpty rule
                                var modules = await _moduleLookup.GetAllModuleAsync();
                                var validIds = modules.Select(m => m.ModuleId).ToHashSet();
                                return ids.Distinct().All(id => validIds.Contains(id));
                            })
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .When(x => x.ModuleIds != null && x.ModuleIds.Count > 0);
                        break;
                }
            }
        }
    }
}
