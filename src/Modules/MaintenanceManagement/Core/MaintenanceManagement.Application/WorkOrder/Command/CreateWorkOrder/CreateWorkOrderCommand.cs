
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
{
    public class CreateWorkOrderCommand : IRequest<ApiResponseDTO<WorkOrderCombineDto>>  
    {
       public WorkOrderCombineDto? WorkOrderDto { get; set; }       
    }
}