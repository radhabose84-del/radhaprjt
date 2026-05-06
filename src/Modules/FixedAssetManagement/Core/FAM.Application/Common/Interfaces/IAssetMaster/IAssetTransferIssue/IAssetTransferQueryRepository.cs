using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId;

namespace FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue
{
  public interface IAssetTransferQueryRepository
  {
    Task<(List<AssetTransferDto>, int)> GetAllAsync(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null);
    Task<List<GetAllTransferDtlDto>> GetAssetTransferByIDAsync(int assetTransferId);
    Task<AssetTransferJsonDto?> GetAssetTransferByIdAsync(int assetTransferId);
    Task<List<GetCategoryByDeptIdDto>> GetCategoriesByDepartmentAsync(int departmentId);
    Task<List<GetAssetMasterDto>> GetAssetsByCategoryAsync(int assetCategoryId, int assetDepartmentId);
    Task<GetAssetDetailsToTransferHdrDto?> GetAssetDetailsToTransferByIdAsync(int assetId);
    // Returns true if the given asset has any AssetTransferIssueHdr in 'Pending' status,
    // or 'Approved' status that has not yet been acknowledged (AckStatus <> 1).
    // Used to block duplicate Asset Transfer creation/update for the same asset (SCRUM-1463).
    // Pass excludeTransferId during Update so the request's own row doesn't trip the guard against itself.
    Task<bool> IsAssetPendingOrApprovedAsync(int assetId, int? excludeTransferId = null);

    // Task<List<GetTransferTypeDto>> GetTransferTypeAsync() ;

    Task<List<FAM.Domain.Entities.MiscMaster>> GetTransferTypeAsync();

    Task<bool> DepartmentSoftDeleteValidation(int departmentId);

    Task<List<GetAssetCustodianDto>> GetCustodianByDepartmentAsync(string oldUnitId, int departmentId);

    Task<List<GetCategoryByCustodianDto>> GetCategoryByCustodianAsync(string custodianId, int departmentId);

    //Task<List<GetAssetDetailsToTransferHdrDto>> GetAssetDetailsToTransferByFiltersAsync(int departmentId, string CategoryId, string custodianId); 
    //Task<GetAssetDetailsToTransferHdrDto> GetAssetDetailsToTransferByFiltersAsync(string custodianIdsCsv, int departmentId, string categoryId);    

    Task<List<GetAssetDetailsToTransferHdrDto>> GetAssetDetailsToTransferByFiltersAsync(string custodianIdsCsv, int departmentId, string categoryIdsCsv);
 
         

           
    }
}