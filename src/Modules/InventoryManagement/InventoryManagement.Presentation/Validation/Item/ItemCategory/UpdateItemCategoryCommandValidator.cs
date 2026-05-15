using Contracts.Interfaces.Lookups.Users;
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
        private readonly IModuleLookup _moduleLookup;

        public UpdateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository,
            IItemCategoryQueryRepository itemCategoryQueryRepository,
            IModuleLookup moduleLookup)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _itemCategoryQueryRepository = itemCategoryQueryRepository;
            _moduleLookup = moduleLookup;

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

                        RuleFor(x => x.ModuleIds)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .Must(ids => ids != null && ids.Count > 0)
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ModuleIds)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.ItemCategoryName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ItemCategoryName)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.EmergencyPoLimit)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.EmergencyPoLimit)} {rule.Error}")
                            .When(x => x.EmergencyPoApplicable == 1 && x.EmergencyPoLimit.HasValue);
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
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .When(x => x.ModuleIds != null && x.ModuleIds.Count > 0);
                        break;

                    case "AlreadyExists":
                        // Handled below with full command access (needs cmd.Id to exclude self)
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

            // AlreadyExists: exclude self by passing cmd.Id
            RuleFor(x => x)
                .MustAsync(async (cmd, cancellation) =>
                    !await _itemCategoryCommandRepository.IsNameDuplicateAsync(cmd.ItemCategoryName!, cmd.Id))
                .WithName(nameof(UpdateItemCategoryCommand.ItemCategoryName))
                .WithMessage("A Category Name already exists.");
        }
    }
}
