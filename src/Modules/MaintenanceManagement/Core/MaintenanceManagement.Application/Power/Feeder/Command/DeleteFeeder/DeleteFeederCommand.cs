using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder
{
    public class DeleteFeederCommand : IRequest<bool>, IRequirePermission
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
