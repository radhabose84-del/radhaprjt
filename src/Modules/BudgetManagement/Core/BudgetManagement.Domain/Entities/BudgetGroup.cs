    using BudgetManagement.Domain.Common;


    namespace BudgetManagement.Domain.Entities
    {
        public class BudgetGroup : BaseEntity
        {
            public string? Name { get; set; }
            public string? Description { get; set; }

            public int UnitId { get; set; }
            public int DepartmentId { get; set; }
            public int CostCenterId { get; set; }

            public int? ParentBudgetGroupId { get; set; }
            public BudgetGroup? ParentBudgetGroup { get; set; }

            public int CurrencyId { get; set; }

            public int? AllocationRuleId { get; set; }
            public MiscMaster? AllocationRule { get; set; }
            public decimal? AllocatedPercentage { get; set; }
            public decimal? AllocatedSpindleCost { get; set; }
            public int? BudgetTypeId { get; set; }
            public MiscMaster? BudgetType { get; set; }
            public bool CarryForward { get; set; }
            public bool IsParent { get; set; }
            public ICollection<BudgetAllocation>? BudgetAllocationGroupType { get; set; }   
            public ICollection<BudgetRequest>? BudgetRequestGroupType { get; set; }   
        }
    }