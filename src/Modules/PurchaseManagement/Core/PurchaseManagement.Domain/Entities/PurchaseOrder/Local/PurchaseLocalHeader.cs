using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.Local;

public class PurchaseLocalHeader : BaseEntity, IActivityTracked
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrderHeader? PurchaseLocal { get; set; }    
    public bool IsPartialReceiptAllowed { get; set; }
    public int? IncotermsId { get; set; }
    public MiscMaster? MiscIncoterms { get; set; }
    public int? ModeOfDispatchId { get; set; }
    public MiscMaster? MiscModeOfDispatch { get; set; }
    public decimal? FreightCharges { get; set; }
    public int? TermsId { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public ICollection<PurchaseLocalDetail> Details { get; set; } = new List<PurchaseLocalDetail>(); 
}
