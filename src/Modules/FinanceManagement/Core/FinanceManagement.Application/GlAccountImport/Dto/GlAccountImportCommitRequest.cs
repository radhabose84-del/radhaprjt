namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>
    /// Everything the command repository needs to persist one import run in a single transaction:
    /// the validated plan, the row-error report, and the log header metadata.
    /// </summary>
    public sealed class GlAccountImportCommitRequest
    {
        public int CompanyId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileFormat { get; set; } = null!;
        public string ImportMode { get; set; } = null!;

        public int TotalRows { get; set; }
        public int GroupRows { get; set; }
        public int AccountRows { get; set; }
        public int DurationMs { get; set; }

        /// <summary>Final run status (CompletedWithSkips when valid rows were imported but some skipped).</summary>
        public string Status { get; set; } = null!;

        /// <summary>New groups to create, ordered parent-before-child. Empty when nothing committed.</summary>
        public List<PlannedAccountGroup> Groups { get; set; } = new();

        /// <summary>Accounts to create (all set Inactive by the repository — AC3).</summary>
        public List<PlannedGlAccount> Accounts { get; set; } = new();

        /// <summary>Row-error report to retain alongside the log (AC1/AC2).</summary>
        public List<GlAccountImportErrorDto> Errors { get; set; } = new();
    }
}
