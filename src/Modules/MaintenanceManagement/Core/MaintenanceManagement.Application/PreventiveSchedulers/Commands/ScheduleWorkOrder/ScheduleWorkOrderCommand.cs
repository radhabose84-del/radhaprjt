using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder
{
    public class ScheduleWorkOrderCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
        public int PreventiveScheduleId { get; set; }
    }
}