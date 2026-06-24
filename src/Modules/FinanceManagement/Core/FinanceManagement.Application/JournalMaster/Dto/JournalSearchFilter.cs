namespace FinanceManagement.Application.JournalMaster.Dto
{
    // US-GL01 Journal List & Search — advanced filter criteria. All fields are optional; only the supplied
    // ones are applied (AND-combined). Account / Cost Centre / Reference match at the line level.
    public sealed class JournalSearchFilter
    {
        public string? VoucherNo { get; set; }
        public DateOnly? DateFrom { get; set; }       // VoucherDate >=
        public DateOnly? DateTo { get; set; }         // VoucherDate <=
        public int? AccountId { get; set; }           // a line uses this GL account
        public int? CostCentreId { get; set; }        // a line uses this cost centre
        public decimal? AmountMin { get; set; }       // TotalDr >=
        public decimal? AmountMax { get; set; }       // TotalDr <=
        public int? VoucherTypeId { get; set; }
        public int? StatusId { get; set; }
        public int? CreatedBy { get; set; }           // Creator (user id)
        public string? ApprovedBy { get; set; }       // Approver name (LIKE)
        public int? SourceId { get; set; }            // Source module
        public string? Narration { get; set; }        // full-text (LIKE)
        public string? Reference { get; set; }        // header TriggerDocRef or a line ReferenceDocNo
    }
}
