using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.UploadGateEntryDocument
{
    public class GateEntryDocumentDto  : IMapFrom<GateEntryHeader>
    {
        public string? ImagePath { get; set; }
        public string? GateEntryDocumentBase64 { get; set; }
    }
}