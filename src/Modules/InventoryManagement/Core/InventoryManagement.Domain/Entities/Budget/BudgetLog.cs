
using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Budget
{
    public class BudgetLog : BaseEntity 
    {
        //public int Id { get; set; }
        public int BudgetDetailId { get; set; }
        public BudgetDetail BudgetDetail { get; set; } = null!;
        public int ActionTypeId { get; set; }
        public MiscMaster MiscAction { get; set; }      = null!;
        public decimal OldBudgetAmount { get; set; }
        public decimal NewBudgetAmount { get; set; }
        public string? Remarks { get; set; }
       /*  public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }      
        public string? CreatedByName { get; set; }      
        public string? CreatedIP { get; set; } */
    }
}