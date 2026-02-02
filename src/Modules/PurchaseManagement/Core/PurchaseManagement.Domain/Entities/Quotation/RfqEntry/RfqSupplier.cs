using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ValueObjects;

namespace PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

public class RfqSupplier : IActivityTracked
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    public RfqMaster Rfq { get; set; } = default!;
    public int? SupplierId { get; set; }   
    public string? Name { get; set; } = default!;
    public EmailAddress? Email { get; set; } = default!;
    public string? Mobile { get; set; }
    public string? GSTNumber { get; set; }    
}
