using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval
{
    public class UpdateAssetTranferIssueApprovalCommand :IRequest<int>, IRequirePermission
    {
        public List<int>? Id { get; set; }
        public string Status { get; set; }

        public UpdateAssetTranferIssueApprovalCommand(List<int> id, string status)
        {
            Id = id;  // Corrected
            Status = status;
        }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
