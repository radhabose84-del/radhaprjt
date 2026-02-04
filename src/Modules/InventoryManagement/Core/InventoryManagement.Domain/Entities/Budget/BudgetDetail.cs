using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Budget
{
    public class BudgetDetail : BaseEntity
    {
        public int BudgetId { get; set; }
        public BudgetMaster BudgetMaster { get; set; } = null!;
        public int Month { get; set; }
        public decimal BudgetAmount { get; set; }      
        public ICollection<BudgetLog>? BudgetLog { get; set; }              
    }
}
