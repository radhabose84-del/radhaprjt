using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IUOMLookup
    {
        Task<List<UOMLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<UOMLookupDto>> GetByIdsAsync(IEnumerable<int> uomIds, CancellationToken ct = default);
    }
}
