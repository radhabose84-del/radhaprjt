using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseReturn;

public class ReturnReason : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int ReturnTypeId { get; set; }
    public ReturnType ReturnType { get; set; } = null!;

    public bool? IsReplacementOverride { get; set; }
    public bool? IsDebitNoteOverride { get; set; }
    public bool? IsQcMandatoryOverride { get; set; }

    public ICollection<PurchaseReturnHeader> PurchaseReturns { get; set; } = new List<PurchaseReturnHeader>();
    public ICollection<PurchaseReturnDetail> PurchaseReturnDetailReasons { get; set; } = new List<PurchaseReturnDetail>();
}
