using FAM.Domain.Entities.AssetPurchase;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost
{
    public interface IAssetAdditionalCostCommandRepository
    {
          Task<int> CreateAsync(AssetAdditionalCost assetAdditionalCost);
          Task<int> UpdateAsync(int id,AssetAdditionalCost assetAdditionalCost);
    }
}