namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentCustomerDto
    {
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public int? SalesSegmentId { get; set; }
        public string? SalesSegmentName { get; set; }
        public DateTimeOffset? EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public bool IsDefaultAgent { get; set; }
    }
}
