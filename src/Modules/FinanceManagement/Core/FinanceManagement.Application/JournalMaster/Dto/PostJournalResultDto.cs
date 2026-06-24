namespace FinanceManagement.Application.JournalMaster.Dto
{
    // US-GL01-09 — posting confirmation: voucher number + the balances that were updated.
    public class PostJournalResultDto
    {
        public int JournalId { get; set; }
        public string? VoucherNo { get; set; }
        public List<PostedBalanceDto> UpdatedBalances { get; set; } = new();
    }

    public class PostedBalanceDto
    {
        public int GlAccountId { get; set; }
        public int? CostCentreId { get; set; }
        public decimal Balance { get; set; }
    }
}
