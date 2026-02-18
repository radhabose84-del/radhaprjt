

using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item
{
    public class UploadFileItemCommand : IRequest<ApiResponseDTO<ItemImageDto>>
    {
         public IFormFile? File { get; set; }       
    }
}