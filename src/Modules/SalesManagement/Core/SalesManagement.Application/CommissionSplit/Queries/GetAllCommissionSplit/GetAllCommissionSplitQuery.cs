using Contracts.Common;
using MediatR;
using SalesManagement.Application.CommissionSplit.Dto;

namespace SalesManagement.Application.CommissionSplit.Queries.GetAllCommissionSplit
{
    public class GetAllCommissionSplitQuery : IRequest<ApiResponseDTO<List<CommissionSplitDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
