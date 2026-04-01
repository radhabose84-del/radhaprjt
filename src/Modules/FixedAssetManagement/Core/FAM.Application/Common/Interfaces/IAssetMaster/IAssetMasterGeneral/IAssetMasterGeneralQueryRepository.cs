using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral
{
    public interface IAssetMasterGeneralQueryRepository
    {
        Task<AssetMasterGeneralDTO>  GetByIdAsync(int assetId);
        Task<AssetMasterGeneralDTO>  GetByParentIdAsync(int assetTypeId);
        Task<(List<AssetMasterGeneralDTO>,int)> GetAllAssetAsync(int PageNumber, int PageSize, string? SearchTerm);        
        Task<List<AssetMasterGeneralDTO>> GetByAssetNameAsync(string assetName);    
        Task<List<FAM.Domain.Entities.MiscMaster>> GetAssetTypeAsync();     
        Task<List<FAM.Domain.Entities.MiscMaster>> GetWorkingStatusAsync();       
        Task<bool> GetAssetChildDetails(int assetId);
        Task<string?> GetLatestAssetCode(int assetGroupId, int assetCategoryId,int DepartmentId,int LocationId);
        Task<string> GetBaseDirectoryAsync();
        Task<List<FAM.Domain.Entities.MiscMaster>> GetAssetPattern();
        Task<(dynamic AssetResult, dynamic LocationResult, IEnumerable<dynamic> PurchaseDetails, IEnumerable<dynamic> Spec, IEnumerable<dynamic> Warranty,IEnumerable<dynamic> Amc,dynamic Disposal, IEnumerable<dynamic> Insurance , IEnumerable<dynamic> AdditionalCost)> GetAssetMasterByIdAsync(int assetId);
        Task<(dynamic AssetResult, dynamic LocationResult, IEnumerable<dynamic> PurchaseDetails,  IEnumerable<dynamic> AdditionalCost)> GetAssetMasterSplitByIdAsync(int assetId);
        //Task<(string CompanyName, string UnitName)> GetCompanyUnitAsync(int companyId,int unitId);           
        Task<string> GetDocumentDirectoryAsync();

        /// <summary>Inactivate guard: checks if any active child records reference this asset.</summary>
        Task<bool> IsAssetMasterLinkedAsync(int assetId);
    }
}