using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue
{
    public class CreateAssetTransferIssueCommand   : IRequest<int>
    {

      public AssetTransferIssueHdrDto? AssetTransferIssueHdrDto { get; set; }
    }
}