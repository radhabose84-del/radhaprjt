namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Normalized row from the four Journal log sources — SecurityViolation, SequenceGap,
    // RecurringGeneration, JournalFlag — for a unified "log analysis" feed.
    public sealed class LogAnalysisDto
    {
        public string? LogType { get; set; }          // SecurityViolation | SequenceGap | RecurringGeneration | JournalFlag
        public int Id { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public string? Reference { get; set; }         // voucher no / series / template
        public string? Summary { get; set; }           // human-readable event
        public string? Detail { get; set; }            // extra context
        public bool Flag { get; set; }                 // alerted / auto-posted / digest-sent / violation
    }

    // Per-source counts for the log-analysis header cards.
    public sealed class LogAnalysisSummaryDto
    {
        public int SecurityViolationCount { get; set; }
        public int SequenceGapCount { get; set; }
        public int RecurringGenerationCount { get; set; }
        public int JournalFlagCount { get; set; }
        public int TotalCount { get; set; }
    }
}
