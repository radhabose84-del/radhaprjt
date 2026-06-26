namespace FinanceManagement.Application.JournalMaster.Dto
{
    // One parsed row from the import template (US-GL01-17). Rows with the same GroupNo form one voucher.
    public class JournalImportRowInputDto
    {
        public int RowNo { get; set; }
        public int GroupNo { get; set; }
        public int VoucherTypeId { get; set; }
        public DateOnly VoucherDate { get; set; }
        public string? Narration { get; set; }
        public int GlAccountId { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public int CurrencyId { get; set; }
        public int? CostCentreId { get; set; }
        public int? ProfitCentreId { get; set; }
        public string? LineNarration { get; set; }
        public string? ReferenceDocNo { get; set; }
    }

    public class JournalImportErrorDto
    {
        public int RowNo { get; set; }
        public string? ColumnName { get; set; }
        public string? Message { get; set; }
    }

    public class JournalImportBatchDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int ErrorRows { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int ImportedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public List<JournalImportErrorDto> Errors { get; set; } = new();
        public List<JournalImportJournalDto> Journals { get; set; } = new();   // journals created by this batch (header + lines)
    }

    // A journal voucher created by an import batch — header + its line items.
    public class JournalImportJournalDto
    {
        public int Id { get; set; }
        public string? VoucherNo { get; set; }          // null until the draft is posted
        public DateOnly VoucherDate { get; set; }
        public string? Narration { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public decimal TotalDr { get; set; }
        public decimal TotalCr { get; set; }
        public bool IsPosted { get; set; }
        public List<JournalImportJournalLineDto> Lines { get; set; } = new();
    }

    public class JournalImportJournalLineDto
    {
        public int JournalHeaderId { get; set; }
        public int LineNo { get; set; }
        public int GlAccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public int CurrencyId { get; set; }
        public int? CostCentreId { get; set; }
        public int? ProfitCentreId { get; set; }
        public string? LineNarration { get; set; }
        public string? ReferenceDocNo { get; set; }
    }

    // Result of an import run (US-17 AC-1/AC-3).
    public class ImportJournalsResultDto
    {
        public int BatchId { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int ErrorRows { get; set; }
        public string? Status { get; set; }                 // COMMITTED / FAILED
        public bool Committed { get; set; }
        public List<JournalImportErrorDto> Errors { get; set; } = new();
        public List<int> CreatedJournalIds { get; set; } = new();
    }
}
