namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>Import-log header row for the audit/history list (GET logs).</summary>
    public sealed class GlAccountImportLogDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string FileName { get; set; } = null!;
        public string FileFormat { get; set; } = null!;
        public string ImportMode { get; set; } = null!;
        public string Status { get; set; } = null!;

        public int TotalRows { get; set; }
        public int GroupRows { get; set; }
        public int AccountRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int ImportedGroups { get; set; }
        public int ImportedAccounts { get; set; }
        public int SkippedRows { get; set; }
        public int DurationMs { get; set; }

        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
