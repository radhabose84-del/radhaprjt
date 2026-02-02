using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
public class QuotationHeader : BaseEntity, IActivityTracked
{
    public int UnitId { get; set; }  
    public int RfqId { get; set; }  
    public RfqMaster Rfq { get; set; } = null!;
    public int SupplierId { get; set; }
    public DateOnly ValidTill { get; set; }    
    public string QuotationNumber { get; set; } = string.Empty;   
    public int? FreightModeId { get; set; }  
    public MiscMaster? MiscFreightMode { get; set; }      
    public decimal? Freight { get; set; }
    public int? PaymentTermsId { get; set; }  
    public MiscMaster? MiscPaymentTerms { get; set; }     
    public int? IncotermsId { get; set; }          
    public MiscMaster? MiscIncoterms { get; set; } 
    public decimal? InsuranceCharge { get; set; }
    public decimal TaxableSubtotal { get; set; }
    public decimal GstTotal { get; set; }
    public decimal ItemsTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? QuotationImage { get; set; }
    public ICollection<QuotationDetail> Lines { get; set; } = new List<QuotationDetail>();    
	public ICollection<QuotationComparisonDetail> ConfirmedLines { get; set; } = new List<QuotationComparisonDetail>();   
}