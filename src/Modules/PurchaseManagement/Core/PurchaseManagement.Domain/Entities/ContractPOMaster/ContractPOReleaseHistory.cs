using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.ContractPOMaster;

public class ContractPOReleaseHistory : BaseEntity, IActivityTracked
{
    public int ContractPOHeaderId { get; set; }
    public ContractPOHeader? ContractPOHeader { get; set; }
    public int ContractPODetailId { get; set; }
    public ContractPODetail? ContractPODetail { get; set; }
    public int ReleasePOId { get; set; }
    public PurchaseOrderHeader? ReleasePurchaseOrder { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public decimal ReleasedQuantity { get; set; }
    public decimal ReleasedRate { get; set; }
    public decimal ReleasedValue { get; set; }
}
