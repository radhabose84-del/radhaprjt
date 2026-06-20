using Contracts.Common;
using FinanceManagement.Application.ProfitCentre.Dto;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetAllProfitCentre
{
    public class GetAllProfitCentreQuery : IRequest<ApiResponseDTO<List<ProfitCentreDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
