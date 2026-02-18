using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationEntry.Commands.DeleteImage
{
    public class DeleteFileCommand : IRequest<ApiResponseDTO<bool>>
    {  
        public string? imagePath { get; set; }       
    }
}