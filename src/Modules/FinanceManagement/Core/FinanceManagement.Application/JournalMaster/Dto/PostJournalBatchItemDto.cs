namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Per-voucher outcome of a best-effort bulk post (US-GL01-09).
    public sealed class PostJournalBatchItemDto
    {
        public int JournalId { get; set; }
        public bool IsSuccess { get; set; }
        public string? VoucherNo { get; set; }   // assigned number when posted
        public string? Message { get; set; }     // success message or the failure reason
    }
}
