using Contracts.Dtos.Lookups.FixedAssetManagement;

namespace Contracts.Interfaces.Lookups.FixedAssetManagement
{
    public interface IAssetSpecificationLookup
    {
        Task<List<AssetSpecificationLookupDto>> GetAllAssetSpecificationAsync();
        Task<List<AssetSpecificationLookupDto>> GetByAssetIdAsync(int assetId, CancellationToken ct = default);
    }
}
