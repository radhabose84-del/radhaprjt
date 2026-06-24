namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Line payload for Create/Update recurring-template commands (US-GL01-11A).
    public class RecurringTemplateLineInputDto
    {
        public int GlAccountId { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? CrAmount { get; set; }
        public string? AmountFormula { get; set; }
        public int? CostCentreId { get; set; }
        public int? ProfitCentreId { get; set; }
        public string? LineNarration { get; set; }
    }
}
