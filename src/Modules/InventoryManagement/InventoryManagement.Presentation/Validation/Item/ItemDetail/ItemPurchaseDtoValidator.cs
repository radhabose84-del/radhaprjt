using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;

namespace InventoryManagement.Presentation.Validation.Item.ItemDetail
{
    public sealed class ItemPurchaseDtoValidator : AbstractValidator<ItemPurchaseDto>
    {
        public ItemPurchaseDtoValidator()
        {
            RuleFor(x => x.PurchaseUomId).GreaterThan(0).When(x => x.PurchaseUomId.HasValue);
            RuleFor(x => x.LeadTimeDays).InclusiveBetween(0, 365).When(x => x.LeadTimeDays.HasValue);
            RuleFor(x => x.GrProcessingTimeDays).InclusiveBetween(0, 60).When(x => x.GrProcessingTimeDays.HasValue);
        }
    }
}