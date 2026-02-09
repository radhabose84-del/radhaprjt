using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Maintenance;

namespace Contracts.Interfaces.Lookups.Maintenance
{
    public interface ICostCenterLookup
    {
        Task<CostCenterLookupDto?> GetByIdAsync(int costCenterId, CancellationToken ct = default);
        Task<IReadOnlyList<CostCenterLookupDto>> GetByIdsAsync(IEnumerable<int> costCenterIds, CancellationToken ct = default);
        Task<List<CostCenterLookupDto>> GetAllCostCentersAsync();
    }
}
