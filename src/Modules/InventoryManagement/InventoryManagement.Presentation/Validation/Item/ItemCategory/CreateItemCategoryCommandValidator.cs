using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.Shared;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemCategory
{
    public class CreateItemCategoryCommandValidator : AbstractValidator<CreateItemCategoryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IModuleLookup _moduleLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IUOMLookup _uomLookup;

        public CreateItemCategoryCommandValidator(
            IMaxLengthProvider maxLengthProvider,
            IItemCategoryCommandRepository itemCategoryCommandRepository,
            IModuleLookup moduleLookup,
            IUnitLookup unitLookup,
            IUOMLookup uomLookup)
        {
            var maxLength = maxLengthProvider
                .GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>("ItemCategoryName") ?? 100;

            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _moduleLookup = moduleLookup;
            _unitLookup = unitLookup;
            _uomLookup = uomLookup;
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

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.EmgencyValueLimit)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateItemCategoryCommand.EmgencyValueLimit)} {rule.Error}")
                            .When(x => x.EmergencyPOById.HasValue && x.EmgencyValueLimit.HasValue);
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
                            .WithMessage($"{nameof(CreateItemCategoryCommand.ModuleIds)} {rule.Error}")
                            .When(x => x.ModuleIds != null && x.ModuleIds.Count > 0);
                        break;
                }
            }

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
        }
    }
}
