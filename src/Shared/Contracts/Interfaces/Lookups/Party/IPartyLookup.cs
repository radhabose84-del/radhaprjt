using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    public interface IPartyLookup
    {
        Task<PartyLookupDto?> GetByIdAsync(int partyId, CancellationToken ct = default);
        Task<IReadOnlyList<PartyLookupDto>> GetByIdsAsync(IEnumerable<int> partyIds, CancellationToken ct = default);
    }
}
