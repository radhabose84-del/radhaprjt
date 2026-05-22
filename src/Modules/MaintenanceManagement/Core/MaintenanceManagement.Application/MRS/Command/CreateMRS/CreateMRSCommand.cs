using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MRS.Command.CreateMRS
{
    public class CreateMRSCommand :IRequest<int>, IRequirePermission
    {
         public HeaderRequest? Header { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
