
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemDetail
{
    public sealed class ItemInventoryDtoValidator : AbstractValidator<ItemInventoryDto>
    {
        public ItemInventoryDtoValidator(IMaxLengthProvider maxLen)
        {
            RuleFor(x => x.Weight).GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue);
            RuleFor(x => x.WeightUomId).GreaterThan(0).When(x => x.WeightUomId.HasValue);
            RuleFor(x => x.DefaultMaterialRequestTypeId).GreaterThan(0).When(x => x.DefaultMaterialRequestTypeId.HasValue);
            RuleFor(x => x.ValuationMethodId).GreaterThan(0).When(x => x.ValuationMethodId.HasValue);

            RuleFor(x => x.ShelfLife).InclusiveBetween(0, 3650).When(x => x.ShelfLife.HasValue);
            RuleFor(x => x.UpperTolerance).GreaterThanOrEqualTo(0).When(x => x.UpperTolerance.HasValue);
            RuleFor(x => x.LowerTolerance).GreaterThanOrEqualTo(0).When(x => x.LowerTolerance.HasValue);

            // Reorder logic
            RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0).When(x => x.ReorderLevel.HasValue);
            RuleFor(x => x.ReorderQty).GreaterThanOrEqualTo(0).When(x => x.ReorderQty.HasValue);

            // If ApplyBatchNumber is true → BatchManagement must be true
            RuleFor(x => x.BatchManagement)
                .Equal(true)
                .When(x => x.ApplyBatchNumber)
                .WithMessage("Batch Management must be enabled when Apply Batch Number is checked.");

            var batchSeriesMax = maxLen.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemInventory>(nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemInventory.BatchNumberSeries)) ?? 100;
            var serialSeriesMax = maxLen.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemInventory>(nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemInventory.SerialNumberSeries)) ?? 100;
            RuleFor(x => x.BatchNumberSeries).MaximumLength(batchSeriesMax);
            RuleFor(x => x.SerialNumberSeries).MaximumLength(serialSeriesMax);
        }
    }
}