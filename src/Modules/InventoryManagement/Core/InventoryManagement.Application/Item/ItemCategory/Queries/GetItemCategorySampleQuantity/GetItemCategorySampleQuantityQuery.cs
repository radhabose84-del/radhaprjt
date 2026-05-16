using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategorySampleQuantity
{
    public class GetItemCategorySampleQuantityQuery : IRequest<SampleQuantityDto?>
    {
        public int ItemCategoryId { get; set; }
        public int UnitId { get; set; }
        public int UOMId { get; set; }
    }
}
