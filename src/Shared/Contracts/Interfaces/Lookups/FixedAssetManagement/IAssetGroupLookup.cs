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
