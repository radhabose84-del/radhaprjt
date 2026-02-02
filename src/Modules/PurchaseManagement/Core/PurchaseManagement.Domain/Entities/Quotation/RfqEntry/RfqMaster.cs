
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;


namespace PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

public class RfqMaster : BaseEntity, IActivityTracked
{
    public int? UnitId { get; set; }
    public string RfqCode { get; set; } = default!;
    public int RfqStatusId { get; set; }
    public MiscMaster RfqStatus { get; set; } = default!;
    public int? InitiationTypeId { get; set; }
    public MiscMaster InitiationType { get; set; } = default!;
    public int? IndentId { get; set; } // when FromIndent
    public ICollection<RfqItem> Items { get; set; } = new List<RfqItem>();
    public ICollection<RfqSupplier> Suppliers { get; set; } = new List<RfqSupplier>();
    public DateOnly? LastSubmitDate { get; private set; }  
    public ICollection<QuotationHeader> QuotationRfq { get; set; } = new List<QuotationHeader>();
    public ICollection<QuotationComparisonHeader> QuotationrfqConfirmed { get; set; } = new List<QuotationComparisonHeader>();
}


