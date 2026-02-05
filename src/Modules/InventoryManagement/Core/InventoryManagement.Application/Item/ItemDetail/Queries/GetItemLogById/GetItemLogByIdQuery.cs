using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById
{
    public sealed class GetItemLogByIdQuery : IRequest<ItemLogDto?>
    {
        public int Id { get; }
        public GetItemLogByIdQuery(int id) => Id = id;
    }
}
