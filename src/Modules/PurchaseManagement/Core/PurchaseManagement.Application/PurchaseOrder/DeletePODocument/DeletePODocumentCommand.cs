using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.DeletePODocument
{
    public class DeletePODocumentCommand :IRequest<ApiResponseDTO<bool>>
    {       
        public string? PODocumentPath { get; set; }        
        public int Id { get; set; }
        public int POId { get; set; }
        public string? FileName { get; set; }  
   }
}