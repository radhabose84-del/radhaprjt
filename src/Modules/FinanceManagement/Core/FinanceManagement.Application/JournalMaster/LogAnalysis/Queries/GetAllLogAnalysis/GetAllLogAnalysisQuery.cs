using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetAllLogAnalysis
{
    // Unified, normalized log feed across SecurityViolation / SequenceGap / RecurringGeneration / JournalFlag.
    public class GetAllLogAnalysisQuery : IRequest<ApiResponseDTO<List<LogAnalysisDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? LogType { get; set; }            // optional filter
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
    }
}
