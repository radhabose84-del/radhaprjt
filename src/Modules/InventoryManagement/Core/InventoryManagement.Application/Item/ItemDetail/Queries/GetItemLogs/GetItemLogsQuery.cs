using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs
{
    public sealed class GetItemLogsQuery 
        : IRequest<(List<ItemLogDto> Items, int TotalCount)>
    {
        public ItemLogFilter Filter { get; }
        public GetItemLogsQuery(ItemLogFilter filter) => Filter = filter;
    }
}
