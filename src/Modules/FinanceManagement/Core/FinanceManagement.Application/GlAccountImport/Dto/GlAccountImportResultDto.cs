namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>Outcome of an import run, returned to the caller and mirrored in the import log.</summary>
    public sealed class GlAccountImportResultDto
    {
        public int ImportLogId { get; set; }
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

        public List<GlAccountImportErrorDto> Errors { get; set; } = new();
    }
}
