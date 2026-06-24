using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Queries.GetAllJournalFlag
{
    // US-GL01-16B — read the flags raised by the flagging engine (optionally for one journal).
    public class GetAllJournalFlagQuery : IRequest<ApiResponseDTO<List<JournalFlagDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? JournalHeaderId { get; set; }
    }
}
