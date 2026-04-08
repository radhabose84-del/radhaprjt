using Contracts.Common;
using MediatR;
using ProductionManagement.Application.ProductionPack.Dto;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionStockRegister
{
    public class GetProductionStockRegisterQuery : IRequest<ApiResponseDTO<List<ProductionStockRegisterDto>>>
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public int? LotId { get; set; }
        public int? ItemId { get; set; }
    }
}
