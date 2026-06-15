using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter
{
    public class UpdateCostCenterCommand :IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? CostCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public decimal? BudgetAllocated { get; set; }
        public string? Remarks { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
