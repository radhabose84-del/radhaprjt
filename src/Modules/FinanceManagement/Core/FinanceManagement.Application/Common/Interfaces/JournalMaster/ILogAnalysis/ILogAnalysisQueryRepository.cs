using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.ILogAnalysis
{
    public interface ILogAnalysisQueryRepository
    {
        // Unified, normalized log feed across the four Journal log sources. logType optional filter:
        // SecurityViolation | SequenceGap | RecurringGeneration | JournalFlag.
        Task<(List<LogAnalysisDto>, int)> GetAllAsync(
            string? logType, DateTimeOffset? from, DateTimeOffset? to, int pageNumber, int pageSize);

        Task<LogAnalysisSummaryDto> GetSummaryAsync(DateTimeOffset? from, DateTimeOffset? to);
    }
}
