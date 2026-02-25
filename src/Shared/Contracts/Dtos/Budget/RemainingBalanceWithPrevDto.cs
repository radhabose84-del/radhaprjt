namespace Contracts.Dtos.Budget
{
    public class RemainingBalanceWithPrevDto
    {
        public int Id { get; set; }
        public int? PreviousId { get; set; }

        public int BudgetGroupId { get; set; }
        public DateOnly? BudgetDate { get; set; }
        public int? MonthId { get; set; }
        public int? RequestById { get; set; }

        public int? ProjectId { get; set; }
        public int? WbsId { get; set; }
        public int? FinancialYearId { get; set; }

        public decimal? CurrentRemainingBalance { get; set; }
        public decimal? PreviousRemainingBalance { get; set; }
    }
}
