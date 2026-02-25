using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument
{
    public class GRNQcImageDto : IMapFrom<GrnHeader>
    {
        
        public string? ImagePath { get; set; }
        public string? GrnQcEntryDocumentBase64 { get; set; }
    }
}