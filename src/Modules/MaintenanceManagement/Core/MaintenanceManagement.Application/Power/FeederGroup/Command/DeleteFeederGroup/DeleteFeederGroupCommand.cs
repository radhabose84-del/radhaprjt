using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup
{
    public class DeleteFeederGroupCommand  : IRequest<bool>, IRequirePermission
    {
        
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
