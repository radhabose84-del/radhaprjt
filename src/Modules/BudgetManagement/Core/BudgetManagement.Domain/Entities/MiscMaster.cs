using BudgetManagement.Domain.Common;

namespace BudgetManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
           public MiscTypeMaster? MiscTypeMaster { get; set; }
        public ICollection<BudgetRequest>? BudgetRequests { get; set; }
        public ICollection<BudgetGroup>? BudgetAllocationRules { get; set; }
        public ICollection<BudgetAllocation>? BudgetAllocationType { get; set; }
        public ICollection<BudgetGroup>? BudgetTypeGroups { get; set; }
        public ICollection<BudgetAllocation>? BudgetRequestByType { get; set; }  
        public ICollection<BudgetAllocation>? BudgetRequestMonthType { get; set; }  
   
        public ICollection<BudgetRequest>? BudgetRequestBy { get; set; }   
        public ICollection<BudgetRequest>? BudgetRequestMonth { get; set; }  
    }
}
        
    
