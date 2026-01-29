using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Domain.Entities.AssetMaster;


namespace FAM.Application.Common.Interfaces.IAssetTransferIssueApproval
{
    public interface IAssetTransferIssueApprovalQueryRepository
    { 
        Task<(List<AssetTransferIssueApprovalDto>, int)> GetAllPendingAssetTransferAsync(
        int PageNumber, 
        int PageSize, 
        string? SearchTerm, 
        DateTimeOffset? FromDate, 
        DateTimeOffset? ToDate);
        Task<List<AssetTransferIssueApproval>> GetByAssetTransferIdAsync(int Id);

    }
}