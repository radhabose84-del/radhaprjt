namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentCommissionDto
    {
        public int Id { get; set; }
        public int TriggerEventId { get; set; }
        public string? TriggerEventName { get; set; }
        public string? CommissionTypeName { get; set; }
        public string? CommissionBasisName { get; set; }
        public string? ApplicableLevelName { get; set; }
        public string? SlabTypeName { get; set; }
        public string? CommissionSplitName { get; set; }
        public decimal CommissionPercentage { get; set; }
        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset ValidityTo { get; set; }
    }
}
