

using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule
{
    public class UpdateWOScheduleCommand : IRequest<ApiResponseDTO<bool>>     
    {
        public WorkOrderScheduleUpdateDto? WOSchedule { get; set; }    
    }
}