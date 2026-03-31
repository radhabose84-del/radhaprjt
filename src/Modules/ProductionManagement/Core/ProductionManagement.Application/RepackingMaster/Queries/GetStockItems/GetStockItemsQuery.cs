using Contracts.Dtos.Stock;
using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetStockItems
{
    public class GetStockItemsQuery : IRequest<IReadOnlyList<StockItemSummaryDto>>
    {
        public int? PackTypeId { get; set; }
    }
}
