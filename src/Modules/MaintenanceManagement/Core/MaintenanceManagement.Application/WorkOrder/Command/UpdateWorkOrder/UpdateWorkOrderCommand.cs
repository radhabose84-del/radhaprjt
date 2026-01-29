using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class UpdateWorkOrderCommand : IRequest<ApiResponseDTO<bool>>     
    {
        public WorkOrderUpdateDto? WorkOrder { get; set; }    
        
    }
}