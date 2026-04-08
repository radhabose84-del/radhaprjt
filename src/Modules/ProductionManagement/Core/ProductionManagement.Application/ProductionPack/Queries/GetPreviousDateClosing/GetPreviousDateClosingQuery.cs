using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Queries.GetPreviousDateClosing
{
    public class GetPreviousDateClosingQuery : IRequest<ProductionStockClosingDto?>
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public DateOnly DocDate { get; set; }
    }
}
