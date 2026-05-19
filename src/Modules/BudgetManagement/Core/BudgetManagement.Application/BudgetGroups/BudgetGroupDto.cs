namespace BudgetManagement.Application.BudgetGroups
{
    public class BudgetGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public int UnitId { get; set; }
        public string? UnitName { get; set; }    

        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; } 

        public int CostCenterId { get; set; }
        public string? CostCenterName { get; set; }


        public int? ParentBudgetGroupId { get; set; }
        public string? ParentBudgetGroupName { get; set; }  

        public int CurrencyId { get; set; }  

        public int? AllocationRuleId { get; set; }
        public string? AllocationRuleName { get; set; } // From MiscMaster

        public decimal? AllocatedPercentage { get; set; }
        public decimal? AllocatedSpindleCost { get; set; }

        public int? BudgetTypeId { get; set; }
        public string? BudgetTypeName { get; set; }   // from MiscMaster.Code/Description
        public bool CarryForward { get; set; }
        public bool EmergencyPoApplicable { get; set; }
        public bool IsParent { get; set; }
        public bool IsActive { get; set; }
    }

    public class BudgetGroupListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; } = null!;
        public int? ParentBudgetGroupId { get; set; }
        public string? ParentBudgetGroupName { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public int? AllocationRuleId { get; set; }
        public string? AllocationRuleName { get; set; }
        public decimal? AllocatedPercentage { get; set; }
        public decimal? AllocatedSpindleCost { get; set; }
        public int? BudgetTypeId { get; set; }
        public string? BudgetTypeName { get; set; }
        public bool CarryForward { get; set; }
        public bool EmergencyPoApplicable { get; set; }
        public bool IsParent { get; set; }
        public bool IsActive { get; set; }
    }

    public class BudgetGroupAutoCompleteDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentBudgetGroupId { get; set; }
        public string? ParentBudgetGroupName { get; set; }
        public string? BudgetTypeName { get; set; }
        public bool IsParent { get; set; }
        public int? AllocationRuleId { get; set; }
        public string? AllocationRuleName { get; set; }
        public int? BudgetTypeId { get; set; }
        public bool CarryForward { get; set; }
        public bool EmergencyPoApplicable { get; set; }
    }

    public class BudgetGroupListFilterDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public int? CostCenterId { get; set; }
        public int? ParentBudgetGroupId { get; set; }
        public bool? IsActive { get; set; }
        public int? AllocationRuleId { get; set; }
        public int? BudgetTypeId { get; set; }        // optional filter
        public bool? CarryForward { get; set; }       // optional filter
        public bool? EmergencyPoApplicable { get; set; } // optional filter

    }


}