using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class GRNReceivedImageDto : IMapFrom<GrnHeader>
    {
        
        public string? ImagePath { get; set; }
        public string? GrnEntryDocumentBase64 { get; set; }
    }
}