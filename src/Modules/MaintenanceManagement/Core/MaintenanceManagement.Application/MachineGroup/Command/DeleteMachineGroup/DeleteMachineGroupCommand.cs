using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup
{
    public class DeleteMachineGroupCommand : IRequest<bool>, IRequirePermission
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
