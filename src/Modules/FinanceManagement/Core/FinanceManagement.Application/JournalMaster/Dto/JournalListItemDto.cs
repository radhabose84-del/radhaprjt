namespace FinanceManagement.Application.JournalMaster.Dto
{
    // One row of the Journal List & Search grid. Flattens the header + its PRIMARY line (first debit line)
    // so the grid can show Account / Cost Centre / Reference without loading every line.
    public sealed class JournalListItemDto
    {
        public int Id { get; set; }
        public string? VoucherNo { get; set; }
        public DateOnly VoucherDate { get; set; }
        public string? VoucherTypeCode { get; set; }

        // Primary line (main debit account).
        public int? GlAccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }

        public decimal TotalDr { get; set; }   // Amount column
        public decimal TotalCr { get; set; }
        public string? Narration { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }   // Status
        public int SourceId { get; set; }
        public string? SourceName { get; set; }   // Source

        public int CreatedBy { get; set; }
        public string? CreatorName { get; set; }       // Creator (audit CreatedByName)
        public string? ApproverName { get; set; }      // Approver name (JournalHeader.ApprovedBy)
        public DateTimeOffset? ApprovedAt { get; set; } // Approver date

        public string? Reference { get; set; }    // primary line ReferenceDocNo, else header TriggerDocRef
    }
}
