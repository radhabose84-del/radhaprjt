using Contracts.Common;
using MediatR;
using SalesManagement.Application.ProductionPack.Dto;

namespace SalesManagement.Application.ProductionPack.Queries.GetAllProduction
{
    public class GetAllProductionQuery : IRequest<ApiResponseDTO<List<ProductionPackHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
