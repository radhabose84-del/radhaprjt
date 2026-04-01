using Contracts.Dtos.Stock;
using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetStockItems
{
    public class GetStockItemsQuery : IRequest<IReadOnlyList<StockItemSummaryDto>>
    {
        public int ProductionYear { get; set; }
        public int? PackTypeId { get; set; }
    }
}
