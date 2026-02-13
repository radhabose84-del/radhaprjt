using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.FixedAssetManagement;

namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface IAssetGroupLookup
    {
        Task<IReadOnlyList<AssetGroupLookupDto>> GetByIdsAsync(
            IEnumerable<int> assetGroupIds,
            CancellationToken ct = default);
    }
}
