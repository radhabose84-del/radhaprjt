using Contracts.Dtos.Stock;
using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetStockItems
{
    public class GetStockItemsQuery : IRequest<List<StockItemSummaryDto>>
    {
        public int ProductionYear { get; set; }
    }
}
