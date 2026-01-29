using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty
{
    public interface IAssetWarrantyQueryRepository
    {
        Task<AssetWarrantyDTO>  GetByIdAsync(int assetId);
        Task<(List<AssetWarrantyDTO>,int)> GetAllAssetWarrantyAsync(int PageNumber, int PageSize, string? SearchTerm);        
        Task<List<AssetWarrantyDTO>> GetByAssetWarrantyNameAsync(string assetName);  
        Task<List<FAM.Domain.Entities.MiscMaster>> GetWarrantyTypeAsync();    
        Task<List<FAM.Domain.Entities.MiscMaster>> GetWarrantyClaimStatusAsync();  
        Task<bool> SoftDeleteValidation(int Id);   
        Task<string> GetBaseDirectoryAsync();
    }
}