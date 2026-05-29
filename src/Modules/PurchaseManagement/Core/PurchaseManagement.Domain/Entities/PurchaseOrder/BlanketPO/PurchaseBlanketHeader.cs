using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.BlanketMaster;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;

public class PurchaseBlanketHeader : BaseEntity, IActivityTracked
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrderHeader? PurchaseOrder { get; set; }
    public int BlanketHeaderId { get; set; }
    public BlanketHeader? BlanketHeader { get; set; }
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

    public ICollection<PurchaseBlanketDetail> Details { get; set; } = new List<PurchaseBlanketDetail>();
}
