using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    public interface IPartyBankLookup
    {
        Task<PartyBankLookupDto?> GetDefaultBankByGstAsync(string gstNumber, CancellationToken ct = default);
    }
}
