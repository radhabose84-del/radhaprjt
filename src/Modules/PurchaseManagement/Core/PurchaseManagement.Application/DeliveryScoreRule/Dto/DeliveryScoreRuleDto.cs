namespace PurchaseManagement.Application.DeliveryScoreRule.Dto;

public class DeliveryScoreRuleDto
{
    public int Id { get; set; }
    public string? RuleCode { get; set; }
    public string? Description { get; set; }
    public int DelayDaysFrom { get; set; }
    public int DelayDaysTo { get; set; }
    public decimal Score { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
}
