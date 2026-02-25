using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById
{
    public class GetAssetTransferIssueByIdQuery : IRequest<List<AssetTransferIssueByIdDto>>
    {
        public int Id {get; set;}
    }
}