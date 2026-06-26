using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetLogAnalysisSummary
{
    // Per-source counts for the log-analysis header cards.
    public sealed record GetLogAnalysisSummaryQuery(DateTimeOffset? DateFrom = null, DateTimeOffset? DateTo = null)
        : IRequest<LogAnalysisSummaryDto>;
}
