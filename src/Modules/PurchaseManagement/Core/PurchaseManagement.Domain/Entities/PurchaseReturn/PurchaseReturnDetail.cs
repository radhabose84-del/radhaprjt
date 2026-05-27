using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.Domain.Entities.PurchaseReturn;

public class PurchaseReturnDetail : BaseEntity
{
    public int PurchaseReturnHeaderId { get; set; }
    public PurchaseReturnHeader PurchaseReturnHeader { get; set; } = null!;

    public int GrnDetailId { get; set; }
    public GrnDetail GrnDetail { get; set; } = null!;

    public int ItemId { get; set; }
    public int UomId { get; set; }

    public decimal ReceivedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal ReturnQty { get; set; }

    public decimal? RatePerUnit { get; set; }
    public decimal? LineValue { get; set; }

    public int? ReturnReasonId { get; set; }
    public ReturnReason? ReturnReason { get; set; }

    public string? LineRemarks { get; set; }
}
