

using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule
{
    public class UpdateWOScheduleCommand : IRequest<ApiResponseDTO<bool>>     
    {
        public WorkOrderScheduleUpdateDto? WOSchedule { get; set; }    
    }
}