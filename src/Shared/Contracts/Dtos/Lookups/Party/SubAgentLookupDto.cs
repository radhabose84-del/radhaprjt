namespace Contracts.Dtos.Lookups.Party;

public sealed class SubAgentLookupDto
{
    public int Id { get; set; }
    public string SubAgentCode { get; set; } = string.Empty;
    public string SubAgentName { get; set; } = string.Empty;
}
