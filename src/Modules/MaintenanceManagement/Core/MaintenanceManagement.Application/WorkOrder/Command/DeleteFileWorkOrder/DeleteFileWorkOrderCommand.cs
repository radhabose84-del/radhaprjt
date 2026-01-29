using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder
{
    public class DeleteFileWorkOrderCommand : IRequest<ApiResponseDTO<bool>>
    {
        public string? Image { get; set; }        
    }
}