using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.Shared;
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
        private readonly IUnitLookup _unitLookup;
        private readonly IUOMLookup _uomLookup;

        public UpdateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository,
            IItemCategoryQueryRepository itemCategoryQueryRepository,
            IModuleLookup moduleLookup,
            IUnitLookup unitLookup,
            IUOMLookup uomLookup)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _itemCategoryQueryRepository = itemCategoryQueryRepository;
            _moduleLookup = moduleLookup;
            _unitLookup = unitLookup;
            _uomLookup = uomLookup;

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
                                if (ids == null || ids.Count == 0) return true;
                                var modules = await _moduleLookup.GetAllModuleAsync();
                                var validIds = modules.Select(m => m.ModuleId).ToHashSet();
                                return ids.Distinct().All(id => validIds.Contains(id));
                            })
                            .WithMessage($"{nameof(UpdateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .When(x => x.ModuleIds != null && x.ModuleIds.Count > 0);
                        break;

                    case "AlreadyExists":
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

            RuleFor(x => x)
                .MustAsync(async (cmd, cancellation) =>
                    !await _itemCategoryCommandRepository.IsNameDuplicateAsync(cmd.ItemCategoryName!, cmd.Id))
                .WithName(nameof(UpdateItemCategoryCommand.ItemCategoryName))
                .WithMessage("A Category Name already exists.");

            RuleFor(x => x.SampleQuantities)
                .Must(list => list == null || list.Count == 0)
                .WithMessage("Sample quantity configuration is only allowed on leaf item categories (IsGroup = 0).")
                .When(x => x.IsGroup == 1);

            RuleFor(x => x.SampleQuantities)
                .Must(list => list == null || list.GroupBy(s => new { s.UnitId, s.UOMId }).All(g => g.Count() == 1))
                .WithMessage("Duplicate Unit + UOM combination is not allowed within the same Item Category.")
                .When(x => x.SampleQuantities != null && x.SampleQuantities.Count > 0);

            RuleFor(x => x.SampleQuantities)
                .MustAsync(async (list, cancellation) =>
                {
                    if (list == null || list.Count == 0) return true;
                    var unitIds = list.Select(s => s.UnitId).Distinct().ToList();
                    var units = await _unitLookup.GetByIdsAsync(unitIds, cancellation);
                    var validIds = units.Select(u => u.UnitId).ToHashSet();
                    return unitIds.All(id => validIds.Contains(id));
                })
                .WithMessage("One or more Units are inactive or do not exist.")
                .When(x => x.SampleQuantities != null && x.SampleQuantities.Count > 0);

            RuleFor(x => x.SampleQuantities)
                .MustAsync(async (list, cancellation) =>
                {
                    if (list == null || list.Count == 0) return true;
                    var uomIds = list.Select(s => s.UOMId).Distinct().ToList();
                    var uoms = await _uomLookup.GetByIdsAsync(uomIds, cancellation);
                    var validIds = uoms.Select(u => u.Id).ToHashSet();
                    return uomIds.All(id => validIds.Contains(id));
                })
                .WithMessage("One or more UOMs are inactive or do not exist.")
                .When(x => x.SampleQuantities != null && x.SampleQuantities.Count > 0);

            RuleForEach(x => x.SampleQuantities).ChildRules(child =>
            {
                child.RuleFor(s => s.UnitId)
                    .GreaterThan(0)
                    .WithMessage("Unit is required for each sample quantity row.");

                child.RuleFor(s => s.UOMId)
                    .GreaterThan(0)
                    .WithMessage("UOM is required for each sample quantity row.");

                child.RuleFor(s => s.MaxSampleQuantity)
                    .GreaterThan(0m)
                    .WithMessage("Maximum Sample Quantity must be greater than zero.");

                child.RuleFor(s => s.IsActive)
                    .InclusiveBetween((byte)0, (byte)1)
                    .WithMessage("IsActive must be either 0 or 1.");
            });

            RuleFor(x => x)
                .MustAsync(async (cmd, cancellation) =>
                {
                    var rowIds = (cmd.SampleQuantities ?? new())
                        .Where(s => s.Id.HasValue && s.Id.Value > 0)
                        .Select(s => s.Id!.Value)
                        .ToList();
                    if (rowIds.Count == 0) return true;
                    var existing = await _itemCategoryQueryRepository.GetSampleQuantitiesAsync(cmd.Id);
                    var existingIds = existing.Select(e => e.Id).ToHashSet();
                    return rowIds.All(id => existingIds.Contains(id));
                })
                .WithName(nameof(UpdateItemCategoryCommand.SampleQuantities))
                .WithMessage("One or more sample quantity row IDs do not belong to this category.");
        }
    }
}
