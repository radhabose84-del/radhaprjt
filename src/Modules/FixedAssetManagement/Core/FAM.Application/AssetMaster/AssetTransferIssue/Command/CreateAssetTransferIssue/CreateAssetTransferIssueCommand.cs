using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue
{
    public class CreateAssetTransferIssueCommand   : IRequest<int>, IRequirePermission
    {

      public AssetTransferIssueHdrDto? AssetTransferIssueHdrDto { get; set; }
      public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
