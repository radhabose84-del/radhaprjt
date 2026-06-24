namespace FinanceManagement.Application.JournalMaster.Dto
{
    public class JournalThresholdRuleDto
    {
        public int Id { get; set; }
        public int RuleTypeId { get; set; }
        public string? RuleTypeName { get; set; }           // MiscMaster (THRESHOLD_RULE_TYPE)
        public decimal? ThresholdValue { get; set; }
        public bool Active { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
