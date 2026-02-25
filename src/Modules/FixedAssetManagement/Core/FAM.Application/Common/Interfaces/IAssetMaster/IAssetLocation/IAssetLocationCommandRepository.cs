namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation
{
    public interface IAssetLocationCommandRepository
    {
        Task<FAM.Domain.Entities.AssetMaster.AssetLocation> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetLocation assetLocation); 

         Task<int>  UpdateAsync(int Id,FAM.Domain.Entities.AssetMaster.AssetLocation assetLocation);
    }
}