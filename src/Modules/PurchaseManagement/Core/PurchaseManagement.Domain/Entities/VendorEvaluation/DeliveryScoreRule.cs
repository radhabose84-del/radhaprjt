using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.VendorEvaluation;

public class DeliveryScoreRule : BaseEntity
{
    public string? RuleCode { get; set; }
    public string? Description { get; set; }
    public int DelayDaysFrom { get; set; }
    public int DelayDaysTo { get; set; }
    public decimal Score { get; set; }
    public int SortOrder { get; set; }
}
