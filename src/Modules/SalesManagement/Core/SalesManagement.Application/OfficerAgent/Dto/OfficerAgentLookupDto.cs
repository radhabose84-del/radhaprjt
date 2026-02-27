namespace SalesManagement.Application.OfficerAgent.Dto
{
    public sealed class OfficerAgentLookupDto
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? OfficerName { get; set; }
    }
}
