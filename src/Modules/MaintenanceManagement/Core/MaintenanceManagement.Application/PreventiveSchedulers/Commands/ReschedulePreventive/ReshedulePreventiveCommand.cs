using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive
{
    public class ReshedulePreventiveCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
        public int PreventiveScheduleDetailId { get; set; }
        public DateOnly RescheduleDate { get; set; }
    }
}