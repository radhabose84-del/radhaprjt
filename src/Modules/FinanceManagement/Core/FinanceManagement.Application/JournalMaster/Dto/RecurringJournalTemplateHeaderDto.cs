namespace FinanceManagement.Application.JournalMaster.Dto
{
    public class RecurringJournalTemplateHeaderDto
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public int VoucherTypeId { get; set; }
        public string? VoucherTypeCode { get; set; }
        public string? VoucherTypeName { get; set; }
        public int FrequencyId { get; set; }
        public string? FrequencyName { get; set; }          // MiscMaster (RECURRING_FREQUENCY)
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AutoPost { get; set; }
        public int AmountAdjustmentRuleId { get; set; }
        public string? AmountAdjustmentRuleName { get; set; }   // MiscMaster (AMOUNT_ADJ_RULE)
        public bool LowRisk { get; set; }

        public List<RecurringJournalTemplateDetailDto> Lines { get; set; } = new();

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
