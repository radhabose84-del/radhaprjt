using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup
{
    public class UpdateBudgetGroupCommand : IRequest<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int CostCenterId { get; set; }

        public int? ParentBudgetGroupId { get; set; }
        public int CurrencyId { get; set; }

        // Allocation rule from MiscMaster (By % / By Spindle / On Request)
        public int? AllocationRuleId { get; set; }
        public decimal? AllocatedPercentage { get; set; }
        public decimal? AllocatedSpindleCost { get; set; }
        public int BudgetTypeId { get; set; }
        public bool CarryForward { get; set; } = false;
        public bool IsParent { get; set; }
        public bool IsActive { get; set; }
    }
}
