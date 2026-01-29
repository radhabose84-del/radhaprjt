using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty
{
    public interface IAssetWarrantyCommandRepository
    {
        Task<AssetWarranties> CreateAsync(AssetWarranties assetWarranty);
        Task<bool>  UpdateAsync(AssetWarranties assetWarranty);
        Task<int>  DeleteAsync(int assetId,AssetWarranties assetWarranty);        
        Task<bool> ExistsByAssetIdAsync(int? assetId); 
        Task<AssetWarrantyDTO?> GetByAssetCodeAsync(string  assetCode);
        Task<bool> UpdateAssetWarrantyImageAsync(int assetId, string imageName);
         Task<AssetWarrantyDTO?> GetByAssetWarrantyAsync(string? assetCode);
        Task<bool> RemoveAssetWarrantyAsync(string assetPath);
    }
}