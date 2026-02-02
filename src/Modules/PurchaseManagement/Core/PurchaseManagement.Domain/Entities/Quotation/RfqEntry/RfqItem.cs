
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
public class RfqItem : IActivityTracked
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    public RfqMaster Rfq { get; set; } = default!;
    public int ItemId { get; set; }             
    public int HsnId { get; set; }   
    public decimal Quantity { get; set; }
    public int UomId { get; set; } = default!;    
}
