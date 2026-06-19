using Contracts.Common;
using FinanceManagement.Application.CostCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetAllCostCentre
{
    public class GetAllCostCentreQuery : IRequest<ApiResponseDTO<List<CostCentreDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
