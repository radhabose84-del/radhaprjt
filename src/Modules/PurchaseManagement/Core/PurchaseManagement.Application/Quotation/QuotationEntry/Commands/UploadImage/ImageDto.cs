
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;

namespace PurchaseManagement.Application.Quotation.QuotationEntry.Commands.UploadItemImage
{
    public class ImageDto : IMapFrom<QuotationHeader>
    {
        public string? Image { get; set; }
        public string? ImageBase64 { get; set; } 

    }
}