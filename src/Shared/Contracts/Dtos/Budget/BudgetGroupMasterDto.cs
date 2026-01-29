namespace Contracts.Dtos.Budget
{
    public class BudgetGroupMasterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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
        public int CurrencyName { get; set; }

        public int? AllocationRuleId { get; set; }
        public string? AllocationRuleName { get; set; }

        public decimal? AllocatedPercentage { get; set; }
        public decimal? AllocatedSpindleCost { get; set; }

        public bool IsParent { get; set; }
        public bool IsActive { get; set; }
    }
}
