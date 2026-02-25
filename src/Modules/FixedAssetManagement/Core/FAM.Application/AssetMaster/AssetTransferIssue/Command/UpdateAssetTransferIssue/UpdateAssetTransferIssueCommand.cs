using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferIssueCommand  : IRequest<bool>
    {
       
       public UpdateAssetTransferHdrDto? AssetTransferHdr  { get; set; }
    }
}