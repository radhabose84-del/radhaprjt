namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Lightweight autocomplete row for journal vouchers (by-name picker, optional status filter).
    public sealed class JournalLookupDto
    {
        public int Id { get; set; }
        public string? VoucherNo { get; set; }
        public DateOnly VoucherDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public decimal TotalDr { get; set; }

        // True when a reversal entry can be created for this voucher: POSTED, not already reversed,
        // and not itself a reversal (mirrors the ReverseJournal eligibility rules).
        public bool IsReverseApplicable { get; set; }
    }
}
