using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine
{
    public class MapMachineCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int Id { get; set; }
        public int MachineId { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }
    }
}