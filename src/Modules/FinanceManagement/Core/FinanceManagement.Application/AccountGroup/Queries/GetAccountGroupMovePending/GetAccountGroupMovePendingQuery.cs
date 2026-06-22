using Contracts.Common;
using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupMovePending
{
    // Approval-inbox: Move requests awaiting the logged-in approver (FC at L1, CFO at L2).
    public class GetAccountGroupMovePendingQuery : IRequest<ApiResponseDTO<List<AccountGroupMovePendingDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
