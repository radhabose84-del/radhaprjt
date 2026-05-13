using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

public class RfqAttachment : BaseEntity
{
    public int RfqId { get; set; }
    public RfqMaster? Rfq { get; set; }

    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long FileSize { get; set; }
}
