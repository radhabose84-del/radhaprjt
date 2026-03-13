namespace Contracts.Dtos.Lookups.Party;

public sealed class AgentLookupDto
{
    public int Id { get; set; }
    public string AgentCode { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
}
