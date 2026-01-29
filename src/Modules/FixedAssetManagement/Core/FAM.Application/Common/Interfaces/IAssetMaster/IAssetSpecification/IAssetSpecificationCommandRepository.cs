using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification
{
    public interface IAssetSpecificationCommandRepository
    {
        Task<AssetSpecifications> CreateAsync(AssetSpecifications assetSpecification);
        Task<int>  UpdateAsync(int assetId,AssetSpecifications assetSpecification);
        Task<int>  DeleteAsync(int assetId,AssetSpecifications assetSpecification);        
        Task<bool> ExistsByAssetSpecIdAsync(int? assetId,int? assetSpecId); 
    }
}