namespace InventoryManagement.Application.Budget.Queries.GetAllBudgets
{
    public class BudgetListDto
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
        public List<BudgetDetailDto> Details { get; set; } = new();
    }

    public class BudgetDetailDto
    {
        public int DetailId { get; set; }
        public int Month { get; set; }
        public decimal BudgetAmount { get; set; }
    }
}
