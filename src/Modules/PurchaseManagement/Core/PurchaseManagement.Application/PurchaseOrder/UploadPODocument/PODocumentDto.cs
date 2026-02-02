using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Domain.PurchaseOrder;
namespace PurchaseManagement.Application.PurchaseOrder.UploadPODocument
{
    public class PODocumentDto  : IMapFrom<PurchaseDocument>
    {
        public string? FileName { get; set; }
        public string? PODocumentBase64 { get; set; }
    }
}