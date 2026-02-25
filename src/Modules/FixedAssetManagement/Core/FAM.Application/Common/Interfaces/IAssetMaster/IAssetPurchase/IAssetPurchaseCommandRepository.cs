using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetPurchase
{
    public interface IAssetPurchaseCommandRepository
    {
        Task<int> CreateAsync(AssetPurchaseDetails assetPurchaseDetails);
        Task<int> UpdateAsync(int Id,AssetPurchaseDetails assetPurchaseDetails);
    }
}