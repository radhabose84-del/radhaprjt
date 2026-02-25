using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetStockItemBased
{
    public class GetStockItemBasedQuery : IRequest<List<GetStockItemDto>>
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
      
    }
}