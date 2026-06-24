using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetPendingApprovalJournals
{
    // US-GL01-06B — manual DRAFT vouchers awaiting the CURRENT user's approval.
    public class GetPendingApprovalJournalsQuery : IRequest<ApiResponseDTO<List<JournalListItemDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
