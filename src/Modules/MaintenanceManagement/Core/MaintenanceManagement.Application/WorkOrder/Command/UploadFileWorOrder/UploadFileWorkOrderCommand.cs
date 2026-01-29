using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder
{
    public class UploadFileWorkOrderCommand : IRequest<ApiResponseDTO<WorkOrderImageDto>>
    {
         public IFormFile? File { get; set; }       
    }
}