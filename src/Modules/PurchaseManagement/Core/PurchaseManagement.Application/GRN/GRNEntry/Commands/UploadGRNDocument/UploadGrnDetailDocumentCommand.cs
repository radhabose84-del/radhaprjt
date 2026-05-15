using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class UploadGrnDetailDocumentCommand : IRequest<GRNDetailImageDto>
    {
         public IFormFile? File { get; set; }
    }
}
