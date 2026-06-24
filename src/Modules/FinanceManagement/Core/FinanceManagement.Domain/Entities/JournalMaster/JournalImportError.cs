namespace FinanceManagement.Domain.Entities
{
    // US-GL01-17 — row-level validation errors for an import batch. Lean child log (NOT a BaseEntity).
    public class JournalImportError
    {
        public int Id { get; set; }

        public int ImportBatchId { get; set; }          // same-module FK -> JournalImportBatch
        public int RowNo { get; set; }
        public string? ColumnName { get; set; }         // NULL = row-level error
        public string? Message { get; set; }

        // Same-module FK navigation
        public JournalImportBatch? ImportBatch { get; set; }
    }
}
