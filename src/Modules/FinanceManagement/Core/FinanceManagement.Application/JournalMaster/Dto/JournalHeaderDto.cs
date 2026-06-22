namespace FinanceManagement.Application.JournalMaster.Dto
{
    public class JournalHeaderDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public int VoucherTypeId { get; set; }
        public string? VoucherTypeCode { get; set; }
        public string? VoucherTypeName { get; set; }
        public string? VoucherNo { get; set; }
        public DateOnly VoucherDate { get; set; }
        public DateOnly? PostingDate { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }
        public int? AccountingPeriodId { get; set; }
        public string? PeriodName { get; set; }
        public string? Narration { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }             // MiscMaster (JOURNAL_STATUS)
        public int SourceId { get; set; }
        public string? SourceName { get; set; }             // MiscMaster (JOURNAL_SOURCE)
        public string? TriggerDocType { get; set; }
        public string? TriggerDocRef { get; set; }
        public bool AutoApproved { get; set; }
        public decimal TotalDr { get; set; }
        public decimal TotalCr { get; set; }
        public int? ReversalOfId { get; set; }
        public string? ReversalOfVoucherNo { get; set; }
        public bool IsReversal { get; set; }
        public string? CopiedFromRef { get; set; }
        public int? ImportBatchId { get; set; }

        public int? SubmittedBy { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public int? RejectedBy { get; set; }
        public DateTimeOffset? RejectedAt { get; set; }
        public string? RejectReason { get; set; }
        public int? PostedBy { get; set; }
        public DateTimeOffset? PostedAt { get; set; }

        public List<JournalDetailDto> Lines { get; set; } = new();

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
