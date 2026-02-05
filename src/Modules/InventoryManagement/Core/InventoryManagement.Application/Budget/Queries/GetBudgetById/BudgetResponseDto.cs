namespace InventoryManagement.Application.Budget.Queries.GetBudgetById
{
    public class BudgetResponseDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int BudgetGroupId { get; set; }
        public string? BudgetGroupName { get; set; }
        public int FiscalYear { get; set; }
        public decimal YearBudgetAmount { get; set; }
        public byte? Is_MRApplicable { get; set; }
        public byte? Is_POApplicable { get; set; }
        public byte? Is_ServiceApplicable { get; set; }
        public List<BudgetDetailWithLogsDto> Details { get; set; } = new();
    }

    public class BudgetDetailWithLogsDto
    {
        public int DetailId { get; set; }
        public int Month { get; set; }
        public string? MonthName { get; set; }
        public decimal BudgetAmount { get; set; }
        //public List<BudgetLogDto> Logs { get; set; } = new();
    }

  /*   public class BudgetLogDto
    {
        public int Id { get; set; }
        public int ActionTypeId { get; set; }
        public decimal OldBudgetAmount { get; set; }
        public decimal NewBudgetAmount { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    } */
}
