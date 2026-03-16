using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party;

public interface ISubAgentLookup
{
    Task<IReadOnlyList<SubAgentLookupDto>> GetAllSubAgentAsync();
}
