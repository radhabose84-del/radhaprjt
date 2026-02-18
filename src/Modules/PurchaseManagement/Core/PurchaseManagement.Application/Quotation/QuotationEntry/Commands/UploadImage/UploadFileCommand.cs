using Contracts.Common;
using PurchaseManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.Item.ItemDetail.Commands.UploadItemImage
{
    public class UploadFileCommand : IRequest<ApiResponseDTO<ImageDto>> 
    {
        public IFormFile? File { get; set; }
    }
}
