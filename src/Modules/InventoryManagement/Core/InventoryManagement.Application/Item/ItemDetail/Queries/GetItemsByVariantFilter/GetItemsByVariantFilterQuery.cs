using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByVariantFilter
{
    public class GetItemsByVariantFilterQuery : IRequest<List<GetItemAutoCompleteDto>>
    {
        public bool? HasVariant { get; set; }
        public int? ParentItemId { get; set; }
        public int? ModuleId { get; set; }
    }
}
