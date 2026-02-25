using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IHSNLookup
    {
        Task<List<HSNLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<HSNLookupDto>> GetByIdsAsync(IEnumerable<int> hsnIds, CancellationToken ct = default);
    }
}
