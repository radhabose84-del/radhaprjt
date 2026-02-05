using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete
{
    public class GetItemCategoryAutoCompleteQuery : IRequest<List<ItemCategoryAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
        public int? GroupId { get; set; }
        public bool? IsGroup { get; set; } = false;
        public int excludeId { get; set; } = 0;
    }
}