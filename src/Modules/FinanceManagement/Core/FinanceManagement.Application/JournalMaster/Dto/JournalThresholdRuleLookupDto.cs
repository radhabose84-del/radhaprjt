namespace FinanceManagement.Application.JournalMaster.Dto
{
    public sealed class JournalThresholdRuleLookupDto
    {
        public int Id { get; set; }
        public int RuleTypeId { get; set; }
        public string? RuleTypeName { get; set; }
    }
}
