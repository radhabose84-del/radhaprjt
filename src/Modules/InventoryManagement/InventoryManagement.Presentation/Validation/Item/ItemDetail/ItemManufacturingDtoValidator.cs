using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.Item.ItemDetail
{
    public sealed class ItemManufacturingDtoValidator : AbstractValidator<ItemManufactureDto>
    {
        public ItemManufacturingDtoValidator()
        {
            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.ManufacturingTypeId).GreaterThan(0);
        }
    }
}