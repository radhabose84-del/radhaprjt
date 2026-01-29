
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder.Item
{
    public class DeleteFileItemCommand : IRequest<ApiResponseDTO<bool>>
    {
        public string? Image { get; set; }        
    }
}