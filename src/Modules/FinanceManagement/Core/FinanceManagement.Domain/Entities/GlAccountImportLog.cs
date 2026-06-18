using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    /// <summary>
    /// One row per COA bulk-import run (GL02-FR-006). Retained for at least one year for
    /// auditor review (AC4). The row/column/message detail lives in <see cref="GlAccountImportError"/>.
    /// </summary>
    public class GlAccountImportLog : BaseEntity
    {
        public int CompanyId { get; set; }

        public string FileName { get; set; } = null!;
        public string FileFormat { get; set; } = null!;   // Excel | Csv
        public string ImportMode { get; set; } = null!;   // ValidRowsOnly | AllOrNothing

        // Counters (AC1 / AC2 / AC4)
        public int TotalRows { get; set; }
        public int GroupRows { get; set; }
        public int AccountRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int ImportedGroups { get; set; }
        public int ImportedAccounts { get; set; }
        public int SkippedRows { get; set; }

        // Validated (errors only, nothing committed) | Completed | CompletedWithSkips | Failed
        public string Status { get; set; } = null!;

        // Wall-clock of the import — evidences the < 60s requirement (AC4).
        public int DurationMs { get; set; }

        // Row-level error report (AC1 / AC2).
        public ICollection<GlAccountImportError>? Errors { get; set; }
    }
}
