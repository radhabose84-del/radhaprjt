using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete
{
    public class GetItemGroupAutoCompleteQuery : IRequest<List<ItemGroupAutoCompleteDto>>    
    {
        public string? SearchPattern { get; set; }       
    }
}