using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetPostableJournals
{
    // US-GL01-06B — vouchers ready to post: APPROVED, or a system journal (source != MANUAL) still in DRAFT.
    public class GetPostableJournalsQuery : IRequest<ApiResponseDTO<List<JournalListItemDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
