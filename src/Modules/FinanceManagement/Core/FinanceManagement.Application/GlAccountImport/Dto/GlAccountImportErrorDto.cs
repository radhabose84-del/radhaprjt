namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>Row-level validation failure (row #, column, message) — AC1/AC2.</summary>
    public sealed class GlAccountImportErrorDto
    {
        public int RowNumber { get; set; }
        public string? RecordType { get; set; }
        public string? ColumnName { get; set; }
        public string? AttemptedValue { get; set; }
        public string ErrorMessage { get; set; } = null!;
    }
}
