using Contracts.Common;
using MediatR;
using SalesManagement.Application.Production.Dto;

namespace SalesManagement.Application.Production.Queries.GetAllProduction
{
    public class GetAllProductionQuery : IRequest<ApiResponseDTO<List<ProductionPackHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
