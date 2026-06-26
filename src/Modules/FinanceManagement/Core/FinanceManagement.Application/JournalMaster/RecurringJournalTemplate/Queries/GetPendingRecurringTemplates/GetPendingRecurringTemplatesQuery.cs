using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetPendingRecurringTemplates
{
    // US-GL01-11 — templates awaiting the CURRENT user's approval (Pending), filtered by the workflow approver list.
    public class GetPendingRecurringTemplatesQuery : IRequest<ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
