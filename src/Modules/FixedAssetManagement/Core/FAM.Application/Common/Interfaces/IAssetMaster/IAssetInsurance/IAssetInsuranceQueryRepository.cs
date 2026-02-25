namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance
{
    public interface IAssetInsuranceQueryRepository 
    {
            Task<FAM.Domain.Entities.AssetMaster.AssetInsurance>  GetByAssetIdAsync(int id );            

            Task<(List<FAM.Domain.Entities.AssetMaster.AssetInsurance>,int)> GetAllAssetInsuranceAsync(int PageNumber, int PageSize, string? SearchTerm);
            Task<bool> AlreadyExistsAsync(string PolicyNo, int? id = null);
             Task<bool> ActiveInsuranceValidation(int AssetId, int? id = null);
           
           
        
    }
}