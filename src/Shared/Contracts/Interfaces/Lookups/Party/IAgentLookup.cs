using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party;

public interface IAgentLookup
{
    Task<IReadOnlyList<AgentLookupDto>> GetAllAgentAsync();
}
