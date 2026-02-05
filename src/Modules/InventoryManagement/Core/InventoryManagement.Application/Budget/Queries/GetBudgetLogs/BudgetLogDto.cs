namespace InventoryManagement.Application.Budget.Queries.GetBudgetLogs
{
    public class BudgetLogDto
    {
        public int Id { get; set; }
        public int BudgetDetailId { get; set; }
        public int ActionTypeId { get; set; }
        public decimal OldBudgetAmount { get; set; }
        public decimal NewBudgetAmount { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedIP { get; set; }
        public int Month { get; set; }
        public string? BudgetGroupName { get; set; }
        public string? ItemGroupName { get; set; }
        public int FiscalYear { get; set; }   
        public string? ActionName { get; set; }     
    }
}
