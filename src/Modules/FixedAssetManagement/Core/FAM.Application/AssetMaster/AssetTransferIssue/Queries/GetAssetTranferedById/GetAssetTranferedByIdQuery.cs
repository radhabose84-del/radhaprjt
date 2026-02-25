using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTranferedById
{
    public class GetAssetTranferedByIdQuery  : IRequest<AssetTransferJsonDto>
    {       
        public int AssetTransferId { get; set; }

            
    }
}