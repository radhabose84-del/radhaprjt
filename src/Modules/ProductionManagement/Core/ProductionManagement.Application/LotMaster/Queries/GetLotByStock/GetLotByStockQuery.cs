using Contracts.Dtos.Stock;
using MediatR;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotByStock
{
    public class GetLotByStockQuery : IRequest<IReadOnlyList<StockLotByItemDto>>
    {
        public int ItemId { get; set; }
        public int? SourceUnitId { get; set; }
    }
}
