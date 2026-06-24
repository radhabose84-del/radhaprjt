namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Line payload for Create/Update journal commands (US-GL01-01 / 05).
    public class JournalLineInputDto
    {
        public int GlAccountId { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public int CurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }
        public int? CostCentreId { get; set; }
        public int? ProfitCentreId { get; set; }
        public string? LineNarration { get; set; }
        public string? ReferenceDocNo { get; set; }
    }
}
