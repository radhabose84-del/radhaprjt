using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferIssueCommand  : IRequest<bool>, IRequirePermission
    {
       
       public UpdateAssetTransferHdrDto? AssetTransferHdr  { get; set; }
       public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
