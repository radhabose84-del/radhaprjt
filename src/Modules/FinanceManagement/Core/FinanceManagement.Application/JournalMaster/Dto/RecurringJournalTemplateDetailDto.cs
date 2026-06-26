namespace FinanceManagement.Application.JournalMaster.Dto
{
    public class RecurringJournalTemplateDetailDto
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int LineNo { get; set; }
        public int GlAccountId { get; set; }
        public string? GlAccountCode { get; set; }
        public string? GlAccountName { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public string? AmountFormula { get; set; }
        public int CurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }
        public int? CostCentreId { get; set; }
        public string? CostCentreName { get; set; }
        public int? ProfitCentreId { get; set; }
        public string? ProfitCentreName { get; set; }
        public string? LineNarration { get; set; }
    }
}
