using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    /// <summary>
    /// A single row/column validation failure produced during a COA bulk-import run
    /// (GL02-FR-006, AC1/AC2). Retained alongside its <see cref="GlAccountImportLog"/>.
    /// </summary>
    public class GlAccountImportError : BaseEntity
    {
        public int ImportLogId { get; set; }

        public int RowNumber { get; set; }
        public string? RecordType { get; set; }     // GROUP | ACCOUNT (null for file-level errors)
        public string? ColumnName { get; set; }      // null = whole-row error
        public string? AttemptedValue { get; set; }
        public string ErrorMessage { get; set; } = null!;

        // Same-module FK navigation
        public GlAccountImportLog? ImportLog { get; set; }
    }
}
