namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation
{
    public interface IAssetLocationQueryRepository
    {
            Task<(List<FAM.Domain.Entities.AssetMaster.AssetLocation>,int)> GetAllAssetLocationAsync(int PageNumber, int PageSize, string? SearchTerm);
          
            Task<FAM.Domain.Entities.AssetMaster.AssetLocation>  GetByIdAsync(int id);
            Task<FAM.Domain.Entities.AssetMaster.AssetLocation?> GetByAssetLocationCodeAsync(int? id = null);

            Task<(List<FAM.Domain.Entities.AssetMaster.Employee>,int)> GetAllCustodianAsync(string OldUnitId, string? SearchTerm);


             Task<List<FAM.Domain.Entities.SubLocation>>  GetSublocationByIdAsync(int id); 

    }
}