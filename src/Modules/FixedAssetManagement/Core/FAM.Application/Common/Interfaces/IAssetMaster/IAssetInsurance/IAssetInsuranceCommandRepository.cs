namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance
{
    public interface IAssetInsuranceCommandRepository
    {
        Task<FAM.Domain.Entities.AssetMaster.AssetInsurance> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance);   
        Task<bool> UpdateAsync(int id, FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance);
        Task<bool> DeleteAsync(int id, FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance);
         

        
    }
}