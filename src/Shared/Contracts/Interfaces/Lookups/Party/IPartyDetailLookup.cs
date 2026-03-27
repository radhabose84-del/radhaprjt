using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    public interface IPartyDetailLookup
    {
        Task<PartyDetailLookupDto?> GetByIdAsync(int partyId, CancellationToken ct = default);
    }
}
