using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;
using InventoryManagement.API.Validation.Common;

namespace InventoryManagement.API.Validation.Item.ItemDetail
{
    public sealed class ItemSupplierDtoValidator : AbstractValidator<ItemSupplierDto>
    {
        public ItemSupplierDtoValidator(IMaxLengthProvider maxLen)
        {
            RuleFor(x => x.SupplierId).GreaterThan(0);            

            var partMax = maxLen.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemSupplier>(nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemSupplier.SupplierPartNo)) ?? 100;
            RuleFor(x => x.SupplierPartNo).MaximumLength(partMax);
        }
    }
}