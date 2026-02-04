using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Budget
{
    public class BudgetMaster : BaseEntity
    {
        public int UnitId { get; set; }
        //public int BudgetAgainstId { get; set; }
        public int BudgetGroupId { get; set; }
        public int FiscalYear { get; set; }
        public decimal YearBudgetAmount { get; set; }
        public byte? Is_MRApplicable { get; set; }
        public byte? Is_POApplicable { get; set; }
        public byte? Is_ServiceApplicable { get; set; }
        public ICollection<BudgetDetail>? BudgetDetail { get; set; } 
    }
}


