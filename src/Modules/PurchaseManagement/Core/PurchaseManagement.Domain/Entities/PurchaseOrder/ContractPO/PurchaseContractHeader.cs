using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;

public class PurchaseContractHeader : BaseEntity, IActivityTracked
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrderHeader? PurchaseOrder { get; set; }
    public int ContractPOHeaderId { get; set; }
    public ContractPOHeader? ContractPOHeader { get; set; }
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

    public ICollection<PurchaseContractDetail> Details { get; set; } = new List<PurchaseContractDetail>();
}
