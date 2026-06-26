using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport
{
    /// <summary>
    /// US-GL03-04 / AC#3 — paginated list of every backdated journal entry (IsBackdated = 1) for
    /// the session company, optionally narrowed by accounting period and date range.
    /// </summary>
    public class GetLatePostingReportQuery : IRequest<ApiResponseDTO<List<LatePostingReportDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int? AccountingPeriodId { get; set; }

        public DateOnly? FromDate { get; set; }       // matched against PostedAt (date portion)
        public DateOnly? ToDate { get; set; }

        public string? SortBy { get; set; }            // 'PostedAt' (default) | 'VoucherDate'
        public string? SortDirection { get; set; }     // 'DESC' (default) | 'ASC'
    }
}
