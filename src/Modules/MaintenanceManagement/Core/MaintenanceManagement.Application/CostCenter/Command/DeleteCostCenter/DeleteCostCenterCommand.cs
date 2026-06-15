using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter
{
    public class DeleteCostCenterCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
