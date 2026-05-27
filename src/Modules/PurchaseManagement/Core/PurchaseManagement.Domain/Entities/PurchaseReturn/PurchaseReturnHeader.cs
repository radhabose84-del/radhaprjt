using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.PurchaseReturn;

public class PurchaseReturnHeader : BaseEntity
{
    public string RtvNumber { get; set; } = string.Empty;
    public DateOnly RtvDate { get; set; }
    public int UnitId { get; set; }
    public int VendorId { get; set; }

    public int PoId { get; set; }
    public PurchaseOrderHeader Po { get; set; } = null!;

    public int GrnHeaderId { get; set; }
    public GrnHeader GrnHeader { get; set; } = null!;

    public int ReturnTypeId { get; set; }
    public ReturnType ReturnType { get; set; } = null!;

    public int ReturnReasonId { get; set; }
    public ReturnReason ReturnReason { get; set; } = null!;

    public int ReturnActionId { get; set; }
    public MiscMaster ReturnAction { get; set; } = null!;

    public bool IsReplacementRequired { get; set; }
    public bool IsDebitNoteRequired { get; set; }
    public bool IsQcVerified { get; set; }

    public string? Remarks { get; set; }

    public int StatusId { get; set; }
    public MiscMaster MiscStatus { get; set; } = null!;

    public int? ApprovalRequestId { get; set; }

    public int? ReplacementStatusId { get; set; }
    public MiscMaster? ReplacementStatus { get; set; }

    public DateTimeOffset? ReplacementClosedDate { get; set; }

    public ICollection<PurchaseReturnDetail> Details { get; set; } = new List<PurchaseReturnDetail>();
}
