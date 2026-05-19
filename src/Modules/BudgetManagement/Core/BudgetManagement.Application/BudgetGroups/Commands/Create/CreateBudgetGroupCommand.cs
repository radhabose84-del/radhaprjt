using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup
{
    public class CreateBudgetGroupCommand : IRequest<int>
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int CostCenterId { get; set; }
        public int? ParentBudgetGroupId { get; set; }
        public int CurrencyId { get; set; }
        public int? AllocationRuleId { get; set; }
        public decimal? AllocatedPercentage { get; set; }
        public decimal? AllocatedSpindleCost { get; set; }
        public int BudgetTypeId { get; set; }          
        public bool CarryForward { get; set; } = false;
        public bool EmergencyPoApplicable { get; set; } = false;
        public bool IsParent { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
