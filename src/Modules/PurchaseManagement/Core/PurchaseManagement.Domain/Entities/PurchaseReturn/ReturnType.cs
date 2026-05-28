using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseReturn;

public class ReturnType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? InventoryImpactId { get; set; }
    public MiscMaster? InventoryImpact { get; set; }

    public int? FinanceImpactId { get; set; }
    public MiscMaster? FinanceImpact { get; set; }

    public bool IsReplacementApplicable { get; set; }
    public bool IsQcMandatory { get; set; }

    public string? ApprovalRoleCode { get; set; }

    public ICollection<ReturnReason> Reasons { get; set; } = new List<ReturnReason>();
    public ICollection<PurchaseReturnHeader> PurchaseReturns { get; set; } = new List<PurchaseReturnHeader>();
}
