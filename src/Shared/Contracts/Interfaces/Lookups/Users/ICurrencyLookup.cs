using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICurrencyLookup
    {
        Task<IReadOnlyList<CurrencyLookupDto>> GetByIdsAsync(IEnumerable<int> currencyIds, CancellationToken ct = default);
    }
}
