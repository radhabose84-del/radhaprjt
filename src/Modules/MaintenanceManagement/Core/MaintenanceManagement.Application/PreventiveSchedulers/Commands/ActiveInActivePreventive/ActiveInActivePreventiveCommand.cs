using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive
{
    public class ActiveInActivePreventiveCommand : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int Id { get; set; }
        public byte IsActive { get; set; }
    }
}