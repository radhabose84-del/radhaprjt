

using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule
{
    public class CreateWOScheduleCommand : IRequest<ApiResponseDTO<bool>>     
    {
        public WorkOrderScheduleUpdateDto? WOSchedule { get; set; }    
    }
}