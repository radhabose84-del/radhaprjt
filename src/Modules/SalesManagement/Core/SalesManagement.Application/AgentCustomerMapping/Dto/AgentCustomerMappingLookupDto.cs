namespace SalesManagement.Application.AgentCustomerMapping.Dto
{
    public sealed class AgentCustomerMappingLookupDto
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? AgentName { get; set; }
    }
}
