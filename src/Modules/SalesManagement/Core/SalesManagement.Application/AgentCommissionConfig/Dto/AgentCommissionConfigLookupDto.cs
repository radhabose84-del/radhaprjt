namespace SalesManagement.Application.AgentCommissionConfig.Dto
{
    public sealed class AgentCommissionConfigLookupDto
    {
        public int Id { get; set; }
        public string? AgentName { get; set; }
        public string? ItemName { get; set; }
        public string? SegmentName { get; set; }
    }
}
