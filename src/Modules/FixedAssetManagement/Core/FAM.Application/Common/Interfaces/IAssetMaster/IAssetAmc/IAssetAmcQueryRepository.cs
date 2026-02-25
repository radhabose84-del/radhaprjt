using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc
{
    public interface IAssetAmcQueryRepository
    {
        Task<List<ExistingVendorDetails>> GetVendorDetails(string OldUnitId,string? VendorCode);
        Task<List<FAM.Domain.Entities.MiscMaster>> GetRenewStatusAsync(); 
        Task<List<FAM.Domain.Entities.MiscMaster>> GetCoverageScopeAsync(); 
        Task<AssetAmc?> GetByIdAsync(int Id);
        Task<(List<AssetAmc>,int)> GetAllAssetAmcAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<bool> ActiveAMCValidation(int AssetId, int? id = null);
     

    }
}