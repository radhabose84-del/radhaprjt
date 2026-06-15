using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate
{
    public class MachineWiseFrequencyUpdateCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int Id { get; set; }
        public int FrequencyInterval { get; set; }
        public byte IsActive { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }
    }
}