using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.FixedAssetManagement;

namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface IAssetSpecificationLookup
    {
        Task<List<AssetSpecificationLookupDto>> GetAllAssetSpecificationAsync();
        Task<List<AssetSpecificationLookupDto>> GetByAssetIdAsync(int assetId, CancellationToken ct = default);
    }
}
