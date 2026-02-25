using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Interfaces.IAssetTransferIssueApproval
{
    public interface IAssetTransferIssueApprovalCommandRepository
    {
        Task<List<AssetTransferIssueHdr>> GetByIdsAsync(List<int> ids);
      //  Task<int> UpdateRangeAsync(List<AssetTransferIssueHdr> transfers);
        Task<int> ExecuteBulkUpdateAsync(List<int> ids, string status, int userId, DateTimeOffset currentTime, string username, string currentIp);
    }
}