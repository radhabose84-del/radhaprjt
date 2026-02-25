using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class UploadGrnEntryDocumentCommand : IRequest<GRNReceivedImageDto>
    {
         public IFormFile? File { get; set; }  
    }
}