using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter
{
    public class CreateCostCenterCommand :IRequest<int>, IRequirePermission
    {
        public string? CostCenterCode { get; set; }
        public string? CostCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public decimal? BudgetAllocated { get; set; }
        public string? Remarks { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
