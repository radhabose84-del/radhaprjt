using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.PurchaseOrder.UploadPODocument
{
    public class UploadPODocumentCommand : IRequest<ApiResponseDTO<PODocumentDto>> 
    {
        public IFormFile? File { get; set; }  
    }
}