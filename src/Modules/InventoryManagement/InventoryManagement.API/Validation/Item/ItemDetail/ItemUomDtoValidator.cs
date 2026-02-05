using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;

namespace InventoryManagement.API.Validation.Item.ItemDetail
{
    public sealed class ItemUomDtoValidator : AbstractValidator<ItemUomDto>
    {
        public ItemUomDtoValidator()
        {            
            RuleFor(x => x.ConversionUOMId).GreaterThan(0).When(x => x.ConversionUOMId.HasValue);
            RuleFor(x => x.ConversionRate).GreaterThan(0).When(x => x.ConversionRate.HasValue);
        }
    }
}