using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetStockItemBased
{
    public class GetStockItemBasedQuery : IRequest<List<GetStockItemDto>>
    {
         public int ItemId { get; set; }
        public int WarehouseId { get; set; }
    }
}