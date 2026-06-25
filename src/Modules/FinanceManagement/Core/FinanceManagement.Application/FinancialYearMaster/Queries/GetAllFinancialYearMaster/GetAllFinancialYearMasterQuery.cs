using Contracts.Common;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetAllFinancialYearMaster
{
    public class GetAllFinancialYearMasterQuery : IRequest<ApiResponseDTO<List<FinancialYearMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }
    }
}
