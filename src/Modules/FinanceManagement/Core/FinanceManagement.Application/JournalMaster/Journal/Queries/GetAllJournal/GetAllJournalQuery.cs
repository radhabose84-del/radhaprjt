using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetAllJournal
{
    public class GetAllJournalQuery : IRequest<ApiResponseDTO<List<JournalHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }   // optional JOURNAL_STATUS filter
    }
}
