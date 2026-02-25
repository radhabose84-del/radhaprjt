namespace BudgetManagement.Application.BudgetAllocation.Command.Create
{
    public class CreateBudgetAllocationDto
    {
        public int FinancialYearId { get; set; }
        public int RequestById { get; set; }
        public int RequestMonthId { get; set; }
        public int UnitId { get; set; }
        public int? RequestId { get; set; }
        public int BudgetGroupId { get; set; }
        public int? BudgetSubGroupId { get; set; }
        public int AllocationTypeId { get; set; }
        public int? SpindleCount { get; set; }
        public decimal? RatePerSpindle { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string? Remarks { get; set; }
        public decimal? RemainingBalance { get; set; } 
    }
}